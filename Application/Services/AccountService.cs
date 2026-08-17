using BackendJX3D.Application.DTOs.Request.Account;
using BackendJX3D.Application.DTOs.Response.Account;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using BackendJX3D.Core.Utils;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.External;
using BackendJX3D.Infrastructure.Session;
using BackendJX3D.Infrastructure.Session.Data;
using Network.Bishop;
using Network.GameServer;
using Network.Header;
using Network.Resource.Header;

namespace BackendJX3D.Application.Services;

public class AccountService : IAccountService
{
    private readonly ISessionManager _sessionManager;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUser _currentUser;
    private readonly IJwtService _jwt;

    public AccountService(ISessionManager sessionManager, IConfiguration config, ICurrentUser currentUser, IJwtService jwt)
    {
        _sessionManager=sessionManager;
        _configuration = config;
        _currentUser = currentUser;
        _jwt=jwt;
    }

    public async Task<LoginResponse> LoginAccount(LoginRequest request)
    {
        var region = KServerManager
            .GetServerList()
            .Regions
            .FirstOrDefault(x => x.GetKeyName() == request.RegionKey);

        if (region.ServerCount <= 0)
            throw new BaseException.NotFoundException("not_found", "Region không tồn tại");

        if (request.ServerKey < 0 || request.ServerKey >= region.ServerCount)
            throw new BaseException.NotFoundException("not_found", "Server không tồn tại");

        var serverInfo = region.Servers[request.ServerKey];
        
        var handler = new GameSession();

        var processGame = new ProcessGame(handler);

        var bishop = new BishopSession(processGame);
        
        var gameServer = new GameServerSession(processGame);
        
        handler.Initialize(bishop, () => gameServer);
        
        await bishop.Client.ConnectAsync(serverInfo.GetAddress(), serverInfo.nPort);
        //
        // bishop.resultCode = -1;
        await Task.Delay(1000);
        await BS_ClientSend.PlayerLoginInfo(
            bishop.Client,
            request.Username,
            request.Password);

        var timeout = TimeSpan.FromSeconds(5);
        var start = DateTime.UtcNow;

        while (bishop.resultCode == -1 &&
               DateTime.UtcNow - start < timeout)
        {
            await Task.Delay(50);
        }

        if (bishop.resultCode == -1)
        {
            bishop.Client.Dispose();
            throw new TimeoutException("Đăng nhập quá thời gian.");
        }

        if (bishop.resultCode != BishopProtocolDef.LOGIN_R_SUCCESS)
        {
            bishop.Client.Dispose();

            throw new BaseException.NotFoundException(
                "not_found",
                GameSession.GetLoginMessage(bishop.resultCode));
        }

        var sessionId = Guid.NewGuid().ToString("N");

        var session = new PlayerSession
        {
            Bishop = bishop,
            GameServer = gameServer,
            Handler = handler
        };

        _sessionManager.Add(sessionId, session);

        var token = _jwt.GenerateToken(sessionId, request.Username);
        var expireTime = _configuration.GetValue<int>("Jwt:ExpireHours");
        
        return new LoginResponse
        {
            Token = token,
            ExpireTime = expireTime,
        };
    }

    public async Task<List<CharacterResponse>> GetCharacters()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);

        if (!session.Bishop.isLoadFullRoleBase)
        {
            var timeout = TimeSpan.FromSeconds(5);
            var start = DateTime.UtcNow;

            while (!session.Bishop.isLoadFullRoleBase &&
                   DateTime.UtcNow - start < timeout)
            {
                await Task.Delay(100);
            }

            if (!session.Bishop.isLoadFullRoleBase)
            {
                throw new BaseException.NotFoundException("not_found", "Timeout waiting for role list to be loaded.");
            }
        }

        return session.Bishop.Roles
            .Select(r => new CharacterResponse
            {
                Name = r.GetName(),
                Level = r.Level,
                Faction = r.Faction,
                Series = r.Series,
                Flag = r.Flag,
                RolePrimKindNo = r.cRolePrimKindNo,
                TongName = r.GetTongName(),
                LastLoginTime = r.GetLastLoginTime()
            })
            .ToList();
    }



    public async Task<CreateCharacterResponse> CreateCharacter(string charName, byte byRoleNo, ushort wPortraitID)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);

        var bishop = session.Bishop;

        if (bishop == null || bishop.Client == null || !bishop.Client.IsConnected)
            throw new BaseException.ConflictException(
                "bishop_disconnected",
                "Phiên Bishop đã đóng nên không tạo được nhân vật. "
                + "Vào game là socket Bishop bị đóng, phải đăng nhập lại mới tạo tiếp được.");

        ValidateCharName(charName);
        ValidateGenderAndSeries(byRoleNo, wPortraitID);
        
        bishop.RoleCommand = null;
        bishop.LoginServerResultCode = -1;

        await BS_ClientSend.CreateRole(bishop.Client, charName, byRoleNo, wPortraitID);

        var timeout = TimeSpan.FromSeconds(5);
        var start = DateTime.UtcNow;

        while (bishop.RoleCommand == null &&
               DateTime.UtcNow - start < timeout)
        {
            await Task.Delay(50);
        }

        var result = bishop.RoleCommand;

        if (result == null)
            throw new BaseException.ErrorException(
                504,
                "bishop_timeout",
                "Bishop không phản hồi lệnh tạo nhân vật.");

        if (!result.Succeeded)
            throw new BaseException.ErrorException(
                422,
                "create_character_rejected",
                $"Bishop từ chối tạo nhân vật '{charName}', mã lỗi {result.FailReason}.");

        var response = new CreateCharacterResponse
        {
            // Lấy tên trong gói phản hồi, đó là tên Bishop thật sự đã tạo
            Name = string.IsNullOrEmpty(result.Name) ? charName : result.Name,
            Gender = byRoleNo,
            Series = (byte)wPortraitID,
        };

        await WaitAutoEnterGame(bishop, response);

        return response;
    }


    private static async Task WaitAutoEnterGame(BishopSession bishop, CreateCharacterResponse response)
    {
        var timeout = TimeSpan.FromSeconds(5);
        var start = DateTime.UtcNow;

        while (bishop.LoginServerResultCode == -1 &&
               DateTime.UtcNow - start < timeout)
        {
            await Task.Delay(50);
        }

        if (bishop.LoginServerResultCode == -1)
            return;

        if (bishop.LoginServerResultCode != BishopProtocolDef.ROLE_LOGIN_RESULT_SUCCESS)
        {
            response.EnterGameMessage =
                GameSession.GetLoginServerMessage(bishop.LoginServerResultCode);

            return;
        }

        response.EnteredGame = true;

        response.CharacterListStale = true;

        // Ping đã tự bật trong GameSession.ReturnNotifyClient sau khi GS kết nối.
    }



    private static void ValidateGenderAndSeries(byte byRoleNo, ushort wPortraitID)
    {
        if (byRoleNo > 1)
            throw new BaseException.BadRequestException(
                "gender_invalid",
                $"Giới tính chỉ nhận 0 hoặc 1.");

        if (wPortraitID > 14)
            throw new BaseException.BadRequestException(
                "series_invalid",
                $"Hệ chỉ nhận 0..4.");
    }

    private static void ValidateCharName(string charName)
    {
        if (string.IsNullOrWhiteSpace(charName))
            throw new BaseException.BadRequestException(
                "char_name_empty",
                "Tên nhân vật rỗng.");

        if (charName.Length > 31)
            throw new BaseException.BadRequestException(
                "char_name_too_long",
                $"Tên nhân vật tối đa 31 ký tự.");


        foreach (var c in charName)
        {
            if (c <= 0x7F && !char.IsControl(c)) continue;

            throw new BaseException.BadRequestException(
                "char_name_not_ascii",
                "Tên nhân vật chỉ được dùng chữ và số không dấu.");
        }
    }

    public async Task<string> LoginServerAccount(LoginServerRequest request)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);

        session.Bishop.LoginServerResultCode = -1;

        await BS_ClientSend.GameLoginRequest(session.Bishop.Client, request.CharacterName, 3435973836);

        var timeout = TimeSpan.FromSeconds(5);
        var start = DateTime.UtcNow;

        while (session.Bishop.LoginServerResultCode == -1 &&
               DateTime.UtcNow - start < timeout)
        {
            await Task.Delay(50);
        }

        if (session.Bishop.LoginServerResultCode == -1)
        {
            throw new TimeoutException("Đăng nhập game server quá thời gian.");
        }

        if (session.Bishop.LoginServerResultCode != BishopProtocolDef.ROLE_LOGIN_RESULT_SUCCESS)
        {
            throw new BaseException.NotFoundException(
                "not_found",
                GameSession.GetLoginServerMessage(session.Bishop.LoginServerResultCode));
        }

        return "Success";
    }
    
    private async Task SavePlayerConfig(PlayerState state)
    {
        var config = state.Config;

        if (config == null || state.uuId == 0)
        {
            Console.WriteLine($"[Config] không có gì để lưu (uuId={state.uuId}, config={(config == null ? "null" : "có")})");
            return;
        }

        try
        {
            var saved = await PlayerConfigClient.SaveAsync(state.uuId, config);

            Console.WriteLine(saved
                ? $"[Config] đã lưu cấu hình uuId={state.uuId}"
                : $"[Config] API ngoài từ chối lưu cấu hình uuId={state.uuId}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Config] lỗi lưu cấu hình uuId={state.uuId}: {e.Message}");
        }
    }

    public async Task<string> LogoutServerAccount()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);

        if (session == null! || session.GameServer == null! || session.GameServer.Client == null!)
            throw new BaseException.NotFoundException("not_found", "null");
        
        Console.WriteLine("LogoutServerAccount Name: " + session.Handler.State.Name);

        await SavePlayerConfig(session.Handler.State);

        session.GameServer.StopPing();
        session.GameServer?.GetSender()?.SendLogoutPacket(session.Handler.State.Name);
        session.Bishop.Client = null!;
        
        var old = session.GameServer?.Client;
        session.GameServer!.Client = null!;
        if (old == null) throw new BaseException.NotFoundException("not_found", "null");
        _ = Task.Run(() =>
        {
            try { old.Close(); } catch (Exception e) { Console.WriteLine($"[Bishop] Close old client failed: {e.Message}"); }
            try { old.Dispose(); } catch (Exception e) { Console.WriteLine($"[Bishop] Dispose old client failed: {e.Message}"); }
        });
        
        _sessionManager.Remove(_currentUser.SessionId);
        
        
        return await Task.FromResult("Success");
    }

    public async Task<string> LogoutBishop()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);

        if (session == null! || session.Bishop == null)
            throw new BaseException.NotFoundException("not_found", "null");
        
        Console.WriteLine("LogOut Bishop");
        
        session.Bishop.Client.Close();
        session.Bishop.Client.Dispose();
        session.Bishop = null!;
        _sessionManager.Remove(_currentUser.SessionId);
        
        return await Task.FromResult("Success");
    }

    
}
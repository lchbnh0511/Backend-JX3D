using BackendJX3D.Application.DTOs.Request.Account;
using BackendJX3D.Application.DTOs.Response.Account;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using BackendJX3D.Core.Utils;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;
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

        // Vào game xong -> ping game server 3s/lần để giữ kết nối
        session.GameServer.StartPing();

        return "Success";
    }
    
    public async Task<string> LogoutServerAccount()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);

        if (session == null! || session.GameServer == null! || session.GameServer.Client == null!)
            throw new BaseException.NotFoundException("not_found", "null");
        
        Console.WriteLine("LogoutServerAccount Name: " + session.Handler.State.Name);
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
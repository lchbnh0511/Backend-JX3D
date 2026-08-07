using BackendJX3D.Application.DTOs.Request.Account;
using BackendJX3D.Application.DTOs.Response.Account;
using BackendJX3D.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using BackendJX3D.Core.Base;
using Microsoft.AspNetCore.Authorization;

namespace BackendJX3D.API.Controllers
{
    [ApiController]
    [Route("api/v1/account")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService accountService;

        public AccountController(IAccountService service)
        {
            accountService = service;
        }

        [HttpPost("login")]
        public async Task<ActionResult<BaseResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            var result = await accountService.LoginAccount(request);
            return Ok(BaseResponse<LoginResponse>.OkResponse(result, "Đăng nhập thành công."));
        }
        
        [Authorize]
        [HttpGet("characters")]
        public async Task<ActionResult<BaseResponse<List<CharacterResponse>>>> GetCharacters()
        {
            var result = await accountService.GetCharacters();
            return Ok(BaseResponse<List<CharacterResponse>>.OkResponse(result, "Lấy danh sách nhân vật thành công."));
        }
        
        [Authorize]
        [HttpPost("logout-bishop")]
        public async Task<ActionResult<BaseResponse<string>>> LogoutBishop()
        {
            await accountService.LogoutBishop();
            return Ok(BaseResponse<string>.OkResponse("Đăng xuất Bishop thành công."));
        }
        
        [Authorize]
        [HttpPost("login-server")]
        public async Task<ActionResult<BaseResponse<string>>> Login([FromBody] LoginServerRequest request)
        {
            var result = await accountService.LoginServerAccount(request);
            return Ok(BaseResponse<string>.OkResponse("Đăng nhập thành công."));
        } 
        
        [Authorize]
        [HttpPost("logout-server")]
        public async Task<ActionResult<BaseResponse<string>>> Logout()
        {
            await accountService.LogoutServerAccount();
            return Ok(BaseResponse<string>.OkResponse("Đăng xuất thành công."));
        }
    }
}
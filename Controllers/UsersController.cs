using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebApi.Services;
using WebApi.Models;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IProductService _productService;
        private ITokenService _tokenService;

        public UsersController(IUserService userService, IProductService productService, ITokenService tokenService)
        {
            _userService = userService;
            _productService = productService;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });
            
            TokenModel token = _tokenService.Generate(user);
            Response.Headers.Add("Token", token.Token);
            Response.Headers.Add("Token-Expiry", token.Expires.Split('.')[0]);
            Response.Headers.Add("Refresh-Token", token.RefreshToken);
            Response.Headers.Add("Refresh-Token-Expiry", token.RefreshTokenExpires.Split('.')[0]);
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("refresh")]
        public IActionResult Refresh()
        {
            Microsoft.Extensions.Primitives.StringValues token;
            Microsoft.Extensions.Primitives.StringValues refreshToken;
            Request.Headers.TryGetValue("Authorization", out token);
            Request.Headers.TryGetValue("Refresh-Token", out refreshToken);
            if(token.Count > 0 && refreshToken.Count > 0){
                var NewToken = _tokenService.Generate(token.ToArray()[0].Split(' ')[1], refreshToken.ToArray()[0]);
            
                Response.Headers.Add("Token", NewToken.Token);
                Response.Headers.Add("Token-Expiry", NewToken.Expires.Split('.')[0]);
                Response.Headers.Add("Refresh-Token", NewToken.RefreshToken);
                Response.Headers.Add("Refresh-Token-Expiry", NewToken.RefreshTokenExpires.Split('.')[0]);

                return Ok();
            }

            return BadRequest(new { message = "Refresh token and token are required" });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpGet]
        [Route("products")]
        public IActionResult GetAllProducts()
        {
            var users = _productService.GetAll();
            return Ok(users);
        }
    }
}

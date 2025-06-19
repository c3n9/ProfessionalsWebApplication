using Microsoft.AspNetCore.Mvc;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Models.DTO;
using ProfessionalsWebApplication.Services;

namespace ProfessionalsWebApplication.Controllers;


[ApiController]
[Route("[controller]")]
public class AuthorizationController : ControllerBase
{
    private readonly JwtService _jwtService;
    
    public AuthorizationController(JwtService jwtService)
    {
        _jwtService = jwtService;
    }
    
    [HttpPost("login")]
    public IActionResult Login([FromBody] AdminDto admin)
    {
        if (admin.UserName == "admin" && admin.Password == "1Ltk-Lzrar")
        {
            var token = _jwtService.GenerateToken(admin.UserName);
            return Ok(new { token });
        }
        return Unauthorized();
    }
}
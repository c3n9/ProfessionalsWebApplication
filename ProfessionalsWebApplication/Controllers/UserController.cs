using Microsoft.AspNetCore.Mvc;
using ProfessionalsWebApplication.Models;

namespace ProfessionalsWebApplication.Controllers
{
	[Route("api/Users")]
	[ApiController]
	public class UserController : Controller
	{
		private readonly ProfessionalsDbContext _context;

		public UserController(ProfessionalsDbContext context)
		{
			_context = context;
		}

		[HttpPost]
		public async Task<IActionResult> SaveUser([FromBody] User user)
		{
			return Ok();
		}

	}
}

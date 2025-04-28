using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Services;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ProfessionalsWebApplication.Controllers
{
	[Route("forms")]
	[ApiController]
	public class FormController : Controller
	{
		private readonly ProfessionalsDbContext _context;

		public FormController(ProfessionalsDbContext context)
		{
			_context = context;
		}

		[HttpGet("{hash}")]
		public IActionResult GetForm(string hash)
		{
			// Заглушка для данных формы (обычно загружается из БД)
			var form = _context.Forms.Include(f => f.Questions).FirstOrDefault(x => x.Hash == hash);
			if (form == null)
			{
				Response.StatusCode = 404;
				return View("NotFoundView");
			}
			return View("FormView", form);
		}

		[HttpPost("submit")]
		public async Task<IActionResult> SubmitForm([FromBody] dynamic submission)
		{
			var serializedData = JsonSerializer.Serialize(new
			{
				FormData = submission,
				Timestamp = DateTime.UtcNow,
			}, new JsonSerializerOptions
			{
				WriteIndented = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			});

			Console.WriteLine(serializedData);

			HttpContext.Session.SetString("FormSubmitted", "true");

			return Json(new { redirectUrl = Url.Action("thank-you", "forms") });
		}

		[HttpGet("thank-you")]
		public IActionResult ThankYou()
		{
			var isSubmitted = HttpContext.Session.GetString("FormSubmitted");
			HttpContext.Session.Remove("FormSubmitted");
			if (isSubmitted == "true")
				return View("ThankYouView");
			else
				return View("NotFoundView");
		}


	}
}

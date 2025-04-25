using Microsoft.AspNetCore.Mvc;
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
		[HttpGet("{hash}")]
		public IActionResult GetForm(string hash)
		{
			// Заглушка для данных формы (обычно загружается из БД)
			var formDataJson = System.IO.File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "questions.json"));
			var form = (JsonSerializer.Deserialize<List<FormModel>>(formDataJson)).FirstOrDefault(x => x.Hash == hash);
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

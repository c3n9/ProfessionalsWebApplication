using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Services;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using ProfessionalsWebApplication.Enums;
using ProfessionalsWebApplication.Models.DTO;

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
			var form = _context.Forms.Where(x => x.DateStart.Date <= DateTime.Now.Date && x.DateEnd.Date >= DateTime.Now.Date).Include(f => f.Questions).ToList().FirstOrDefault(x => x.Hash == hash);
			if (form == null)
			{
				Response.StatusCode = 404;
				return View("NotFoundView");
			}
			return View("FormView", form);
		}
		
		[HttpPost("submit")]
		public async Task<IActionResult> SubmitFormDesctop([FromBody] EncryptedSubmissionDto encryptSubmission)
		{
			var submission = CryptoService.Decrypt(encryptSubmission.Data);
			var root = JsonNode.Parse(submission);
			string formId = root?["FormId"]?.ToString();
			string answersNode = root?["Answers"].ToString();

			var newList = MigrateOldJson(answersNode);
			var newUser = new User()
			{
				FormId = formId,
				Answers = newList,
				Timestamp = DateTime.Now,
			};
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
		
		public static List<Answer> MigrateOldJson(string oldJson)
		{
			var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(oldJson);
			var result = new List<Answer>();

			foreach (var pair in dict)
			{
				if (pair.Value.ValueKind == JsonValueKind.Object)
				{
					var file = pair.Value.Deserialize<FileAnswer>();
					result.Add(new Answer
					{
						Question = pair.Key,
						Type = AnswerType.File,
						File = file
					});
				}
				else
				{
					result.Add(new Answer
					{
						Question = pair.Key,
						Type = AnswerType.Text,
						Value = pair.Value.GetString()
					});
				}
			}

			return result;
		}



	}
}

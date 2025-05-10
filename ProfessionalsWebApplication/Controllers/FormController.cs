using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Services;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using ProfessionalsWebApplication.Enums;
using ProfessionalsWebApplication.Models.DTO;
using System.Security.Cryptography;
using System.Text;



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
			var decryptedKeyData = CryptoService.Decrypt(encryptSubmission.Key);
			var newUser = new User()
			{
				FormId = encryptSubmission.FormId,
				AnswersJson = encryptSubmission.Data,
				Timestamp = DateTime.Now,
			};
			var answers = newUser.Answers;
			var keyInfo = JsonSerializer.Deserialize<AesKeyInfo>(decryptedKeyData);
			var decryptedData = DecryptAes(Convert.FromBase64String(answers[0].Value), Convert.FromBase64String(keyInfo.Key), Convert.FromBase64String(keyInfo.Iv));
			HttpContext.Session.SetString("FormSubmitted", "true");
			return Json(new { redirectUrl = Url.Action("thank-you", "forms") });
		}
		

		public static string DecryptAes(byte[] encryptedData, byte[] key, byte[] iv)
		{
			using (Aes aes = Aes.Create())
			{
				aes.Key = key;
				aes.IV = iv;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;

				using (ICryptoTransform decryptor = aes.CreateDecryptor())
				using (MemoryStream ms = new MemoryStream(encryptedData))
				using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
				using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))
				{
					return sr.ReadToEnd(); // Возвращает расшифрованную строку
				}
			}
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

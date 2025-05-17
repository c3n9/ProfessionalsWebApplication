using System.Net;
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
            var form = _context.Forms
                .Where(x => x.DateStart.Date <= DateTime.Now.Date && x.DateEnd.Date >= DateTime.Now.Date)
                .Include(f => f.Questions).ToList().FirstOrDefault(x => x.Hash == hash);
            if (form == null || form.Questions.Count == 0)
            {
                Response.StatusCode = 404;
                return View("NotFoundView");
            }

            return View("FormView", form);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitFormDesctop([FromBody] EncryptedSubmissionDto encryptSubmission)
        {
            var decryptedKeyData = CryptoService.DecryptRSA(encryptSubmission.Key);
            var keyInfo = JsonSerializer.Deserialize<AesKeyInfo>(decryptedKeyData);
            var newUser = new User()
            {
                FormId = encryptSubmission.FormId,
                AnswersJson = encryptSubmission.Data,
                Timestamp = DateTime.Now,
                Key = keyInfo.Key,  
                Iv = keyInfo.Iv,
            };
            var answers = newUser.Answers;
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
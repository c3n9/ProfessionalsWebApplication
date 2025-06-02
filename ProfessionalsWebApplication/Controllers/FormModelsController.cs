using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ProfessionalsWebApplication.Models.DTO;

namespace ProfessionalsWebApplication.Controllers
{
    [Route("api/FormModels")]
    [ApiController]
    public class FormModelsController : Controller
    {
        private readonly ProfessionalsDbContext _context;

        public FormModelsController(ProfessionalsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetForms()
        {
            var forms = await _context.Forms.Include(f => f.Questions).ToListAsync();
            
            foreach (var form in forms)
            {
                if(form.IsVisible && (form.DateStart.Date > DateTime.Now.Date || form.DateEnd.Date < DateTime.Now.Date))
                    form.IsVisible = false;
            }
            
            await _context.SaveChangesAsync();

            return Ok(forms.Select(x => new
            {
                x.Id,
                x.Name,
                x.DateStart,
                x.DateEnd,
                x.IsVisible,
                FormUrl = $"{Request.Scheme}://{Request.Host}" + $"/forms/{x.Hash}",
                x.Questions
            }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetForm(int id)
        {
            var form = await _context.Forms.FindAsync(id);
            if (form == null) return NotFound("Такая форма не найдена.");
           
            if(form.IsVisible && (form.DateStart.Date > DateTime.Now.Date || form.DateEnd.Date < DateTime.Now.Date))
                form.IsVisible = false;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                form.Id,
                form.Name,
                form.DateStart,
                form.DateEnd,
                form.IsVisible,
                FormUrl = $"{Request.Scheme}://{Request.Host}" + $"/forms/{form.Hash}",
                form.Questions,
            });
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateForm([FromForm] FormModelDto formModelDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var formModel = new FormModel
            {
                Name = formModelDto.Name,
                Hash = string.Empty,
                IsVisible = true,
                DateStart = formModelDto.DateStart,
                DateEnd = formModelDto.DateEnd
            };
            
            
            if(formModel.DateStart.Date > DateTime.Now.Date || formModel.DateEnd.Date < DateTime.Now.Date)
                formModel.IsVisible = false;

            _context.Forms.Add(formModel);
            await _context.SaveChangesAsync();

            formModel.Hash = HashGenerator.GenerateHash(formModel.Id);
            _context.Forms.Update(formModel);
            await _context.SaveChangesAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var formUrl = $"{baseUrl}/forms/{formModel.Hash}";

            return Created(formUrl, new
            {
                formModel.Id,
                formModel.Name,
                formModel.DateStart,
                formModel.DateEnd,
                formModel.IsVisible,
                FormUrl = $"{Request.Scheme}://{Request.Host}" + $"/forms/{formModel.Hash}",
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateForm(int id, [FromForm] FormModelDto formModelDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingForm = await _context.Forms.FindAsync(id);
            if (existingForm == null)
                return NotFound("Форма не найдена.");

            existingForm.Name = formModelDto.Name;
            existingForm.DateStart = formModelDto.DateStart;
            existingForm.DateEnd = formModelDto.DateEnd;
            
            if(existingForm.IsVisible && (existingForm.DateStart.Date > DateTime.Now.Date || existingForm.DateEnd.Date < DateTime.Now.Date))
                existingForm.IsVisible = false;

            try
            {
                await _context.SaveChangesAsync();

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var formUrl = $"{baseUrl}/forms/{existingForm.Hash}";

                return Ok(new
                {
                    existingForm.Id,
                    existingForm.Name,
                    existingForm.DateStart,
                    existingForm.DateEnd,
                    existingForm.IsVisible,
                    FormUrl = $"{baseUrl}/forms/{existingForm.Hash}",
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                var entry = _context.Entry(existingForm);
                await entry.ReloadAsync();

                if (entry.State == EntityState.Detached)
                    return NotFound("Форма была удалена.");
                else
                    return Conflict("Конфликт версий. Данные были изменены другим пользователем.");
            }
        }
        
        [Authorize]
        [HttpPut("updateactive/{id}")]
        public async Task<IActionResult> UpdateFormActiveForm(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingForm = await _context.Forms.FindAsync(id);
            if (existingForm == null)
                return NotFound("Форма не найдена.");

            try
            {
                existingForm.IsVisible = !existingForm.IsVisible;
                await _context.SaveChangesAsync();

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var formUrl = $"{baseUrl}/forms/{existingForm.Hash}";

                return Ok(new
                {
                    existingForm.Id,
                    existingForm.Name,
                    existingForm.DateStart,
                    existingForm.IsVisible,
                    existingForm.DateEnd,
                    FormUrl = $"{baseUrl}/forms/{existingForm.Hash}",
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                var entry = _context.Entry(existingForm);
                await entry.ReloadAsync();

                if (entry.State == EntityState.Detached)
                    return NotFound("Форма была удалена.");
                else
                    return Conflict("Конфликт версий. Данные были изменены другим пользователем.");
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteForm(int id)
        {
            var formModel = await _context.Forms.FindAsync(id);
            if (formModel == null)
            {
                return NotFound();
            }

            _context.Forms.Remove(formModel);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
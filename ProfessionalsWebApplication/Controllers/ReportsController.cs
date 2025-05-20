using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using ProfessionalsWebApplication.Enums;
using ProfessionalsWebApplication.Models;
using System.IO.Compression;
using Microsoft.AspNetCore.Authorization;

namespace ProfessionalsWebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : Controller
    {
        private readonly ProfessionalsDbContext _context;
        public ReportsController(ProfessionalsDbContext context)
        {
            _context = context;
        }

        [HttpGet("{formId}")]
        [Authorize]
        public async Task<IActionResult> GetReportByForm(int formId)
        {
            var form = _context.Forms
                .Include(f => f.Questions)
                .Include(f => f.Users)
                .FirstOrDefault(x => x.Id == formId);

            if (form == null)
                return NotFound("Форма не найдена");

            if (form.Users == null || !form.Users.Any())
                return NotFound("Результатов по этой форме нет");

            bool hasFileAnswers = form.Users
                .SelectMany(u => u.Answers)
                .Any(a => a.Type == AnswerType.File && a.File != null);

            // Создаем временную структуру папок
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            try
            {
                var excelFile = Path.Combine(tempFolder, $"report_{form.Name}.xlsx");
                IWorkbook workbook = new XSSFWorkbook();
                ISheet worksheet = workbook.CreateSheet("Responses");

                // Стили для оформления
                var headerStyle = workbook.CreateCellStyle();
                var headerFont = workbook.CreateFont();
                headerFont.IsBold = true;
                headerFont.FontHeightInPoints = 12;
                headerStyle.SetFont(headerFont);
                headerStyle.FillForegroundColor = IndexedColors.Grey25Percent.Index;
                headerStyle.FillPattern = FillPattern.SolidForeground;

                var linkStyle = workbook.CreateCellStyle();
                var linkFont = workbook.CreateFont();
                linkFont.Underline = FontUnderlineType.Single;
                linkFont.Color = IndexedColors.Blue.Index;
                linkStyle.SetFont(linkFont);

                // Заголовки
                var headerRow = worksheet.CreateRow(0);
                headerRow.CreateCell(0).SetCellValue("ID");
                headerRow.CreateCell(1).SetCellValue("Дата и время");

                // Получаем все уникальные вопросы
                var allQuestions = form.Users
                    .SelectMany(u => u.Answers.Select(a => a.Question))
                    .Distinct()
                    .ToList();

                // Добавляем вопросы как заголовки
                for (int i = 0; i < allQuestions.Count; i++)
                {
                    var cell = headerRow.CreateCell(i + 2);
                    cell.SetCellValue(allQuestions[i]);
                    cell.CellStyle = headerStyle;
                }

                string filesRootFolder = null;
                if (hasFileAnswers)
                {
                    filesRootFolder = Path.Combine(tempFolder, "Files");
                    Directory.CreateDirectory(filesRootFolder);
                }

                // Заполняем данные
                for (int rowIdx = 0; rowIdx < form.Users.Count; rowIdx++)
                {
                    var user = form.Users[rowIdx];
                    var row = worksheet.CreateRow(rowIdx + 1);

                    row.CreateCell(0).SetCellValue(user.Id);
                    row.CreateCell(1).SetCellValue(user.Timestamp.ToString("g"));

                    foreach (var answer in user.Answers)
                    {
                        var col = allQuestions.IndexOf(answer.Question) + 2;

                        if (answer.Type == AnswerType.Text)
                        {
                            row.CreateCell(col).SetCellValue(answer.Value);
                        }
                        else if (answer.Type == AnswerType.File && answer.File != null && hasFileAnswers)
                        {
                            var userFilesFolder = Path.Combine(filesRootFolder, $"Files_{user.Id}");
                            Directory.CreateDirectory(userFilesFolder);

                            var fileAnswer = answer.File;
                            var safeFileName = Path.GetFileName(fileAnswer.FileName);
                            var filePath = Path.Combine(userFilesFolder, safeFileName);
                            await System.IO.File.WriteAllBytesAsync(filePath, Convert.FromBase64String(fileAnswer.FileContent));

                            var relativePath = $"Files/Files_{user.Id}/{safeFileName}";

                            var linkCell = row.CreateCell(col);
                            linkCell.SetCellValue($"📎 {safeFileName}");

                            var link = workbook.GetCreationHelper().CreateHyperlink(HyperlinkType.File);
                            link.Address = relativePath;
                            linkCell.Hyperlink = link;
                            linkCell.CellStyle = linkStyle;
                        }
                        else if (answer.Type == AnswerType.File)
                        {
                            row.CreateCell(col).SetCellValue("📎 (файл)");
                        }
                    }
                }

                // Автонастройка ширины столбцов
                for (int i = 0; i < headerRow.LastCellNum; i++)
                {
                    worksheet.AutoSizeColumn(i);
                }

                // Сохраняем Excel файл
                using (var fileStream = new FileStream(excelFile, FileMode.Create))
                {
                    workbook.Write(fileStream);
                }

                // Если есть файловые ответы - возвращаем ZIP, иначе - Excel
                if (hasFileAnswers)
                {
                    var zipFile = Path.Combine(Path.GetTempPath(), $"report_{form.Name}_{Guid.NewGuid()}.zip");
                    ZipFile.CreateFromDirectory(tempFolder, zipFile, CompressionLevel.Optimal, false);

                    var fileStream = new FileStream(zipFile, FileMode.Open, FileAccess.Read, FileShare.Delete);
                    var fileStreamResult = new FileStreamResult(fileStream, "application/zip")
                    {
                        FileDownloadName = $"report_{form.Name}.zip"
                    };

                    Response.OnCompleted(async () =>
                    {
                        try
                        {
                            fileStream.Dispose();
                            Directory.Delete(tempFolder, true);
                            System.IO.File.Delete(zipFile);
                        }
                        catch { }
                    });

                    return fileStreamResult;
                }
                else
                {
                    var fileStream = new FileStream(excelFile, FileMode.Open, FileAccess.Read, FileShare.Delete);
                    var fileStreamResult = new FileStreamResult(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"report_{form.Name}.xlsx"
                    };

                    Response.OnCompleted(async () =>
                    {
                        try
                        {
                            fileStream.Dispose();
                            Directory.Delete(tempFolder, true);
                        }
                        catch { }
                    });

                    return fileStreamResult;
                }
            }
            catch
            {
                try
                {
                    Directory.Delete(tempFolder, true);
                }
                catch { }
                throw;
            }
        }
    }
}

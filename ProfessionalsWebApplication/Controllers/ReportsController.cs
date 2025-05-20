using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using ProfessionalsWebApplication.Enums;
using ProfessionalsWebApplication.Models;
using System.IO.Compression;

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

            // Создаем временную структуру папок
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var filesRootFolder = Path.Combine(tempFolder, "Files");
            Directory.CreateDirectory(tempFolder);
            Directory.CreateDirectory(filesRootFolder);

            try
            {
                // Создаем Excel файл с помощью NPOI
                var excelFile = Path.Combine(tempFolder, $"report_{formId}.xlsx");
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

                // Заполняем данные
                for (int rowIdx = 0; rowIdx < form.Users.Count; rowIdx++)
                {
                    var user = form.Users[rowIdx];
                    var row = worksheet.CreateRow(rowIdx + 1);

                    row.CreateCell(0).SetCellValue(user.Id);
                    row.CreateCell(1).SetCellValue(user.Timestamp.ToString("g"));

                    // Папка для файлов этого пользователя
                    var userFilesFolder = Path.Combine(filesRootFolder, $"Files_{user.Id}");
                    Directory.CreateDirectory(userFilesFolder);

                    foreach (var answer in user.Answers)
                    {
                        var col = allQuestions.IndexOf(answer.Question) + 2;

                        if (answer.Type == AnswerType.Text)
                        {
                            row.CreateCell(col).SetCellValue(answer.Value);
                        }
                        else if (answer.Type == AnswerType.File && answer.File != null)
                        {
                            var fileAnswer = answer.File;
                            var safeFileName = Path.GetFileName(fileAnswer.FileName);
                            var filePath = Path.Combine(userFilesFolder, safeFileName);
                            await System.IO.File.WriteAllBytesAsync(filePath, Convert.FromBase64String(fileAnswer.FileContent));

                            // ОТНОСИТЕЛЬНЫЙ ПУТЬ ВНУТРИ АРХИВА
                            var relativePath = $"Files/Files_{user.Id}/{safeFileName}";

                            var linkCell = row.CreateCell(col);
                            linkCell.SetCellValue($"📎 {safeFileName}");

                            var link = workbook.GetCreationHelper().CreateHyperlink(HyperlinkType.File);
                            link.Address = relativePath; // Используем относительный путь
                            linkCell.Hyperlink = link;
                            linkCell.CellStyle = linkStyle;
                        }
                    }
                }

                // Автонастройка ширины столбцов
                for (int i = 0; i < headerRow.LastCellNum; i++)
                {
                    worksheet.AutoSizeColumn(i);
                }

                // Сохраняем Excel
                using (var fileStream = new FileStream(excelFile, FileMode.Create))
                {
                    workbook.Write(fileStream);
                }

                // Создаем ZIP архив
                var zipFile = Path.Combine(Path.GetTempPath(), $"report_{formId}.zip");
                ZipFile.CreateFromDirectory(tempFolder, zipFile, CompressionLevel.Optimal, false);

                // Возвращаем файл
                var fileStreamResult = new FileStream(zipFile, FileMode.Open, FileAccess.Read);
                return File(fileStreamResult, "application/zip", $"report_{formId}.zip");
            }
            finally
            {
                // Удаляем временные файлы после отправки
                try { Directory.Delete(tempFolder, true); }
                catch { /* Игнорируем ошибки удаления */ }
            }
        }
    }
}

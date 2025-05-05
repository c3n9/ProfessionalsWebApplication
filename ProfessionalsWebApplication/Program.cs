using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ProfessionalsWebApplication.Controllers;
using ProfessionalsWebApplication.Models;

var builder = WebApplication.CreateBuilder(args);

// Регистрация конфигурации FileStorage
builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection("FileStorage"));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ProfessionalsDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSession();

var app = builder.Build();

// Настройка статических файлов
app.UseStaticFiles(); // Для wwwroot

// Настройка дополнительных путей для статических файлов
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Styles")),
    RequestPath = "/styles"
});

// Для загрузки баннеров
var fileStorageSettings = app.Configuration.GetSection("FileStorage").Get<FileStorageSettings>();
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), fileStorageSettings?.BannerImagesPath ?? "uploads/banners");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads/banners"
});

app.UseSession();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();


// ��������� ��� ����
//dotnet ef migrations add <��������>
//dotnet ef database update


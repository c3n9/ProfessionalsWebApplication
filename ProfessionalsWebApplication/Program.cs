using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ProfessionalsWebApplication.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ProfessionalsDbContext>(options =>
	options.UseSqlite(connectionString));

// Добавляем поддержку MVC с представлениями
builder.Services.AddControllersWithViews();

// Добавляем поддержку API Explorer (Swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSession(); 

var app = builder.Build();

app.UseStaticFiles(); // Для обслуживания файлов из wwwroot по умолчанию

// Для обслуживания файлов из другой папки (например, Styles)
app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new PhysicalFileProvider(
		Path.Combine(Directory.GetCurrentDirectory(), "Styles")),
	RequestPath = "/styles" // Этот путь будет использоваться для доступа к файлам
});


app.UseSession();
// Включаем Swagger только в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Подключаем статические файлы (CSS, JS)
app.UseStaticFiles();

// Используем маршрутизацию и авторизацию
app.UseRouting();
app.UseAuthorization();

//Добавляем маршруты для представлений и API
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();



// Подсказка для себя
//dotnet ef migrations add <название>
//dotnet ef database update


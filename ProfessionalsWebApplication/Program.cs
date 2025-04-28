using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ProfessionalsWebApplication.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ProfessionalsDbContext>(options =>
	options.UseSqlite(connectionString));

// ��������� ��������� MVC � ���������������
builder.Services.AddControllersWithViews();

// ��������� ��������� API Explorer (Swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSession(); 

var app = builder.Build();

app.UseStaticFiles(); // ��� ������������ ������ �� wwwroot �� ���������

// ��� ������������ ������ �� ������ ����� (��������, Styles)
app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new PhysicalFileProvider(
		Path.Combine(Directory.GetCurrentDirectory(), "Styles")),
	RequestPath = "/styles" // ���� ���� ����� �������������� ��� ������� � ������
});


app.UseSession();
// �������� Swagger ������ � ������ ����������
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ���������� ����������� ����� (CSS, JS)
app.UseStaticFiles();

// ���������� ������������� � �����������
app.UseRouting();
app.UseAuthorization();

//��������� �������� ��� ������������� � API
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();



// ��������� ��� ����
//dotnet ef migrations add <��������>
//dotnet ef database update


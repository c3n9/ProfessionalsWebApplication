using Microsoft.EntityFrameworkCore;
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

var app = builder.Build();

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


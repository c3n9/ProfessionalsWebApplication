using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProfessionalsWebApplication.Models;
using ProfessionalsWebApplication.Models.Settings;
using ProfessionalsWebApplication.Services;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, 
            ValidateAudience = true, 
            ValidateLifetime = true, 
            ValidateIssuerSigningKey = true, 
            ValidIssuer = jwtSettings["Issuer"], 
            ValidAudience = jwtSettings["Audience"], 
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization(); 

builder.Services.AddSingleton<JwtService>();


// Регистрация конфигурации FileStorage
builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection("FileStorage"));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ProfessionalsDbContext>(options =>
    options.UseSqlite(connectionString));

//builder.Services.AddCors(options =>{
//    options.AddPolicy("AllowFrontend", builder =>    {
//        builder.WithOrigins("http://localhost:5173")               
//            .AllowAnyHeader()
//            .AllowAnyMethod()   
//			.AllowCredentials();
//    });
//});

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", builder =>
	{
		builder.AllowAnyOrigin()    // Разрешает запросы с ЛЮБОГО домена
			   .AllowAnyMethod()    // Разрешает все HTTP-методы (GET, POST, PUT и т.д.)
			   .AllowAnyHeader();   // Разрешает все HTTP-заголовки
	});
});




builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSession();
builder.Services.AddHttpClient();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {your_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>() 
        }
    });
});

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

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Fonts")),
    RequestPath = "/fonts"
});

// Для загрузки баннеров
// В Program.cs

// 1. Создаем scope для доступа к сервисам
using (var scope = app.Services.CreateScope())
{
    // 2. Получаем IOptions<FileStorageSettings> из DI-контейнера
    var fileStorageSettings = scope.ServiceProvider
        .GetRequiredService<IOptions<FileStorageSettings>>()
        .Value;
    // 3. Формируем путь с учетом возможного null
    var uploadsPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        fileStorageSettings?.BannerImagesPath ?? "uploads/banners"
    );
    // 4. Создаем директорию, если не существует
    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
    }
    // 5. Настраиваем доступ к статическим файлам
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads/banners"
    });
}

// 1. Создаем scope для доступа к сервисам
using (var scope = app.Services.CreateScope())
{
	// 2. Получаем IOptions<FileStorageSettings> из DI-контейнера
	var fileStorageSettings = scope.ServiceProvider
		.GetRequiredService<IOptions<FileStorageSettings>>()
		.Value;
	// 3. Формируем путь с учетом возможного null
	var uploadsPath = Path.Combine(
		Directory.GetCurrentDirectory(),
		fileStorageSettings?.ExpertImagesPath ?? "uploads/experts"
	);
	// 4. Создаем директорию, если не существует
	if (!Directory.Exists(uploadsPath))
	{
		Directory.CreateDirectory(uploadsPath);
	}
	// 5. Настраиваем доступ к статическим файлам
	app.UseStaticFiles(new StaticFileOptions
	{
		FileProvider = new PhysicalFileProvider(uploadsPath),
		RequestPath = "/uploads/experts"
	});
}

// Для загрузки участников
// В Program.cs

// 1. Создаем scope для доступа к сервисам
using (var scope = app.Services.CreateScope())
{
    // 2. Получаем IOptions<FileStorageSettings> из DI-контейнера
    var fileStorageSettings = scope.ServiceProvider
        .GetRequiredService<IOptions<FileStorageSettings>>()
        .Value;
    // 3. Формируем путь с учетом возможного null
    var uploadsPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        fileStorageSettings?.CompetitorImagesPath ?? "uploads/competitors"
    );
    // 4. Создаем директорию, если не существует
    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
    }
    // 5. Настраиваем доступ к статическим файлам
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads/competitors"
    });
}

// 1. Создаем scope для доступа к сервисам
using (var scope = app.Services.CreateScope())
{
    // 2. Получаем IOptions<FileStorageSettings> из DI-контейнера
    var fileStorageSettings = scope.ServiceProvider
        .GetRequiredService<IOptions<FileStorageSettings>>()
        .Value;
    // 3. Формируем путь с учетом возможного null
    var uploadsPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        fileStorageSettings?.CompetenceImagesPath ?? "uploads/competences"
    );
    // 4. Создаем директорию, если не существует
    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
    }
    // 5. Настраиваем доступ к статическим файлам
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads/competences"
    });
}

app.UseSession();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.UseCors("AllowAll");


//app.UseCors("AllowFrontend");


app.Run();


// ��������� ��� ����
//dotnet ef migrations add <��������>
//dotnet ef database update


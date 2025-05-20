using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ProfessionalsWebApplication.Services;

/// <summary>
	/// Сервис для генерации JWT-токенов.
	/// </summary>
	public class JwtService
	{
		private readonly string _secret;   // Секретный ключ для подписи токена
		private readonly string _issuer;   // Издатель (Issuer) токена
		private readonly string _audience; // Аудитория (Audience) токена

		/// <summary>
		/// Конструктор JwtService. Загружает настройки JWT из конфигурации.
		/// </summary>
		/// <param name="config">Объект IConfiguration для получения значений из appsettings.json.</param>
		public JwtService(IConfiguration config)
		{
			_secret = config["Jwt:Key"];       // Считываем секретный ключ
			_issuer = config["Jwt:Issuer"];    // Считываем издателя токена
			_audience = config["Jwt:Audience"];// Считываем аудиторию токена

			// Проверяем, чтобы секретный ключ был не менее 32 символов (256 бит)
			if (string.IsNullOrEmpty(_secret) || Encoding.UTF8.GetBytes(_secret).Length < 32)
			{
				throw new ArgumentException("JWT Secret Key must be at least 32 characters long!");
			}
		}

		/// <summary>
		/// Генерирует JWT-токен на основе имени пользователя.
		/// </summary>
		/// <param name="username">Имя пользователя, для которого создается токен.</param>
		/// <returns>Строка с JWT-токеном.</returns>
		public string GenerateToken(string username)
		{
			// Создаем список claims (утверждений), которые будут включены в токен.
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, username), // Основной claim - имя пользователя
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Уникальный идентификатор токена (JTI)
			};

			// Преобразуем секретный ключ в массив байтов и создаем объект SymmetricSecurityKey
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));

			// Создаем учетные данные для подписи токена (алгоритм HMAC SHA256)
			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			// Создаем объект JWT-токена
			var token = new JwtSecurityToken(
				_issuer,    // Издатель токена
				_audience,  // Аудитория токена
				claims,     // Утверждения (claims)
				expires: DateTime.UtcNow.AddHours(1), // Время жизни токена (1 час)
				signingCredentials: credentials       // Учетные данные для подписи
			);

			// Генерируем строку токена и возвращаем её
			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
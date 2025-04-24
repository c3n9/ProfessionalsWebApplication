using Microsoft.EntityFrameworkCore;

namespace ProfessionalsWebApplication.Models
{
	public class ProfessionalsDbContext : DbContext
	{
		public ProfessionalsDbContext(DbContextOptions<ProfessionalsDbContext> options) : base(options) 
		{

		}
		public DbSet<FormModel> Forms { get; set; }

		public DbSet<QuestionModel> Questions { get; set; }

		public DbSet<User> Users {  get; set; }
	}
}

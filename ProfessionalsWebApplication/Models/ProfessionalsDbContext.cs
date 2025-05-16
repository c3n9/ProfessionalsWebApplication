using Microsoft.EntityFrameworkCore;

namespace ProfessionalsWebApplication.Models
{
	public class ProfessionalsDbContext : DbContext
	{
		public ProfessionalsDbContext(DbContextOptions<ProfessionalsDbContext> options) : base(options) 
		{

		}
		public DbSet<FormModel> Forms { get; set; }
		
		public DbSet<Championship> Championships { get; set; }
		public DbSet<Competence> Competences { get; set; }
		public DbSet<QuestionModel> Questions { get; set; }

		public DbSet<User> Users {  get; set; }
		public DbSet<Banner> Banners {  get; set; }
		public DbSet<Competitor> Competitors {  get; set; }
		public DbSet<Expert> Experts {  get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<QuestionModel>()
				.HasOne(q => q.FormModel)
				.WithMany(f => f.Questions)
				.HasForeignKey(q => q.ThemeId)
				.OnDelete(DeleteBehavior.Cascade);

			base.OnModelCreating(modelBuilder);
		}
	}
}

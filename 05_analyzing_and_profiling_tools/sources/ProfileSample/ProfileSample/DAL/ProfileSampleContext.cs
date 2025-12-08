using Microsoft.EntityFrameworkCore;

namespace ProfileSample.DAL;

public class ProfileSampleContext : DbContext
{
    public ProfileSampleContext(DbContextOptions<ProfileSampleContext> options) : base(options) { }

    public DbSet<ImgSource> ImgSources { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImgSource>(entity =>
        {
            entity.ToTable("ImgSources");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Data).IsRequired();
        });
    }
}
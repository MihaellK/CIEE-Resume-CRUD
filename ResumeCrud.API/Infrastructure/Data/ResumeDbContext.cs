using Microsoft.EntityFrameworkCore;
using ResumeCrud.API.Domain.Entities;

namespace ResumeCrud.API.Infrastructure.Data;

public class ResumeDbContext : DbContext
{
    public ResumeDbContext(DbContextOptions<ResumeDbContext> options) : base(options) { }

    public DbSet<Resume> Resumes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resume>(entity =>
        {
            entity.ToTable("Resumes");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);
                  
            entity.Property(e => e.Email)
                  .HasMaxLength(150);
                  
            entity.Property(e => e.Phone)
                  .HasMaxLength(20);
                  
            // byte[] mapeia automaticamente para varbinary(max) no SQL Server
            entity.Property(e => e.PdfContent)
                  .IsRequired(); 
        });
    }
}
using Microsoft.EntityFrameworkCore;
using AspireApp.BedRock.SonetOps.MCP.Models;

namespace AspireApp.BedRock.SonetOps.MCP.Data;

public class MCPContext : DbContext
{
    public MCPContext(DbContextOptions<MCPContext> options) : base(options)
    {
    }

    public DbSet<Instruction> Instructions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Instruction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
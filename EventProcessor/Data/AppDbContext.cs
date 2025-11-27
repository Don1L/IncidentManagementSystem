using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace EventProcessor.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Incident> Incidents { get; set; } = null!;
    public DbSet<IncidentEvent> IncidentEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<IncidentEvent>()
            .HasKey(ie => new { ie.EventId, ie.IncidentId });
        
        //Incident -> IncidentEvent
        modelBuilder.Entity<IncidentEvent>()
            .HasOne(ie => ie.Incident)
            .WithMany(i => i.IncidentEvents)
            .HasForeignKey(ie => ie.IncidentId);
        
        //Event -> IncidentEvent
        modelBuilder.Entity<IncidentEvent>()
            .HasOne(ie => ie.Event)
            .WithMany()
            .HasForeignKey(ie => ie.EventId);
    }
}
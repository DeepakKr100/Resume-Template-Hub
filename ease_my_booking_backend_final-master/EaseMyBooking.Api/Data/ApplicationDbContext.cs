using EaseMyBooking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EaseMyBooking.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Place> Places => Set<Place>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<PlaceImage> PlaceImages => Set<PlaceImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Place>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        // NEW: cap URL length
        modelBuilder.Entity<Place>()
            .Property(p => p.GoogleMapsUrl)
            .HasMaxLength(2048);

        modelBuilder.Entity<Place>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.PlacesOwned)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Place)
            .WithMany(p => p.Bookings)
            .HasForeignKey(b => b.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Place)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews!)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<PlaceImage>()
            .HasOne(pi => pi.Place)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlaceImage>()
            .HasIndex(pi => new { pi.PlaceId, pi.SortOrder });
    }
}

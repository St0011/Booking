using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Booking.Models;
using Microsoft.AspNetCore.Identity;

namespace Booking.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Rooms> Rooms { get; set; }
        public DbSet<BookRooms> BookRooms { get; set; }
    }
}
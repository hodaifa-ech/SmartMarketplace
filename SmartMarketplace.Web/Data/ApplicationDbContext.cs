using Microsoft.EntityFrameworkCore;
using SmartMarketplace.Web.Models;

namespace SmartMarketplace.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Mission> Missions { get; set; }
}
using HolidayTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HolidayTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
}
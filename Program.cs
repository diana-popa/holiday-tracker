using HolidayTracker.Data;
using HolidayTracker.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlite("Data Source=holidays.db"));
builder.Services.AddScoped<HolidayStore>();
builder.Services.AddHttpClient<SlackService>();

var app = builder.Build();

// Auto-create the database and tables on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Seed default team if empty
    var store = scope.ServiceProvider.GetRequiredService<HolidayStore>();

    if (!store.GetTeam().Any())
    {
        foreach (var name in new[] { "Alice", "Bob", "Carlos", "Dana", "Eli", "Fiona" })
            store.AddTeamMember(name);
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

// Weekly Slack summary every Monday at 9am
var timer = new System.Timers.Timer(60_000);
timer.Elapsed += async (_, _) =>
{
    var now = DateTime.Now;
    if (now.DayOfWeek == DayOfWeek.Monday && now.Hour == 9 && now.Minute == 0)
    {
        using var scope = app.Services.CreateScope();
        var slack = scope.ServiceProvider.GetRequiredService<SlackService>();
        var store = scope.ServiceProvider.GetRequiredService<HolidayStore>();
        await slack.SendWeeklySummary(store.GetTeam(), store.GetAll().Cast<dynamic>().ToList());
    }
};
timer.Start();

app.Run($"http://0.0.0.0:{Environment.GetEnvironmentVariable("PORT") ?? "8080"}");
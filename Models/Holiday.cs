namespace HolidayTracker.Models;

public class Holiday
{
    public int Id { get; set; }
    public string Person { get; set; } = "";
    public string Date { get; set; } = "";
}

public class TeamMember
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

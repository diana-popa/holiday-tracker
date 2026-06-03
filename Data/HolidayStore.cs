using HolidayTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HolidayTracker.Data;

public class HolidayStore
{
    private readonly AppDbContext _db;

    public HolidayStore(AppDbContext db)
    {
        _db = db;
    }

    public List<Holiday> GetAll() => _db.Holidays.ToList();

    public List<string> GetTeam() => _db.TeamMembers.OrderBy(t => t.Id).Select(t => t.Name).ToList();

    public void Add(Holiday holiday)
    {
        if (!_db.Holidays.Any(h => h.Person == holiday.Person && h.Date == holiday.Date))
        {
            _db.Holidays.Add(holiday);
            _db.SaveChanges();
        }
    }

    public void Remove(string person, string date)
    {
        var item = _db.Holidays.FirstOrDefault(h => h.Person == person && h.Date == date);
        if (item != null)
        {
            _db.Holidays.Remove(item);
            _db.SaveChanges();
        }
    }

    public void AddTeamMember(string name)
    {
        if (!_db.TeamMembers.Any(t => t.Name == name))
        {
            _db.TeamMembers.Add(new TeamMember { Name = name });
            _db.SaveChanges();
        }
    }

    public void RemoveTeamMember(string name)
    {
        var member = _db.TeamMembers.FirstOrDefault(t => t.Name == name);
        if (member != null)
        {
            _db.TeamMembers.Remove(member);
            _db.Holidays.RemoveRange(_db.Holidays.Where(h => h.Person == name));
            _db.SaveChanges();
        }
    }
}

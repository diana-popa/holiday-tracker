using HolidayTracker.Data;
using HolidayTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HolidayTracker.Tests;

[TestClass]
public class HolidayStoreTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [TestMethod]
    public void Add_Holiday_ShouldPersistToDatabase()
    {
        var ctx = CreateInMemoryContext();
        var store = new HolidayStore(ctx);
        var holiday = new Holiday { Person = "Alice", Date = "2024-12-25" };

        store.Add(holiday);

        Assert.AreEqual(1, ctx.Holidays.Count());
        Assert.AreEqual("Alice", ctx.Holidays.First().Person);
    }

    [TestMethod]
    public void Add_DuplicateHoliday_ShouldNotAddTwice()
    {
        var ctx = CreateInMemoryContext();
        var store = new HolidayStore(ctx);
        var holiday = new Holiday { Person = "Alice", Date = "2024-12-25" };

        store.Add(holiday);
        store.Add(holiday);

        Assert.AreEqual(1, ctx.Holidays.Count());
    }

    [TestMethod]
    public void Remove_Holiday_ShouldDeleteFromDatabase()
    {
        var ctx = CreateInMemoryContext();
        var store = new HolidayStore(ctx);
        store.Add(new Holiday { Person = "Alice", Date = "2024-12-25" });

        store.Remove("Alice", "2024-12-25");

        Assert.AreEqual(0, ctx.Holidays.Count());
    }

    [TestMethod]
    public void Remove_NonExistentHoliday_ShouldNotThrow()
    {
        var ctx = CreateInMemoryContext();
        var store = new HolidayStore(ctx);

        try
        {
            store.Remove("Alice", "2024-12-25");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Expected no exception but got: {ex.Message}");
        }
    }

    [TestMethod]
    public void AddTeamMember_ShouldPersistToDatabase()
    {
        var ctx = CreateInMemoryContext();
        var store = new HolidayStore(ctx);

        store.AddTeamMember("Alice");

        Assert.AreEqual(1, ctx.TeamMembers.Count());
        Assert.AreEqual("Alice", ctx.TeamMembers.First().Name);
    }

    [TestMethod]
    public void AddTeamMember_Duplicate_ShouldNotAddTwice()
    {
        var ctx = CreateInMemoryContext();
        var store = new HolidayStore(ctx);

        store.AddTeamMember("Alice");
        store.AddTeamMember("Alice");

        Assert.AreEqual(1, ctx.TeamMembers.Count());
    }

    [TestMethod]
    public void RemoveTeamMember_ShouldDeleteMemberAndTheirHolidays()
    {
        var ctx = CreateInMemoryContext();
        var store = new HolidayStore(ctx);
        store.AddTeamMember("Alice");
        store.Add(new Holiday { Person = "Alice", Date = "2024-12-25" });

        store.RemoveTeamMember("Alice");

        Assert.AreEqual(0, ctx.TeamMembers.Count());
        Assert.AreEqual(0, ctx.Holidays.Count());
    }

    [TestMethod]
    public void GetAll_ShouldReturnAllHolidays()
    {
        var ctx = CreateInMemoryContext();
        var store = new HolidayStore(ctx);
        store.Add(new Holiday { Person = "Alice", Date = "2024-12-25" });
        store.Add(new Holiday { Person = "Bob", Date = "2024-12-26" });

        var result = store.GetAll();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void GetTeam_ShouldReturnMemberNamesInOrder()
    {
        var ctx = CreateInMemoryContext();
        var store = new HolidayStore(ctx);
        store.AddTeamMember("Alice");
        store.AddTeamMember("Bob");

        var result = store.GetTeam();

        CollectionAssert.AreEqual(new[] { "Alice", "Bob" }, result);
    }
}

using HolidayTracker.Controllers;
using HolidayTracker.Data;
using HolidayTracker.Models;
using HolidayTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace HolidayTracker.Tests;

[TestClass]
public class HolidayControllerTests
{
    private (HolidayController controller, AppDbContext ctx) CreateController()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var ctx = new AppDbContext(options);
        var store = new HolidayStore(ctx);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Slack:WebhookUrl", "" }
            })
            .Build();
        var http = new HttpClient(new MockHttpMessageHandler(HttpStatusCode.OK));
        var slack = new SlackService(config, http);

        var controller = new HolidayController(store, slack);
        return (controller, ctx);
    }

    [TestMethod]
    public async Task Add_ValidHoliday_ShouldReturnOk()
    {
        var (controller, _) = CreateController();
        var holiday = new Holiday { Person = "Alice", Date = "2024-12-25" };

        var result = await controller.Add(holiday);

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public async Task Add_ValidHoliday_ShouldPersistToDatabase()
    {
        var (controller, ctx) = CreateController();
        var holiday = new Holiday { Person = "Alice", Date = "2024-12-25" };

        await controller.Add(holiday);

        Assert.AreEqual(1, ctx.Holidays.Count());
    }

    [TestMethod]
    public void GetAll_ShouldReturnHolidaysAndTeam()
    {
        var (controller, _) = CreateController();

        var result = controller.GetAll() as OkObjectResult;

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Value);
    }

    [TestMethod]
    public void Remove_ExistingHoliday_ShouldReturnOk()
    {
        var (controller, ctx) = CreateController();
        ctx.Holidays.Add(new Holiday { Person = "Alice", Date = "2024-12-25" });
        ctx.SaveChanges();

        var result = controller.Remove("Alice", "2024-12-25");

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public void AddMember_NewMember_ShouldReturnOk()
    {
        var (controller, _) = CreateController();

        var result = controller.AddMember("Alice");

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public void RemoveMember_ExistingMember_ShouldReturnOk()
    {
        var (controller, ctx) = CreateController();
        ctx.TeamMembers.Add(new TeamMember { Name = "Alice" });
        ctx.SaveChanges();

        var result = controller.RemoveMember("Alice");

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }
    [TestMethod]
    public async Task Add_DuplicateHoliday_ShouldStillReturnOk()
    {
        var (controller, _) = CreateController();
        var holiday = new Holiday { Person = "Alice", Date = "2024-12-25" };

        await controller.Add(holiday);
        var result = await controller.Add(holiday);

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public void Remove_NonExistentHoliday_ShouldReturnOk()
    {
        var (controller, _) = CreateController();

        var result = controller.Remove("Alice", "2024-12-25");

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public void RemoveMember_NonExistentMember_ShouldReturnOk()
    {
        var (controller, _) = CreateController();

        var result = controller.RemoveMember("Nobody");

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }
}
using HolidayTracker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace HolidayTracker.Tests;

[TestClass]
public class SlackServiceTests
{
    private SlackService CreateService(HttpClient http, string webhookUrl = "https://hooks.slack.com/test")
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Slack:WebhookUrl", webhookUrl }
            })
            .Build();
        return new SlackService(config, http);
    }

    [TestMethod]
    public async Task NotifyHolidayAdded_ShouldSendSlackMessage()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
        var http = new HttpClient(handler);
        var service = CreateService(http);

        await service.NotifyHolidayAdded("Alice", "2024-12-25");

        Assert.AreEqual(1, handler.CallCount);
    }

    [TestMethod]
    public async Task NotifyHolidayAdded_WhenWebhookEmpty_ShouldNotSendMessage()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
        var http = new HttpClient(handler);
        var service = CreateService(http, webhookUrl: "");

        await service.NotifyHolidayAdded("Alice", "2024-12-25");

        Assert.AreEqual(0, handler.CallCount);
    }

    [TestMethod]
    public async Task SendWeeklySummary_WhenNoHolidays_ShouldSendNobodyOffMessage()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
        var http = new HttpClient(handler);
        var service = CreateService(http);

        await service.SendWeeklySummary(new List<string> { "Alice" }, new List<dynamic>());

        Assert.AreEqual(1, handler.CallCount);
    }
}

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    public int CallCount { get; private set; }

    public MockHttpMessageHandler(HttpStatusCode statusCode)
    {
        _statusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        CallCount++;
        return Task.FromResult(new HttpResponseMessage(_statusCode));
    }
}
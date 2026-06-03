namespace HolidayTracker.Services;

public class SlackService
{
    private readonly string? _webhookUrl;
    private readonly HttpClient _http;

    public SlackService(IConfiguration config, HttpClient http)
    {
        _webhookUrl = Environment.GetEnvironmentVariable("SLACK_WEBHOOK_URL")
        ?? config["Slack:WebhookUrl"];
        _http = http;
    }

    public async Task NotifyHolidayAdded(string person, string date)
    {
        var parsed = DateTime.Parse(date);
        var friendly = parsed.ToString("dddd d MMMM yyyy");
        await Send($":beach_with_umbrella: *{person}* has added a holiday on *{friendly}*");
    }

    public async Task SendWeeklySummary(List<string> team, List<dynamic> holidays)
    {
        var monday = GetNextMonday();
        var friday = monday.AddDays(4);

        var offThisWeek = holidays
        .Where(h => {
            var d = DateTime.Parse((string)h.date);
            return d >= monday && d <= friday;
        })
        .GroupBy(h => (string)h.person)
        .ToList();

        if (!offThisWeek.Any())
        {
            await Send(":date: *This week's holidays:* Nobody is off this week :tada:");
            return;
        }

        var lines = offThisWeek.Select(g => {
            var days = g.Select(h => DateTime.Parse((string)h.date).ToString("ddd d MMM")).ToList();
            return $"- *{g.Key}*: {string.Join(", ", days)}";
        });

        var msg = ":date: *This week's holidays:*\n" + string.Join("\n", lines);
        await Send(msg);
    }

    private async Task Send(string message)
    {
        if (string.IsNullOrEmpty(_webhookUrl))
        {
            Console.WriteLine("Slack webhook URL is empty or missing");
            return;
        }
        Console.WriteLine($"Sending Slack message to: {_webhookUrl[..20]}...");
        var payload = System.Text.Json.JsonSerializer.Serialize(new { text = message });
        var response = await _http.PostAsync(_webhookUrl, new StringContent(payload, System.Text.Encoding.UTF8, "application/json"));
        Console.WriteLine($"Slack response: {response.StatusCode}");
    }

    private DateTime GetNextMonday()
    {
        var today = DateTime.Today;
        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        return daysUntilMonday == 0 ? today : today.AddDays(daysUntilMonday);
    }
}
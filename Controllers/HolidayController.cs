using HolidayTracker.Data;
using HolidayTracker.Models;
using HolidayTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace HolidayTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HolidayController : ControllerBase
{
    private readonly HolidayStore _store;
    private readonly SlackService _slack;

    public HolidayController(HolidayStore store, SlackService slack)
    {
        _store = store;
        _slack = slack;
    }

    [HttpGet]
    public IActionResult GetAll() =>
    Ok(new { holidays = _store.GetAll(), team = _store.GetTeam() });

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Holiday holiday)
    {
        _store.Add(holiday);
        await _slack.NotifyHolidayAdded(holiday.Person, holiday.Date);
        return Ok();
    }

    [HttpDelete]
    public IActionResult Remove([FromQuery] string person, [FromQuery] string date)
    {
        _store.Remove(person, date);
        return Ok();
    }

    [HttpPost("team")]
    public IActionResult AddMember([FromBody] string name)
    {
        _store.AddTeamMember(name);
        return Ok();
    }

    [HttpDelete("team")]
    public IActionResult RemoveMember([FromQuery] string name)
    {
        _store.RemoveTeamMember(name);
        return Ok();
    }
}
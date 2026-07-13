using DiLifeCycleDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace YourProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiTestController : ControllerBase
{
    private readonly ICounter _counterFromController;
    private readonly ISomeService _someService;

    // 同時注入計數器與另一個服務
    public DiTestController(ICounter counterFromController, ISomeService someService)
    {
        _counterFromController = counterFromController;
        _someService = someService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        // 1. 由 Controller 直接操控的計數器加 1
        _counterFromController.Increment();

        // 2. 由 SomeService 內操控的計數器加 1
        _someService.DoSomething();

        // 3. 回傳兩邊看到的 Guid 與數值
        return Ok(new
        {
            ControllerCounter = new
            {
                Id = _counterFromController.Id,
                Value = _counterFromController.Value
            },
            ServiceCounter = new
            {
                Id = _someService.Counter.Id,
                Value = _someService.Counter.Value
            }
        });
    }
}
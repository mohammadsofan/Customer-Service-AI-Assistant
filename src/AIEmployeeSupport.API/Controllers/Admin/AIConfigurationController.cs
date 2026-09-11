using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIEmployeeSupport.Application.Interfaces.Services;

namespace AIEmployeeSupport.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Route("api/ai/configuration")]
[Authorize(Policy = "AdminOnly")]
public class AIConfigurationController : ControllerBase
{
    private readonly IAIConfigurationService _configurationService;

    public AIConfigurationController(IAIConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    [HttpGet]
    public IActionResult Get() => Ok();

    [HttpPut]
    [HttpPost]
    public IActionResult Update() => Ok();
}

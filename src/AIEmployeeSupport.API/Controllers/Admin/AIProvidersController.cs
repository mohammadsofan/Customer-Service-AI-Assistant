using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIEmployeeSupport.Application.Interfaces.Services;

namespace AIEmployeeSupport.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Route("api/ai/providers")]
[Authorize(Policy = "AdminOnly")]
public class AIProvidersController : ControllerBase
{
    private readonly IAIProviderService _providerService;

    public AIProvidersController(IAIProviderService providerService)
    {
        _providerService = providerService;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok();

    [HttpPost]
    public IActionResult Create() => Ok();

    [HttpPut("{id}")]
    public IActionResult Update(Guid id) => Ok();

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id) => Ok();

    [HttpPost("{id}/test")]
    public IActionResult TestConnection(Guid id) => Ok();
}

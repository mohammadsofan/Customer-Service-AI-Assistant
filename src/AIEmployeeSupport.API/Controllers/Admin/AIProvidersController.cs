using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AIEmployeeSupport.Application.Interfaces.Services;
namespace AIEmployeeSupport.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
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
    public IActionResult Update(int id) => Ok();

    [HttpDelete("{id}")]
    public IActionResult Delete(int id) => Ok();

    [HttpPost("{id}/test")]
    public IActionResult Test(int id) => Ok();

    [HttpPatch("{id}/activate")]
    public IActionResult Activate(int id) => Ok();
}

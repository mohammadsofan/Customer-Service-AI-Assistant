using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AIEmployeeSupport.Application.Interfaces.Services;
namespace AIEmployeeSupport.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AIModelsController : ControllerBase
{
    private readonly IAIModelService _modelService;

    public AIModelsController(IAIModelService modelService)
    {
        _modelService = modelService;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok();

    [HttpPost]
    public IActionResult Create() => Ok();

    [HttpPut("{id}")]
    public IActionResult Update(int id) => Ok();

    [HttpDelete("{id}")]
    public IActionResult Delete(int id) => Ok();
}

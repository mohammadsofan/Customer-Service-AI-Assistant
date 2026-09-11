using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AIEmployeeSupport.Application.Interfaces.Services;
namespace AIEmployeeSupport.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class QuestionsController : ControllerBase
{
    private readonly ISupportService _supportService;

    public QuestionsController(ISupportService supportService)
    {
        _supportService = supportService;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok();

    [HttpGet("{id}")]
    public IActionResult GetById(int id) => Ok();
}

using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController(IFileService fileService, IConfiguration configuration) : ControllerBase
{
    private const string AcceptedRoles = "SuperAdmin,Administrator";

    [HttpPost("upload")]
    [Authorize(Roles = AcceptedRoles)]
    public async Task<IActionResult> Upload(IFormFile? file)
    {
        if (file?.Length == 0)
            return BadRequest("No file provided");

        try
        {
            var fileName = await fileService.SaveFileAsync(file!);
            var fullUrl = $"{configuration.GetValue<string>("CDN:ApiPath")}/{fileName}";
            return Ok(new { FileName = fileName, Url = fullUrl });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch
        {
            return StatusCode(500, "Error uploading file");
        }
    }

    [HttpGet("{fileName}")]
    public async Task<IActionResult> Get(string fileName)
    {
        var fileResult = await fileService.GetFileAsync(fileName);

        if (fileResult == null)
            return NotFound();

        return File(fileResult.Data, fileResult.ContentType, fileResult.FileName);
    }

    [HttpDelete("{fileName}")]
    [Authorize(Roles = AcceptedRoles)]
    public async Task<IActionResult> Delete(string fileName)
    {
        var deleted = await fileService.DeleteFileAsync(fileName);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpHead("{fileName}")]
    public IActionResult Exists(string fileName)
    {
        return fileService.FileExists(fileName) ? Ok() : NotFound();
    }
}
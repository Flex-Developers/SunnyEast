using Application.Contract.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Authorize(Roles = ApplicationRoles.Administrator)]
public class ImageController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Upload([FromBody] string imageBase64, [FromQuery] string imageType)
    {
        var bytes = Convert.FromBase64String(imageBase64);
        var imageName = $"{Guid.NewGuid():N}.{imageType}";
        var fileStream =
            new FileStream(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sunnyEastImages",
                    imageName), FileMode.Create);
        await fileStream.WriteAsync(bytes);
        return Ok(imageName);
    }

    [HttpGet]
    public Task<IActionResult> Get([FromRoute] string name)
    {
        var fileStream =
            new FileStream(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sunnyEastImages", name),
                FileMode.Open);
        return Task.FromResult<IActionResult>(File(fileStream, "image", fileDownloadName: name));
    }
}
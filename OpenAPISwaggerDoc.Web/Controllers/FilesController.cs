using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace OpenAPISwaggerDoc.Web.Controllers;

[ApiController]
//[ApiExplorerSettings(GroupName = "v2")]
[Produces("application/json")]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly ILogger<FilesController> _logger;

    public FilesController(ILogger<FilesController> logger) => _logger = logger;

    /// <summary>
    ///     Download a file. This demo will generate a txt file.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}", Name = "Download a File by FileID")]
    public IActionResult Download(int id) =>
        File(Encoding.ASCII.GetBytes("hello world"), "text/plain",
             Invariant($"test-{id}.txt"));

    /// <summary>
    ///     Upload a file. This demo is dummy and only waits 2 seconds.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("single-file")]
    public async Task Upload(IFormFile file)
    {
        if (file == null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        _logger.LogInformation("validating the file {FileName}", file.FileName);
        _logger.LogInformation("saving file");
        await Task.Delay(2000); // validate file type/format/size, scan virus, save it to a storage
        _logger.LogInformation("file saved.");
    }

    /// <summary>
    ///     Upload two files. This demo is dummy and only waits 2 seconds.
    /// </summary>
    /// <param name="file1"></param>
    /// <param name="file2"></param>
    /// <returns></returns>
    [HttpPost("two-files")]
    public async Task Upload(IFormFile file1, IFormFile file2)
    {
        if (file1 == null)
        {
            throw new ArgumentNullException(nameof(file1));
        }

        if (file2 == null)
        {
            throw new ArgumentNullException(nameof(file2));
        }

        _logger.LogInformation("validating the file {FileName}", file1.FileName);
        _logger.LogInformation("validating the file {FileName}", file2.FileName);
        _logger.LogInformation("saving files");
        await Task.Delay(2000);
        _logger.LogInformation("files saved.");
    }

    /// <summary>
    ///     Upload multiple files. This demo is dummy and only waits 2 seconds.
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    [HttpPost("multiple-files")]
    public async Task Upload(List<IFormFile> files)
    {
        if (files == null)
        {
            throw new ArgumentNullException(nameof(files));
        }
        // todo: Currently not working due to an issue in swagger-ui (https://github.com/swagger-api/swagger-ui/issues/4600)
        // todo: Can also follow this issue https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1029

        _logger.LogInformation("validating {Count} files", files.Count);
        _logger.LogInformation("saving files");
        await Task.Delay(2000);
        _logger.LogInformation("files saved.");
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<bool> Delete(int id)
    {
        _logger.LogInformation("deleting file ID=[{Id}]", id);
        await Task.Delay(1500);
        return true;
    }

    /// <summary>
    ///     Submit a form which contains a key-value pair and a file.
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <param name="form">A form which contains the FormId and a file</param>
    /// <returns></returns>
    [HttpPost("{id:int}/forms")]
    [ProducesResponseType(typeof(FormSubmissionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FormSubmissionResult>> SubmitForm(int id, [FromForm] StudentForm form)
    {
        if (form == null)
        {
            throw new ArgumentNullException(nameof(form));
        }

        _logger.LogInformation("validating the form#{FormId} for Student ID={Id}", form.FormId, id);
        _logger.LogInformation("saving file [{FileName}]", form.StudentFile.FileName);
        await Task.Delay(1500);
        _logger.LogInformation("file saved.");
        var result = new FormSubmissionResult { FormId = form.FormId, StudentId = id };
        return CreatedAtAction(nameof(ViewForm), new { id, form.FormId }, result);
    }

    /// <summary>
    ///     View a form
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <param name="formId">Form ID</param>
    /// <returns></returns>
    [HttpGet("{id:int}/forms/{formId:int}")]
    [ProducesResponseType(typeof(FormSubmissionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<FormSubmissionResult> ViewForm(int id, int formId)
    {
        _logger.LogInformation("viewing the form#{FormId} for Student ID={Id}", formId, id);
        await Task.Delay(1000);
        return new FormSubmissionResult { FormId = formId, StudentId = id };
    }
}
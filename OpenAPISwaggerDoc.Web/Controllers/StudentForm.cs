namespace OpenAPISwaggerDoc.Web.Controllers;

public class StudentForm
{
    [Required] public int FormId { get; set; }
    [Required] public IFormFile StudentFile { get; set; }
}
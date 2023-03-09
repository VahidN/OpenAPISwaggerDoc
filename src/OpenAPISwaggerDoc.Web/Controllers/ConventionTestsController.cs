using Microsoft.AspNetCore.Mvc;
using OpenAPISwaggerDoc.Web.AppConventions;

namespace OpenAPISwaggerDoc.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[ApiConventionType(typeof(CustomConventions))]
//[ApiConventionType(typeof(DefaultApiConventions))]
public class ConventionTestsController : ControllerBase
{
    // GET: api/ConventionTests
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new[] { "value1", "value2" };
    }

    // GET: api/ConventionTests/5
    [HttpGet("{id}", Name = "Get")]
    //[ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    public string Get(int id) => "value";

    // POST: api/ConventionTests
    [HttpPost]
    //[ApiConventionMethod(typeof(CustomConventions), nameof(CustomConventions.Insert))]
    public void InsertTest([FromBody] string value)
    {
    }

    // PUT: api/ConventionTests/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE: api/ApiWithActions/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
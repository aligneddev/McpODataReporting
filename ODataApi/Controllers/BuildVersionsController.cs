using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ODataApi.Controllers;

public class BuildVersionsController(ReportingDbContext context) : ODataController
{
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(context.BuildVersions);
    }

    [EnableQuery]
    public async Task<IActionResult> Get([FromRoute] byte key)
    {
        var buildVersion = await context.BuildVersions.FirstOrDefaultAsync(b => b.SystemInformationId == key);
        
        if (buildVersion == null)
        {
            return NotFound();
        }
        
        return Ok(buildVersion);
    }
}

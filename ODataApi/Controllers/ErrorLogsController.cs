using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ODataApi.Controllers;

public class ErrorLogsController(ReportingDbContext context) : ODataController
{
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(context.ErrorLogs);
    }

    [EnableQuery]
    public async Task<IActionResult> Get([FromRoute] int key)
    {
        var errorLog = await context.ErrorLogs.FirstOrDefaultAsync(e => e.ErrorLogId == key);
        
        if (errorLog == null)
        {
            return NotFound();
        }
        
        return Ok(errorLog);
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ODataApi.Controllers;

public class SalesOrderHeadersController(ReportingDbContext context) : ODataController
{
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(context.SalesOrderHeaders);
    }

    [EnableQuery]
    public async Task<IActionResult> Get([FromRoute] int key)
    {
        var salesOrderHeader = await context.SalesOrderHeaders.FirstOrDefaultAsync(soh => soh.SalesOrderId == key);
        
        if (salesOrderHeader == null)
        {
            return NotFound();
        }
        
        return Ok(salesOrderHeader);
    }
}

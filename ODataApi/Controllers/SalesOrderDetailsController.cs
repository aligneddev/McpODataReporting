using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Formatter;
using ODataApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ODataApi.Controllers;

public class SalesOrderDetailsController(ReportingDbContext context) : ODataController
{
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(context.SalesOrderDetails);
    }

    [EnableQuery]
    public async Task<IActionResult> Get([FromODataUri] int keySalesOrderId, [FromODataUri] int keySalesOrderDetailId)
    {
        var salesOrderDetail = await context.SalesOrderDetails
            .FirstOrDefaultAsync(sod => sod.SalesOrderId == keySalesOrderId 
                                && sod.SalesOrderDetailId == keySalesOrderDetailId);
        
        if (salesOrderDetail == null)
        {
            return NotFound();
        }
        
        return Ok(salesOrderDetail);
    }
}

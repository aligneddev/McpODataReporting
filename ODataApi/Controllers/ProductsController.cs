using McpODataReporting.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using ODataApi.Data;

namespace ODataApi.Controllers;

public class ProductsController(ReportingDbContext context) : ODataController
{
    [EnableQuery]
    public async Task<IActionResult>  Get()
    {
        return Ok(context.Products);
    }

    [EnableQuery]
    public IActionResult Get([FromRoute] int key)
    {
        var product = context.Products.FirstOrDefault(p => p.ProductId == key);
        
        if (product == null)
        {
            return NotFound();
        }
        
        return Ok(product);
    }
}

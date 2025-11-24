using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ODataApi.Controllers;

public class ProductModelsController(ReportingDbContext context) : ODataController
{
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(context.ProductModels);
    }

    [EnableQuery]
    public async Task<IActionResult> Get([FromRoute] int key)
    {
        var productModel = await context.ProductModels.FirstOrDefaultAsync(pm => pm.ProductModelId == key);
        
        if (productModel == null)
        {
            return NotFound();
        }
        
        return Ok(productModel);
    }
}

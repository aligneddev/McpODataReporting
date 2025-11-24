using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Formatter;
using ODataApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ODataApi.Controllers;

public class ProductModelProductDescriptionsController(ReportingDbContext context) : ODataController
{
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(context.ProductModelProductDescriptions);
    }

    [EnableQuery]
    public async Task<IActionResult> Get([FromODataUri] int keyProductModelId, [FromODataUri] int keyProductDescriptionId, [FromODataUri] string keyCulture)
    {
        var entity = await context.ProductModelProductDescriptions
            .FirstOrDefaultAsync(pm => pm.ProductModelId == keyProductModelId 
                               && pm.ProductDescriptionId == keyProductDescriptionId 
                               && pm.Culture == keyCulture);
        
        if (entity == null)
        {
            return NotFound();
        }
        
        return Ok(entity);
    }
}

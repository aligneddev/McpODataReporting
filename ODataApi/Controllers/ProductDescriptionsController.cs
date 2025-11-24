using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ODataApi.Controllers;

public class ProductDescriptionsController(ReportingDbContext context) : ODataController
{
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(context.ProductDescriptions);
    }

    [EnableQuery]
    public async Task<IActionResult> Get([FromRoute] int key)
    {
        var productDescription = await context.ProductDescriptions.FirstOrDefaultAsync(pd => pd.ProductDescriptionId == key);
        
        if (productDescription == null)
        {
            return NotFound();
        }
        
        return Ok(productDescription);
    }
}

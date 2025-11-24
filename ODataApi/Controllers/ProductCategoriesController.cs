using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ODataApi.Controllers;

public class ProductCategoriesController(ReportingDbContext context) : ODataController
{
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(context.ProductCategories);
    }

    [EnableQuery]
    public async Task<IActionResult> Get([FromRoute] int key)
    {
        var productCategory = await context.ProductCategories.FirstOrDefaultAsync(pc => pc.ProductCategoryId == key);
        
        if (productCategory == null)
        {
            return NotFound();
        }
        
        return Ok(productCategory);
    }
}

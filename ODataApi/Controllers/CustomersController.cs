using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ODataApi.Controllers;

public class CustomersController(ReportingDbContext context) : ODataController
{
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(context.Customers);
    }

    [EnableQuery]
    public async Task<IActionResult> Get([FromRoute] int key)
    {
        var customer = await context.Customers.FirstOrDefaultAsync(c => c.CustomerId == key);
        
        if (customer == null)
        {
            return NotFound();
        }
        
        return Ok(customer);
    }
}

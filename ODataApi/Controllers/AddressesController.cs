using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ODataApi.Controllers;

public class AddressesController(ReportingDbContext context) : ODataController
{
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(context.Addresses);
    }

    [EnableQuery]
    public async Task<IActionResult> Get([FromRoute] int key)
    {
        var address = await context.Addresses.FirstOrDefaultAsync(a => a.AddressId == key);
        
        if (address == null)
        {
            return NotFound();
        }
        
        return Ok(address);
    }
}

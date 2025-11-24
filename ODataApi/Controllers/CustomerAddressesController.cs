using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Formatter;
using ODataApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ODataApi.Controllers;

public class CustomerAddressesController(ReportingDbContext context) : ODataController
{
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(context.CustomerAddresses);
    }

    [EnableQuery]
    public async Task<IActionResult> Get([FromODataUri] int keyCustomerId, [FromODataUri] int keyAddressId)
    {
        var customerAddress = await context.CustomerAddresses
            .FirstOrDefaultAsync(ca => ca.CustomerId == keyCustomerId && ca.AddressId == keyAddressId);
        
        if (customerAddress == null)
        {
            return NotFound();
        }
        
        return Ok(customerAddress);
    }
}

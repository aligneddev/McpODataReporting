using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataApi.Data;

namespace ODataApi.Controllers;

public class VProductModelCatalogDescriptionsController(ReportingDbContext context) : ODataController
{
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(context.VProductModelCatalogDescriptions);
    }
}

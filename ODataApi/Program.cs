using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using ODataApi.Data;
using ODataApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add DbContext for Azure SQL
builder.Services.AddDbContext<ReportingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ReportingDb")));

// Add OData
builder.Services.AddControllers().AddOData(opt =>
{
    var odataBuilder = new ODataConventionModelBuilder();
    
    // Register all entity sets
    odataBuilder.EntitySet<Address>("Addresses");
    odataBuilder.EntitySet<BuildVersion>("BuildVersions");
    odataBuilder.EntitySet<Customer>("Customers");
    odataBuilder.EntitySet<CustomerAddress>("CustomerAddresses");
    odataBuilder.EntitySet<ErrorLog>("ErrorLogs");
    odataBuilder.EntitySet<Product>("Products");
    odataBuilder.EntitySet<ProductCategory>("ProductCategories");
    odataBuilder.EntitySet<ProductDescription>("ProductDescriptions");
    odataBuilder.EntitySet<ProductModel>("ProductModels");
    odataBuilder.EntitySet<ProductModelProductDescription>("ProductModelProductDescriptions");
    odataBuilder.EntitySet<SalesOrderDetail>("SalesOrderDetails");
    odataBuilder.EntitySet<SalesOrderHeader>("SalesOrderHeaders");
    
    // Configure view entities with explicit keys
    var vGetAllCategory = odataBuilder.EntitySet<VGetAllCategory>("VGetAllCategories").EntityType;
    vGetAllCategory.HasKey(e => e.ProductCategoryId);
    
    var vProductAndDescription = odataBuilder.EntitySet<VProductAndDescription>("VProductAndDescriptions").EntityType;
    vProductAndDescription.HasKey(e => new { e.ProductId, e.Culture });
    
    var vProductModelCatalog = odataBuilder.EntitySet<VProductModelCatalogDescription>("VProductModelCatalogDescriptions").EntityType;
    vProductModelCatalog.HasKey(e => e.ProductModelId);
    
    opt.AddRouteComponents("/odata", odataBuilder.GetEdmModel());
    opt.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100);
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.Run();

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using McpODataReporting.Data;
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
    odataBuilder.EntitySet<Product>("Products");
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

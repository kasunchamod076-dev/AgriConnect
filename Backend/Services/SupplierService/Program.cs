using AgriConnect.Hosting;
using Scalar.AspNetCore;
using SupplierService.Data;
using SupplierService.IServices;
using SupplierService.Services;

var builder = WebApplication.CreateBuilder(
    new WebApplicationOptions { Args = args, ContentRootPath = AppContext.BaseDirectory }
);

// Database: every role writes only to its own database.
builder.Services.AddDbContext<SupplierDbContext>(options =>
    ApiSetup.UseOwnedDatabase(options, builder.Configuration, "Supplier")
);

// JWT authentication and service-to-service HTTP.
builder.AddAgriHttp();
builder.Services.AddAgriAuthentication(builder.Configuration);
builder.Services.AddScoped<ISessionValidator, RemoteSessionValidator>();

// Dependency injection: one interface and service per feature.
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IQuotationService, QuotationService>();
builder.Services.AddScoped<ISupplierAuthService, SupplierAuthService>();
builder.Services.AddScoped<ISupplierDashboardService, SupplierDashboardService>();

// Controllers and API documentation.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
var app = builder.Build();
await app.PrepareDatabaseAsync<SupplierDbContext>();
app.UseMiddleware<ApiErrorMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
if (app.Environment.IsDevelopment() || app.Configuration["DemoMode"] == "1")
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
await app.RunAsync();

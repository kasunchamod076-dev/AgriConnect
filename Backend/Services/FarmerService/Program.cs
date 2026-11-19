using AgriConnect.Hosting;
using FarmerService.Data;
using FarmerService.IServices;
using FarmerService.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(
    new WebApplicationOptions { Args = args, ContentRootPath = AppContext.BaseDirectory }
);

// Database: every role writes only to its own database.
builder.Services.AddDbContext<FarmerDbContext>(options =>
    ApiSetup.UseOwnedDatabase(options, builder.Configuration, "Farmer")
);

// JWT authentication and service-to-service HTTP.
builder.AddAgriHttp();
builder.Services.AddAgriAuthentication(builder.Configuration);
builder.Services.AddScoped<ISessionValidator, RemoteSessionValidator>();

// Dependency injection: one interface and service per feature.
builder.Services.AddScoped<ICropListingService, CropListingService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IFarmerAuthService, FarmerAuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ISupplyRequestService, SupplyRequestService>();

// Controllers and API documentation.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
var app = builder.Build();
await app.PrepareDatabaseAsync<FarmerDbContext>();
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
using CargoLink.ModernApi.Middleware;
using CargoLink.ModernApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CargoLink API",
        Version = "v1",
        Description = "Modern RESTful API for the CargoLink shipping platform"
    });
});

// Register application services
builder.Services.AddSingleton<IRateService, RateService>();
builder.Services.AddSingleton<ShipmentService>();
builder.Services.AddSingleton<IShipmentService>(sp => sp.GetRequiredService<ShipmentService>());
builder.Services.AddSingleton<ITrackingService, TrackingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CargoLink API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

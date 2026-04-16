using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CargoLink.ModernApi.Middleware;
using CargoLink.ModernApi.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Configure Swagger with XML comments
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CargoLink API",
        Version = "v1",
        Description = "Modern RESTful API for the CargoLink shipping platform",
        Contact = new OpenApiContact
        {
            Name = "CargoLink Engineering",
            Email = "api-support@cargolink.example.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT"
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
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
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"CargoLink API {description.GroupName.ToUpperInvariant()}");
        }
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

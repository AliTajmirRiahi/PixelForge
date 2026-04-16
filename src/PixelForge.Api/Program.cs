using Microsoft.OpenApi;
using PixelForge.Application;
using PixelForge.Infrastructure;
using PixelForge.Api.Filters.Swagger;


var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------
// 1) Configure Services (IOC Container)
// -------------------------------------------------------

// Logging
builder.Services.AddLogging();

// Controllers or Minimal APIs
builder.Services.AddControllers();

// OpenAPI (built-in in .NET 8–10)
builder.Services.AddOpenApi();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // It shows version like this v1.0 or v1
    options.SubstituteApiVersionInUrl = true; // On Swagger variable {version} replace with real value (example v1)
});

// Health Checks
builder.Services.AddHealthChecks();

// CORS (Highly recommended)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register Application Services (Clean Architecture)
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHttpClient();

builder.Services.AddSwaggerGen(options =>
{
    options.ParameterFilter<OptionalRouteParameterFilter>();
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "PixelForge V1", Version = "v1" });
    //options.SwaggerDoc("v2", new OpenApiInfo { Title = "PixelForge V2", Version = "v2" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(new Swashbuckle.AspNetCore.Swagger.SwaggerOptions()
    {
        OpenApiVersion = OpenApiSpecVersion.OpenApi3_1
    });
    app.UseSwaggerUI(options =>
    {
        // Show diffrent versions in Swagger's drop down 
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PixelForge V1 Docs");
        //options.SwaggerEndpoint("/swagger/v2/swagger.json", "V2 Docs");
    });
}

// -------------------------------------------------------
// 2) Configure Middleware Pipeline
// -------------------------------------------------------

// Exception handler (global, production-ready)
app.UseExceptionHandler("/error");

// HSTS (production)
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// HTTPS Redirect
app.UseHttpsRedirection();

// CORS
app.UseCors("Default");

// Routing
app.UseRouting();

// Authorization
app.UseAuthorization();

// -------------------------------------------------------
// OpenAPI + Swagger UI
// -------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();        // exposes /openapi/v1.json
}
else
{
    // In production we usually still want OpenAPI (optional)
    app.MapOpenApi();
}

// Map Controllers / Minimal APIs
app.MapControllers();

// Health Checks Endpoint
app.MapHealthChecks("/health");

// Run application
app.Run();


// -------------------------------------------------------
// Optional Minimal Error Endpoint
// -------------------------------------------------------
app.Map("/error", (HttpContext http) =>
{
    return Results.Problem("An unexpected error occurred.");
});

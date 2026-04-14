using Microsoft.AspNetCore.OpenApi;

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
//builder.Services.AddApplicationServices();
//builder.Services.AddInfrastructureServices(builder.Configuration);


var app = builder.Build();

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

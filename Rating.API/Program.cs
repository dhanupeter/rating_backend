using Rating.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Core Services
builder.Services.AddSingleton<ISpannerService, SpannerService>();
builder.Services.AddSingleton<INotificationService, FirebaseNotificationService>();
builder.Services.AddSingleton<ILocationService, GeoapifyLocationService>();
builder.Services.AddSingleton<IAuditLogService, AuditLogService>();

// CORS configuration for Flutter mobile & web clients
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure Swagger for all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rate Anything API v1");
});

app.UseCors("AllowAll");
app.UseAuthorization();

// Root & Health Endpoints
app.MapGet("/", () => Results.Ok(new
{
    service = "Rate Anything API",
    status = "Running",
    version = "1.0.0",
    environment = app.Environment.EnvironmentName,
    endpoints = new[]
    {
        "/swagger",
        "/health",
        "/health/database",
        "/api/entities",
        "/api/issues",
        "/api/criteria?entityType=PLACE",
        "/api/location/search?query=Coimbatore"
    },
    timestamp = DateTime.UtcNow
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "Rate Anything API",
    timestamp = DateTime.UtcNow
}));

app.MapGet("/health/database", async (ISpannerService spanner) =>
{
    try
    {
        var entities = await spanner.GetAllEntitiesAsync();
        return Results.Ok(new
        {
            status = "Connected",
            database = "rating",
            instance = "event-spanner",
            project = "event-506117",
            entitiesCount = entities.Count,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            title: "Spanner Health Check Failed",
            statusCode: 500
        );
    }
});

app.MapControllers();

app.Run();

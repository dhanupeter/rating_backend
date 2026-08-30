using Rating.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Cloud Spanner / Repository Service
builder.Services.AddSingleton<ISpannerService, SpannerService>();

// Enable CORS for mobile & web clients
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

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rate Anything API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger at root /
});

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();

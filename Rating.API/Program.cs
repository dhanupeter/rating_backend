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

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();

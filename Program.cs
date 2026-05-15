using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using QuestPDF.Infrastructure;

using FarmManagement.API.Data;
using FarmManagement.API;
using FarmManagement.API.Helpers;
using FarmManagement.API.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// Add Services
// =====================================================

builder.Services.AddDbContext<FarmDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<EvaluationService>();
builder.Services.AddScoped<BonusService>();

builder.Services.AddScoped<
    IEggProductionReportExportService,
    EggProductionReportExportService>();

builder.Services.AddScoped<
IFeedConsumptionReportExportService,
FeedConsumptionReportExportService>();


builder.Services.AddScoped<
    IDailyRecordExportService,
    DailyRecordExportService>();

// =====================================================
// Controllers + JSON
// =====================================================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// =====================================================
// CORS
// =====================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// =====================================================
// Swagger
// =====================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =====================================================
// QuestPDF License
// =====================================================

QuestPDF.Settings.License = LicenseType.Community;

// =====================================================
// Build App
// =====================================================

var app = builder.Build();

// =====================================================
// Forwarded Headers (Proxy)
// =====================================================

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

// =====================================================
// Middleware
// =====================================================

// CORS
app.UseCors("AllowAll");

// Swagger
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "Farm Management API V1");

    c.RoutePrefix = "swagger";
});

// HTTPS
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// =====================================================
// Seed Data
// =====================================================

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider.GetRequiredService<FarmDbContext>();

    await SeedData.Initialize(context);
}

// =====================================================
// Run App
// =====================================================

app.Urls.Add("http://localhost:5112");

app.Run();
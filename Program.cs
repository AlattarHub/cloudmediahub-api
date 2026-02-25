using CloudMediaHub.Api;
using CloudMediaHub.Api.Configuration;
using CloudMediaHub.Api.Data;
using CloudMediaHub.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<AzureStorageSettings>(builder.Configuration.GetSection("AzureStorage"));

builder.Services.AddScoped<BlobService>();

builder.Services.Configure<FileValidationSettings>(
    builder.Configuration.GetSection("FileValidation"));

builder.Services.AddScoped<IFileValidator, FileValidator>();

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddApplicationInsightsTelemetry();

// عشان لو نسيت Environment Variable التطبيق يقع فورًا بدل ما يشتغل غلطعشان لو نسيت Environment Variable

var storageAccount = builder.Configuration["AzureStorage:AccountName"];
if (string.IsNullOrEmpty(storageAccount))
{
    throw new Exception("AzureStorage:AccountName is not configured.");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

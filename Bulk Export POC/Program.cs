using Bulk_Export_POC;
using Bulk_Export_POC.Models;
using Bulk_Export_POC.Services;
using Bulk_Export_POC.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSingleton<JobRegistry>();
builder.Services.AddSingleton<QueueService<Job>>();
builder.Services.AddTransient<ResourceJsonExportService>();
builder.Services.AddSingleton<FileProcessorEPPlus>();

builder.Services.AddHostedService<BackgroundJobProcessor>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

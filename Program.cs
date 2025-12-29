using Microsoft.EntityFrameworkCore;
using SimulationRealtimeApp.Data;
using SimulationRealtimeApp.Hubs;
using SimulationRealtimeApp.Repositories;
using SimulationRealtimeApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure SQLite Database
builder.Services.AddDbContext<SimulationDbContext>(options =>
    options.UseSqlite("Data Source=simulation_history.db"));

// Register repository
builder.Services.AddScoped<ISimulationHistoryRepository, SimulationHistoryRepository>();

// Register simulation service as singleton
builder.Services.AddSingleton<SimulationService>();

// Add hosted service for background simulation
builder.Services.AddHostedService<SimulationBackgroundService>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SimulationDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS before other middleware
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Map SignalR hub
app.MapHub<SimulationHub>("/simulationHub");

// Serve static files for the web client
app.UseStaticFiles();

app.Run();

using Hangfire;
using Microsoft.EntityFrameworkCore;
using WebAPIWithHangFire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Create Connection String
var ConStr = builder.Configuration.GetConnectionString("Conx");

//Add Database Service
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(ConStr ?? throw new InvalidOperationException("Connection string not found"));
});

builder.Services.AddHangfire(op => { op.UseSqlServerStorage(ConStr); });
builder.Services.AddHangfireServer();

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

app.UseHangfireDashboard();

app.Run();

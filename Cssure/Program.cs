
using Cssure;

using Cssure.ServiceMqtt;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IRawDataService, RawDataService>();

builder.Services.AddSingleton<IMQTTManager,MQTTManager>();
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}else
{
    // For mobile apps, allow http traffic.
    app.UseHttpsRedirection();
}


//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();



using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;

    var myDependency = services.GetRequiredService<IMQTTManager>();
    myDependency.OpenConncetion();
}

app.Run();

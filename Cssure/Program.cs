using Cssure;
using Cssure.Services;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMQTTService, MqttService>();
builder.Services.AddSingleton<IRawDataService, RawDataService>();
//Very important that MQTTServiceLocalPython is below RawDataService and 
//MqttService is above
builder.Services.AddSingleton<IMQTTService, MQTTServiceLocalPython>();



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

    var myDependency = services.GetRequiredService<IMQTTService>();
    myDependency.OpenConncetion();
}

app.Run();

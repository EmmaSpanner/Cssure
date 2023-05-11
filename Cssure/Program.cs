using Cssure;
using Cssure.Constants;
using Cssure.MongoDB;
using Cssure.MongoDB.Services;
using Cssure.Services;
using Microsoft.AspNetCore.Builder;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<EcgDataDb>(builder.Configuration.GetSection(nameof(EcgDataDb)));


var url = Environment.GetEnvironmentVariable("ASPNETCORE_URLS").Split(";").Last();

builder.Services.AddSingleton <IIpAdresses>(new IpAdresses(url));
builder.Services.AddSingleton<MongoService>();
builder.Services.AddSingleton<ProcessedECGDataService>();
builder.Services.AddSingleton<RawECGDataService>();
builder.Services.AddSingleton<DecodedECGDataService>();

builder.Services.AddSingleton<IBssureMQTTService, MqttService>();
builder.Services.AddSingleton<IRawDataService, RawDataService>();
//Very important that MQTTServiceLocalPython is below RawDataService and 
//MqttService is above
builder.Services.AddSingleton<IPythonMQTTService, MQTTServiceLocalPython>();


builder.Services.AddControllers();


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
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

    var myDependencyPython = services.GetRequiredService<IPythonMQTTService>();
    var myDependencyBssure = services.GetRequiredService<IBssureMQTTService>();
    myDependencyPython.OpenConnection();
    myDependencyBssure.OpenConnection();
}

app.Run();

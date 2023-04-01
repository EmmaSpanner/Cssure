using Cssure.ServiceMqtt;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMQTTManager,MQTTManager > ();
//builder.Services.AddScoped<IMQTTManager, MQTTManager>();
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




//using (var scope = app.Services.CreateScope())
//{
//    var sampleService = scope.ServiceProvider.GetRequiredService<IMQTTManager>();
//    sampleService.StartMQTTService();
//}

using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;

    var myDependency = services.GetRequiredService<IMQTTManager>();
    myDependency.StartMQTTService();
}

app.Run();

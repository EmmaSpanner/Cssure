﻿using Cssure.DTO;
using Cssure.ServiceMqtt;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Cssure.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MqttController : Controller
    {
        private readonly IMQTTManager _MQTTManager;
        public MqttController(IMQTTManager MQTTManager)
        {
            _MQTTManager = MQTTManager;
        }

        [HttpGet("/Senddata")]
        public IActionResult Get(string text)
        {
            bool succes = _MQTTManager.SendMQTTData(text);

            if (succes)
            {
                return Ok(new { message = "Det sku godt"}); //
            }
            else return BadRequest(new {message= "MQTT er ikke connected"});

        }
    }
}

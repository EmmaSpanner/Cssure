using Cssure.Constants;
using Cssure.Models;
using Cssure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Web.Http.Description;

namespace Cssure.Controllers
{
    [Route("api/[controller]")]
    public class RawDataController : ControllerBase
    {
        private readonly IBssureMQTTService mqttService;
        public RawDataController(IBssureMQTTService mqttService)
        {
            this.mqttService = mqttService;
        }

        //Post method used to receive RawData from Patient app
        [HttpPost]
        public async Task<ActionResult> Post()
         {
            //Reads from stream and copies to byte[]
            using (var stream = new MemoryStream())
            {
                await Request.Body.CopyToAsync(stream);
                var bytes = stream.ToArray();

                if (bytes == null)
                {
                    Debug.WriteLine("In RawDataController, failed to receive data");

                    return BadRequest(new { msg = "Failed to receive data." }); // Sends a JSON object
                }

                // TODO:  Her starter dataProccessing når data kommer ind i Cssure
                //RawData is send to backend for decoding and processing
                //await Task.Run(() => rawDataService.ProcessData(bytes));

                mqttService.Publish_RawData(Topics.Topic_Series_TempToBSSURE, bytes); 
                Debug.WriteLine("In RawDataController, received data");

                //If data is received succesfully the method returns an OK (Code 200) to patient App
                return Ok(new { msg = "Data received" });
            }
        }
    }
}

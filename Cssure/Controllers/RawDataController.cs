using Cssure.Models;
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
        private readonly IRawDataService rawDataService;
        public RawDataController(IRawDataService rawDataServie)
        {
            this.rawDataService = rawDataServie;
        }

        //Post method used to receive RawData from Patient app
        [HttpPost]
        public async Task<ActionResult> Post()
         {

            using (var stream = new MemoryStream())
            {
                await Request.Body.CopyToAsync(stream);
                var bytes = stream.ToArray();

                if (bytes == null)
                {
                    Debug.WriteLine("In RawDataController, failed to receive data");

                    return BadRequest(new { msg = "Failed to receive data." }); // Sends a JSON object
                    //return BadRequest("Failed to receive data.");
                }
                ////If the received data is null it returns bad request (Code 400) to patient app
                //if (bytes == null)
                //{
                //    Debug.WriteLine("In RawDataController, failed to receive data");

                //    return BadRequest(new { message = "Failed to receive data." });
                //}

                //RawData is send to backend for decoding and processing
                await Task.Run(() => rawDataService.ProcessData(bytes));



                Debug.WriteLine("In RawDataController, received data");

                //If data is received succesfully the method returns an OK (Code 200) to patient App
                return Ok(new { msg = "Data received" });
            }
        }
    }
}

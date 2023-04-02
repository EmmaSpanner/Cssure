using Cssure.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

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
        public async Task<ActionResult> Post(RawData bytes)
         {

            //If the received data is null it returns bad request (Code 400) to patient app
            if (bytes == null)
            {
                Debug.WriteLine("In RawDataController, failed to receive data");

                return BadRequest(new { message = "Failed to receive data." });
            }

            //RawData is send to backend for decoding and processing
            //_rawDataService.(bytes);



            Debug.WriteLine("In RawDataController, received data");

            //If data is received succesfully the method returns an OK (Code 200) to patient App
            return Ok(new { message = "Data received" });
        }
    }
}

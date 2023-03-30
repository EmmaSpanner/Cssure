using Cssure.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;

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

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new {message = "Everything ok"});
        }

        [HttpPost]
        public ActionResult Post([FromBody] RawData bytes)
         {
            if(bytes == null)
            {
                Debug.WriteLine("In RawDataController, failed to receive data");

                return BadRequest(new { message = "Failed to receive data." });
            }

            //_rawDataService.ProcessData(bytes);



            Debug.WriteLine("In RawDataController, received data");

 
            return Ok(new { message = "Data received" });
        }

        [HttpPost("upload")]
        public IActionResult Upload([FromBody] byte[] data)
        {
            // Handle the byte[] here
            return Ok();
        }
    }
}

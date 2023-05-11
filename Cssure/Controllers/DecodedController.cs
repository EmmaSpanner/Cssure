using Cssure.DTO;
using Cssure.Models;
using Cssure.MongoDB.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cssure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DecodedController : ControllerBase
    {
        public DecodedECGDataService Service { get; }

        public DecodedController(DecodedECGDataService service)
        {
            Service = service;
        }

        // GET: api/<CSIController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ECGBatchData>>> Get()
        {
            var decoded = await Service.getAllAsync();
            return Ok(decoded);
        }

        // GET api/<CSIController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CSIController>
        [HttpPost]
        public async Task<IActionResult> Post(ECGBatchData value)
        {
            await Service.postDecoded(value);
            return Ok(value);
        }

        // PUT api/<DecodedController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<DecodedController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

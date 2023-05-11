using Cssure.DTO;
using Cssure.MongoDB.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cssure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CSIController : ControllerBase
    {
        public ProcessedECGDataService Service { get; }

        public CSIController(ProcessedECGDataService service)
        {
            Service = service;
        }

        // GET: api/<CSIController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CSI_DTO>>> Get()
        {
            var csi = await Service.getAllAsync();
            return Ok(csi);
        }

        // GET api/<CSIController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CSIController>
        [HttpPost]
        public async Task<IActionResult> Post(CSI_DTO value)
        {
            await Service.postCSI(value);
            return Ok(value);
        }

        // PUT api/<CSIController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CSIController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

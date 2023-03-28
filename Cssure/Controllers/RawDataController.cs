using Cssure.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;

namespace Cssure.Controllers
{
    public class RawDataController
    {
        [HttpPost]
        public HttpResponseMessage Post([FromBody] byte[] bytes)
        {
            if(bytes == null)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            RawData rawData = new RawData(bytes);

            Debug.WriteLine("In RawDataController, RawData: " + rawData);

            var response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;
            
            Debug.WriteLine("In RawDataController, response on HTTP: " + response);

            return response;
        }
    }
}

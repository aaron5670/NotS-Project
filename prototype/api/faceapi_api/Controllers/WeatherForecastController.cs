using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace faceapi_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        [HttpGet("/{name}/{id}")]
        public IActionResult Get(string name, string id)
        {
            try
            {
                byte[] file = System.IO.File.ReadAllBytes($@".\\images\\{name}\\{id}");
                return File(file, "image/jpg");
            } catch (Exception e)
            {
                return NotFound(e.Message);
            }

        }

        [HttpGet("/wow")]
        public IActionResult GetWow()
        {
            return Ok("wow");
        }
    }
}

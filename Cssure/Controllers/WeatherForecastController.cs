using Cssure.DTO;
using Microsoft.AspNetCore.Mvc;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace Cssure.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        //[HttpGet(Name = "GetWeatherForecast")]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateTime.Now.AddDays(index),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}


        /// <summary>
        /// Denne metode er lavede vha. https://stackoverflow.com/questions/49082312/activating-conda-environment-from-c-sharp-code-or-what-is-the-differences-betwe
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<PyDTO> Get()
        {
            List<string> names = doPython();

            int start = 8; // Der er 8 start linjer, som procesStartInfo
            int end = names.Count - start -2 ;   //Der er 2 linjer til sidst som indikere at mappen for det var kørt fra
            return Enumerable.Range(start, end).Select(index => new PyDTO
            {
                Number = index,
                Text = names[index]
            })
            .ToArray();


            //return new PyDTO
            //{
            //    Number = 1,
            //    Text = string.Join("\n", doPython())
            //};
        }
        private static List<string> doPython()
        {
            var workingDirectory = Path.GetFullPath("Scripts");
            //ScriptEngine engine = Python.CreateEngine();
            //engine.ExecuteFile(@$"{workingDirectory}\\Test.py");


            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = workingDirectory
                }
            };
            process.Start();
            // Pass multiple commands to cmd.exe
            using (var sw = process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    // Vital to activate Anaconda
                    sw.WriteLine("C:\\Users\\madsn\\AppData\\Roaming\\.anaconda\\navigator\\.anaconda\\navigator\\scripts\\activate.bat");
                    // Activate your environment
                    sw.WriteLine("activate BME_MAL");
                    // Any other commands you want to run
                    //sw.WriteLine("set KERAS_BACKEND=tensorflow");
                    // run your script. You can also pass in arguments
                    sw.WriteLine(@$"python {workingDirectory}\\Test2.py");
                }
            }

            List<string> list = new List<string>();
            // read multiple output lines
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                list.Add(line);
                System.Diagnostics.Debug.WriteLine(line);
            }


            return list;
        }
    }
}
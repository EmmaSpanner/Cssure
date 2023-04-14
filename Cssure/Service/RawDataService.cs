using Cssure.Models;
using Cssure.Service;
using static System.Net.Mime.MediaTypeNames;

namespace Cssure.Service
{
    public class RawDataService : IRawDataService
    {

        private readonly IMQTTService mqttService;
        public RawDataService(IMQTTService MQTTManager)
        {
            mqttService = MQTTManager;
        }
        public void ProcessData(byte[] bytes)
        {

            // TODO: Decodeing kan være her
            // Decoded signal
            Console.WriteLine(bytes.Length);
            mqttService.Publish_RawData(10012);
        }
    }
}

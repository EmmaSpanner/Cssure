using Cssure.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Cssure.Services
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
            mqttService.Publish_RawData(Topics.Topic_Series_Raw, bytes);
        }
    }
}

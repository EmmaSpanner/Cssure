using Cssure.Constants;
using Cssure.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Cssure.Services
{
    public class RawDataService : IRawDataService
    {
        private readonly IMQTTService mqttService;
        public RawDataService(IPythonMQTTService MQTTManager)
        {
            mqttService = MQTTManager;
        }
        public void ProcessData(byte[] bytes)
        {
            // Converting to signed bytes
            sbyte[] signedBytes = (sbyte[])(Array)bytes;
            sbyte[] SignedBytes = Array.ConvertAll(bytes, x => unchecked((sbyte)x));
            
            // Decoding bytes to ECGData
            byte[] ecgData = DecodingBytes.DecodingByteArray.DecodeBytes(SignedBytes);
            
            // Publish ECGData to MQTT
            //mqttService.Publish_RawData(Topics.Topic_Series_Raw, bytes);
        }
    }
}

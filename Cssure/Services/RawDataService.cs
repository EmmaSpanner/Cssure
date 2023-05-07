using Cssure.Constants;
using Cssure.DTO;
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
        public void ProcessData(EKGSampleDTO eKGSample)
        {

            var bytes = eKGSample.rawBytes;
            // TODO: Decodeing kan være her
            // Decoded signal
            sbyte[] signedBytes = (sbyte[]) (Array)bytes;
            sbyte[] SignedBytes = Array.ConvertAll(bytes,x => unchecked((sbyte)x));


            byte[] ecgdata = DecodingBytes.DecodingByteArray.DecodeBytes(SignedBytes);
            mqttService.Publish_RawData(Topics.Topic_Series_Raw, ecgdata);
        }
    }
}

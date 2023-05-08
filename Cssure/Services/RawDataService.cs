using Cssure.Constants;
using Cssure.DTO;
using Cssure.Models;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Cssure.Services
{
    public class RawDataService : IRawDataService
    {

        private readonly IMQTTService mqttService;
        
        // Buffering
        private int nBufferSamples = 256 * 60 * 3; //45000 samples, 3750 batches
        private ECGBatchSeriesData bufferedECG = new ECGBatchSeriesData()
        {
            ECGChannel1 = new List<int[]>(),
            ECGChannel2 = new List<int[]>(),
            ECGChannel3 = new List<int[]>(),
            TimeStamp = new List<DateTimeOffset>(),
            Samples = 0
        };

        public RawDataService(IPythonMQTTService MQTTManager)
        {
            mqttService = MQTTManager;
        }
        public void ProcessData(EKGSampleDTO eKGSample)
        {
            // Convert to signed bytes
            sbyte[] bytes = eKGSample.rawBytes;
            //sbyte[] SignedBytes = Array.ConvertAll(bytes, x => unchecked((sbyte)x));

            // Decode bytes
            ECGBatchData ecgdata = DecodingBytes.DecodingByteArray.DecodeBytes(bytes);
            ecgdata.PatientID = eKGSample.patientId;
            ecgdata.TimeStamp = eKGSample.Timestamp;

            // Buffer data for 3 minutes
            BufferData(ecgdata);

            // Send data every minute
            if (ecgdata.Samples % 256 == 0)
            {
                mqttService.Publish_RawData(Topics.Topic_Series_Raw, JsonSerializer.SerializeToUtf8Bytes(bufferedECG));
            }
        }

        private void BufferData(ECGBatchData ecgData)
        {
            int nCurrentSamples = bufferedECG.Samples;
            //TODO: dynamic length of data

            // Add new batch to buffer
            bufferedECG.ECGChannel1.Add(ecgData.ECGChannel1);
            bufferedECG.ECGChannel2.Add(ecgData.ECGChannel2);
            bufferedECG.ECGChannel3.Add(ecgData.ECGChannel3);
            bufferedECG.TimeStamp.Add(ecgData.TimeStamp);
            bufferedECG.Samples += 12;
            bufferedECG.PatientID = ecgData.PatientID;

            //When buffer is full, remove first batch
            if (nCurrentSamples >= nBufferSamples)
            {
                // Remove first batch from buffer (FIFO)
                bufferedECG.ECGChannel1.RemoveAt(0);
                bufferedECG.ECGChannel2.RemoveAt(0);
                bufferedECG.ECGChannel3.RemoveAt(0);
                bufferedECG.TimeStamp.RemoveAt(0);
                bufferedECG.Samples -= 12;
            }
        }
    }
}

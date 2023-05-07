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
        public RawDataService(IPythonMQTTService MQTTManager)
        {
            mqttService = MQTTManager;
        }
        public void ProcessData(EKGSampleDTO eKGSample)
        {

            var bytes = eKGSample.rawBytes;
            // TODO: Decodeing kan være her
            // Decoded signal
            sbyte[] signedBytes = (sbyte[])(Array)bytes;
            sbyte[] SignedBytes = Array.ConvertAll(bytes, x => unchecked((sbyte)x));


            ECGBatchData ecgdata = DecodingBytes.DecodingByteArray.DecodeBytes(SignedBytes);
            ecgdata.PatientID = eKGSample.patientId;
            ecgdata.TimeStamp = eKGSample.Timestamp;
            BufferData(ecgdata);

            if (ecgdata.Samples % 256 == 0)
            {
                mqttService.Publish_RawData(Topics.Topic_Series_Raw, JsonSerializer.SerializeToUtf8Bytes(bufferedECG));
            }
        }


        ECGBatchSeriesData bufferedECG = new ECGBatchSeriesData()
        {
            ECGChannel1 = new List<int[]>(),
            ECGChannel2 = new List<int[]>(),
            ECGChannel3 = new List<int[]>(),
            TimeStamp = new List<DateTimeOffset>(),
            Samples = 0
        };

        int nBufferSamples = 256 * 60 * 3; //45000 samples, 3750 batches

        private void BufferData(ECGBatchData ecgData)
        {
            int nCurrentSamples = bufferedECG.Samples;
            //TODO: dynamic length of data
            // Buffer 3 minutes of data
            // Send every minute

            // Fill the data model with all batches from buffer
            bufferedECG.ECGChannel1.Add(ecgData.ECGChannel1);
            bufferedECG.ECGChannel2.Add(ecgData.ECGChannel2);
            bufferedECG.ECGChannel3.Add(ecgData.ECGChannel3);
            bufferedECG.TimeStamp.Add(ecgData.TimeStamp);
            bufferedECG.Samples += 12;
            bufferedECG.PatientID = ecgData.PatientID;

            //When buffer is full, start removing first in batch and add new batch to end of buffer
            if (nCurrentSamples >= nBufferSamples)
            {
                // Removing first in from buffer (FIFO)
                bufferedECG.ECGChannel1.RemoveAt(0);
                bufferedECG.ECGChannel2.RemoveAt(0);
                bufferedECG.ECGChannel3.RemoveAt(0);
                bufferedECG.TimeStamp.RemoveAt(0);
                bufferedECG.Samples -= 12;
            }
        }
    }
}

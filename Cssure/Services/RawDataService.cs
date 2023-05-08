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
            TimeStamp = new List<long>(),
            Samples = 0,

        };

        public RawDataService(IPythonMQTTService MQTTManager)
        {
            mqttService = MQTTManager;
        }

        public void ProcessData(EKGSampleDTO eKGSample)
        {
            // Decode bytes
            ECGBatchData ecgdata = DecodingBytes.DecodingByteArray.DecodeBytes(eKGSample.RawBytes);
            ecgdata.PatientID = eKGSample.PatientId;
            ecgdata.TimeStamp = eKGSample.Timestamp;

            // Buffer data for 3 minutes
            BufferData(ecgdata);

            if (bufferedECG.Samples % 252 == 0)
            {
                ECGBatchSeriesDataDTO dataDTO = new ECGBatchSeriesDataDTO()
                {
                    ECGChannel1 = bufferedECG.ECGChannel1.ToArray(),
                    ECGChannel2 = bufferedECG.ECGChannel2.ToArray(),
                    ECGChannel3 = bufferedECG.ECGChannel3.ToArray(),
                    TimeStamp = bufferedECG.TimeStamp.ToArray(),
                    Samples = bufferedECG.Samples,
                    PatientID = bufferedECG.PatientID,
                    CSINormMax = new float[] { 15.35f, 15.49f, 17.31f },
                    ModCSINormMax = new float[] { 9074, 8485, 8719 }
                };

                var serialData = JsonSerializer.Serialize<ECGBatchSeriesDataDTO>(dataDTO);
                var bytess = System.Text.Encoding.UTF8.GetBytes(serialData);
                mqttService.Publish_RawData(Topics.Topic_Series_Raw, bytess);
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
            bufferedECG.TimeStamp.Add(ecgData.TimeStamp.ToUnixTimeMilliseconds());
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

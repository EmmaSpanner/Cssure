using Cssure.Constants;
using Cssure.DTO;
using Cssure.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Cssure.Services
{
    public class RawDataService : IRawDataService
    {

        private readonly IMQTTService mqttService;

        // Buffering
        private int nBufferSamples = 252 * 60 * 3; //45000 samples, 3750 batches
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
            bufferedECG.ECGChannel1.Add(new int[] { 
                ecgData.ECGChannel1[0], 
                ecgData.ECGChannel1[1], 
                ecgData.ECGChannel1[2], 
                ecgData.ECGChannel1[3], 
                ecgData.ECGChannel1[4], 
                ecgData.ECGChannel1[5], 
                ecgData.ECGChannel1[6], 
                ecgData.ECGChannel1[7], 
                ecgData.ECGChannel1[8], 
                ecgData.ECGChannel1[9], 
                ecgData.ECGChannel1[10], 
                ecgData.ECGChannel1[11] });
            bufferedECG.ECGChannel2.Add(new int[] {
                ecgData.ECGChannel2[0],
                ecgData.ECGChannel2[1],
                ecgData.ECGChannel2[2],
                ecgData.ECGChannel2[3],
                ecgData.ECGChannel2[4],
                ecgData.ECGChannel2[5],
                ecgData.ECGChannel2[6],
                ecgData.ECGChannel2[7],
                ecgData.ECGChannel2[8],
                ecgData.ECGChannel2[9],
                ecgData.ECGChannel2[10],
                ecgData.ECGChannel2[11] });
            bufferedECG.ECGChannel3.Add(new int[] {
                ecgData.ECGChannel3[0],
                ecgData.ECGChannel3[1],
                ecgData.ECGChannel3[2],
                ecgData.ECGChannel3[3],
                ecgData.ECGChannel3[4],
                ecgData.ECGChannel3[5],
                ecgData.ECGChannel3[6],
                ecgData.ECGChannel3[7],
                ecgData.ECGChannel3[8],
                ecgData.ECGChannel3[9],
                ecgData.ECGChannel3[10],
                ecgData.ECGChannel3[11] });
            //bufferedECG.ECGChannel1.Add(ecgData.ECGChannel1);
            //bufferedECG.ECGChannel2.Add(ecgData.ECGChannel2);
            //bufferedECG.ECGChannel3.Add(ecgData.ECGChannel3);
            long time = ecgData.TimeStamp.ToUnixTimeMilliseconds();
            bufferedECG.TimeStamp.Add(time);
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

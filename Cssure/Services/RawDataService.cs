using Cssure.Constants;
using Cssure.DTO;
using Cssure.Models;
using Cssure.MongoDB.Services;
using MongoDB.Driver.Core.WireProtocol.Messages;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Cssure.Services
{
    public class RawDataService : IRawDataService
    {

        private readonly IMQTTService mqttService;
        public UserList UserList { get; }
        private readonly DecodedECGDataService decodedService;
        private readonly RawECGDataService rawService;


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

        public RawDataService(IPythonMQTTService MQTTManager, DecodedECGDataService decodedService, RawECGDataService rawService, UserList userList)
        //Todo: Db interaktion
        //public RawDataService(IPythonMQTTService MQTTManager, UserList userList)
        {
            mqttService = MQTTManager;
            UserList = userList;
            //Todo: Db interaktion
            this.decodedService = decodedService;
            this.rawService = rawService;
        }

        public async void ProcessData(EKGSampleDTO eKGSample)
        {
            //Save raw data in database
            //Todo: Db interaktion POST RAW
            //await rawService.postRaw(eKGSample);


            // Decode bytes

            try
            {
                var t1 = DateTime.Now.ToUniversalTime();
                // Decode bytes
                ECGBatchData ecgdata = DecodingBytes.DecodingByteArray.DecodeBytes(eKGSample.RawBytes);
                ecgdata.PatientID = eKGSample.PatientId;
                ecgdata.TimeStamp = eKGSample.Timestamp;

                //Save decoded data in database
                //Todo: Db interaktion POST ECG
                //await decodedService.postDecoded(ecgdata);

                // Buffer data for 3 minutes
                BufferData(ecgdata);

                if (bufferedECG.Samples % 252 == 0)
                {
                    float[] CSINormMaxtemp;
                    float[] ModCSINormMaxtemp;
                    if (UserList.Users.ContainsKey(ecgdata.PatientID))
                    {
                        IUserMetadata temp = UserList.Users[ecgdata.PatientID];
                        CSINormMaxtemp = temp.GetMaxNormalCsi();
                        ModCSINormMaxtemp = temp.GetMaxNormalModCsi();
                    }
                    else
                    {
                        CSINormMaxtemp = new float[] { 15.35f, 15.49f, 17.31f };
                        ModCSINormMaxtemp = new float[] { 9074, 8485, 8719 };

                    }
                    ECGBatchSeriesDataDTO dataDTO = new ECGBatchSeriesDataDTO()
                    {
                        ECGChannel1 = bufferedECG.ECGChannel1.ToArray(),
                        ECGChannel2 = bufferedECG.ECGChannel2.ToArray(),
                        ECGChannel3 = bufferedECG.ECGChannel3.ToArray(),
                        TimeStamp = bufferedECG.TimeStamp.ToArray(),
                        Samples = bufferedECG.Samples,
                        PatientID = bufferedECG.PatientID,
                        CSINormMax = CSINormMaxtemp,
                        ModCSINormMax = ModCSINormMaxtemp
                    };


                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var serialData = JsonSerializer.Serialize<ECGBatchSeriesDataDTO>(dataDTO, options);
                    var bytess = System.Text.Encoding.UTF8.GetBytes(serialData);
                    var t2 = DateTime.Now.ToUniversalTime();
                    var timedifferent = t2 - t1;
                    Debug.WriteLine(timedifferent);
                    mqttService.Publish_RawData(Topics.Topic_Series_Raw, bytess);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("ProcessData(EKGSampleDTO eKGSample)");
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
                bufferedECG.ECGChannel1.RemoveRange(0,21);
                bufferedECG.ECGChannel2.RemoveRange(0, 21);
                bufferedECG.ECGChannel3.RemoveRange(0, 21);
                bufferedECG.TimeStamp.RemoveRange(0, 21);
                bufferedECG.Samples -= 12*21;
            }


        }
    }
}

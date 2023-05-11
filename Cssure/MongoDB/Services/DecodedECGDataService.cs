using Cssure.DTO;
using Cssure.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;

namespace Cssure.MongoDB.Services
{
    public class DecodedECGDataService
    {

        private readonly IMongoCollection<ECGBatchData> decoded_collection;
        public DecodedECGDataService(MongoService service)
        {
            decoded_collection = service.Client.GetDatabase("EcgDataDb").GetCollection<ECGBatchData>("DecodedEcgData");
        }

        public async Task<ECGBatchData> postDecoded(ECGBatchData decoded)
        {
            //var ECGBatchData1 = new ECGBatchData() { PatientID = "2", SeriesLength_s = 3.1f };
            try
            {
                await decoded_collection.InsertOneAsync(decoded);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in decoded: " + ex);
            }
            return decoded;

        }

        public async Task<List<ECGBatchData>> getDecoded()
        {
            return await decoded_collection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task<List<ECGBatchData>> getAllAsync()
        {
            return await decoded_collection.Find(s => true).ToListAsync();
        }
    }
}

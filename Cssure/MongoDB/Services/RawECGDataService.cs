using Cssure.DTO;
using Cssure.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;

namespace Cssure.MongoDB.Services
{
    public class RawECGDataService
    {


        private readonly IMongoCollection<EKGSampleDTO> raw_collection;
        public RawECGDataService(MongoService service)
        {
            raw_collection = service.Client.GetDatabase("EcgDataDb").GetCollection<EKGSampleDTO>("RawEcgData");
        }

        public async Task<EKGSampleDTO> postRaw(EKGSampleDTO raw)
        {
            try
            {
                await raw_collection.InsertOneAsync(raw);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in raw: " + ex);
            }
            return raw;

        }

        public async Task<List<EKGSampleDTO>> getRaw()
        {
            return await raw_collection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task<List<EKGSampleDTO>> getAllAsync()
        {
            return await raw_collection.Find(s => true).ToListAsync();
        }
    }
}

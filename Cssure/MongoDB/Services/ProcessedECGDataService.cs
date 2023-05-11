using Cssure.DTO;
using MongoDB.Driver;
using System.Diagnostics;
using MongoDB.Bson;

namespace Cssure.MongoDB.Services
{
    public class ProcessedECGDataService
    {
        private readonly IMongoCollection<CSI_DTO> csi_collection;
        public ProcessedECGDataService(MongoService service)
        {
            csi_collection = service.Client.GetDatabase("EcgDataDb").GetCollection<CSI_DTO>("ProcessedECGData");
        }

        public async Task<CSI_DTO> postCSI(CSI_DTO csi_dto) 
        {
            //var csi_dto1 = new CSI_DTO() { PatientID = "2", SeriesLength_s = 3.1f };
            try
            {
                await csi_collection.InsertOneAsync(csi_dto);
               
            } catch (Exception ex)
            {
                Debug.WriteLine("Exception in processed: " + ex);
            }
            return csi_dto;

        }

        public async Task<List<CSI_DTO>> getCSI()
        {
            return await csi_collection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task<List<CSI_DTO>> getAllAsync()
        {
            return await csi_collection.Find(s => true).ToListAsync();
        }
    }
} 

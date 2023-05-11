//using Cssure.DTO;
//using MongoDB.Bson;
//using MongoDB.Driver;
//using System.Diagnostics;

//namespace Cssure.MongoDB
//{
//    public class MongoDatabase
//    {
//        private MongoClient client;
//        public MongoDatabase()
//        {
//            //const string connectionUri = "mongodb+srv://Temo:<password>@cluster0.afmwg1g.mongodb.net/?retryWrites=true&w=majority";
//            //const string connectionUri = "mongodb://atlas-sql-645b8dbe7f6c901f38570f63-h8xvf.a.query.mongodb.net/EcgDataDb?ssl=true&authSource=admin";
//            var settings = MongoClientSettings.FromConnectionString(connectionUri);
//            // Set the ServerApi field of the settings object to Stable API version 1
//            settings.ServerApi = new ServerApi(ServerApiVersion.V1);

//            // Create a new client and connect to the server
//            client = new MongoClient(settings);
//        }

//        public void TestConnection()
//        {
//            // Send a ping to confirm a successful connection
//            try
//            {
//                var result = client.GetDatabase("EcgDataDb").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
//                Debug.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine(ex);
//            }
//        }

//        public void saveProcessedData(CSI_DTO csi_dto)
//        {
//            var csi = new CSI_DTO() { PatientID = "1", SeriesLength_s = 2.1f };
//            var result = client.GetDatabase("admin").GetCollection<CSI_DTO>("ProcessedECGData");
//            result.InsertOne(csi_dto);
//        }



//    }
//}

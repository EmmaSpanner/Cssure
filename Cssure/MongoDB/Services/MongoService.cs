using MongoDB.Driver;

namespace Cssure.MongoDB.Services
{

    public interface IMongoService
    {

    }
    public class MongoService
    {
        private readonly MongoClient _client;
        
        public MongoService()
        {
            //_client = new MongoClient("mongodb+srv://Temo:<Temo123>@cluster0.afmwg1g.mongodb.net/?retryWrites=true&w=majority");
            _client = new MongoClient("mongodb+srv://Temo:Emmaspanner@cluster0.afmwg1g.mongodb.net/?authSource=admin");
            //_client = new MongoClient("mongodb://Temo:Temo123@localhost:1883");
            var db = _client.GetDatabase("EcgDataDb");
            
        }

        public MongoClient Client { get { return _client; } }
    }
}

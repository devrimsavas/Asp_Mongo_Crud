using MongoDB.Bson;
using MongoDB.Driver;

namespace backend
{

    //mongo service Class 

    public class MongoDbService
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public MongoDbService(IConfiguration config)
        {
            var connectionString = config.GetSection("MongoDB:ConnectionString").Value;
            var databaseName = config.GetSection("MongoDB:DatabaseName").Value;
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<BsonDocument>("cars");

        }
        //show all cars
        public List<CarRecord> GetaAllRecords()
        {
            var documents = _collection.Find(new BsonDocument()).ToList();
            var records = new List<CarRecord>();

            foreach (var doc in documents)
            {
                records.Add(new CarRecord
                {
                    Id = doc["_id"]?.ToString() ?? "",
                    type = doc["type"]?.ToString() ?? "unknown",
                    HP = doc["HP"]?.ToInt32() ?? 0,
                    HPl100 = doc["HPl100"].ToDouble()

                });
            }
            return records;

        }
        //ADD record

        public async Task AddRecord(CarRecord record)
        {
            var document = new BsonDocument{
            {"type",record.type},
            {"HP",record.HP},
            {"HPl100",record.HPl100}

        };
            await _collection.InsertOneAsync(document); //mongosh shell :   db.cars.insertOne 
        }

        public async Task DeleteRecord(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
            //delete matching document 
            var result = await _collection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                throw new Exception($"No Reconds found with id: {id}");
            }
        }

        public async Task UpdateRecord(string id, CarRecord updatedRecord)

        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
            var update = Builders<BsonDocument>.Update
            .Set("type", updatedRecord.type)
            .Set("HP", updatedRecord.HP)
            .Set("HPl100", updatedRecord.HPl100);

            var result = await _collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                throw new Exception($"No record found with id: {id}");
            }

        }

        //HP filter 
        public List<CarRecord> GetCarsWithHighHP(int minHP)
        {
            var filter = Builders<BsonDocument>.Filter.Gt("HP", minHP); //mongosh db.cars.find({HP: {$gt:900}}) for example
            var documents = _collection.Find(filter).ToList();
            var records = new List<CarRecord>();
            foreach (var doc in documents)
            {
                records.Add(new CarRecord

                {
                    Id = doc["_id"]?.ToString() ?? "",
                    type = doc["type"]?.ToString() ?? "unknown",
                    HP = doc["HP"].ToInt32(),
                    HPl100 = doc["HPl100"].ToDouble()
                });
            }

            return records;
        }
        //get cars by Car Name 
        public List<CarRecord> GetCarsByName(string carName)
        {
            var filter = Builders<BsonDocument>.Filter.Regex("type", new BsonRegularExpression(carName, "i"));  //regex search 
            var documents = _collection.Find(filter).ToList();
            var records = new List<CarRecord>(); //create an empty list of records now fill it 
            foreach (var doc in documents)
            {
                records.Add(new CarRecord
                {
                    Id = doc["_id"]?.ToString() ?? "",
                    type = doc["type"]?.ToString() ?? "unknown",
                    HP = doc["HP"].ToInt32(),
                    HPl100 = doc["HPl100"].ToDouble()

                });
            }
            return records;

        }

        public long GetDocumentCount()
        {
            return _collection.CountDocuments(new BsonDocument());
        }

        public List<int> GetDistinct(string distinctField)

        {

            var values = _collection.Distinct<int>(distinctField, new BsonDocument()).ToList();
            return values;
        }
    }



}
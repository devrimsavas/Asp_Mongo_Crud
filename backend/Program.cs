//add packages 
using System.Net.Http.Headers;
using Amazon.Runtime.Internal;
using DnsClient.Protocol;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;




var builder = WebApplication.CreateBuilder(args);


//add MongoDB service
builder.Services.AddSingleton<MongoDbService>();

//add cors 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

//use cors
app.UseCors("AllowAll");


app.MapGet("/", () =>
{
    string test = "test string";
    return Results.Json(new { message = "Home Page for School Mongo Chapter", pageversion = "1.0", status = "OK", debug = test });
});

//show all records 
app.MapGet("/showrecords", (MongoDbService mongoDbService) =>
{
    // get all records

    var records = mongoDbService.GetaAllRecords();
    return Results.Ok(records);

});

//get count of document 
app.MapGet("/countdocument", (MongoDbService mongoDbService) =>
{
    var countDocument = mongoDbService.GetDocumentCount();
    var distinctHP = mongoDbService.GetDistinct("HP");

    return Results.Ok(new { documentCount = countDocument, distinctHP = distinctHP });

});

//add new record 
app.MapPost("/addrecord", async (MongoDbService mongoDbService, CarRecord newRecord) =>
{
    await mongoDbService.AddRecord(newRecord);
    return Results.Ok(new { message = "Record added", record = newRecord });
});


//delete record
app.MapDelete("/deleterecord/{id}", async (MongoDbService mongoDbService, string id) =>
{

    try
    {
        await mongoDbService.DeleteRecord(id);
        return Results.Ok(new { message = "Record Deleted successfully", id = id });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }


});

app.MapPut("/updaterecord/{id}", async (MongoDbService mongoDbService, string id, CarRecord updatedCar) =>
{
    try
    {
        await mongoDbService.UpdateRecord(id, updatedCar);
        return Results.Ok(new { message = "Record updated successfully", id = id });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

//HP> 900 filter 
app.MapGet("/highhp/{minHP}", (MongoDbService mongoDbService, int minHP) =>
{
    var cars = mongoDbService.GetCarsWithHighHP(minHP);
    return Results.Ok(cars);
});

//Name filter 

app.MapGet("/getbytype/{type}", (MongoDbService mongoDbService, string type) =>
{
    var cars = mongoDbService.GetCarsByName(type);
    return Results.Ok(cars);
});




app.Run();
//end of routes 


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





//Car Record it is properties 

public class CarRecord
{

    public string? Id { get; set; }

    public string? type { get; set; }
    public int HP { get; set; }
    public double HPl100 { get; set; }

}

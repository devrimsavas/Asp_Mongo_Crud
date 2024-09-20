//add packages 
/*
using System.Net.Http.Headers;
using Amazon.Runtime.Internal;
using DnsClient.Protocol;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
*/
using backend;




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

    return Results.Ok(new { documentCount = countDocument, HP = distinctHP });

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









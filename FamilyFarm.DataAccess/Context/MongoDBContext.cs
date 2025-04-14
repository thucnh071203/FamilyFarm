using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.Context
{
    public class MongoDBContext
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase? _database;
        public MongoDBContext(IConfiguration configuration)
        {
            _configuration = configuration;
            var connectionString = _configuration["MongoDbSettings:ConnectionString"];
            var mongoUrl = MongoUrl.Create(configuration["MongoDbSettings:ConnectionString"]);

            var mongoClient = new MongoClient(mongoUrl);
            _database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
        }

        public IMongoDatabase? Database => _database;
    }
}

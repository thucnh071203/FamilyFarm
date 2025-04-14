using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class SingletonBase
    {
        private static IMongoDatabase? _database;
        private static readonly object LockObject = new object();

        public static IMongoDatabase Database
        {
            get
            {
                if (_database == null)
                {
                    lock (LockObject)
                    {
                        if (_database == null) // Double-check locking
                        {
                            var configuration = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .Build();

                            var connectionString = configuration["MongoDbSettings:ConnectionString"];
                            var mongoUrl = MongoUrl.Create(connectionString);

                            var mongoClient = new MongoClient(mongoUrl);
                            _database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
                        }
                    }
                }
                return _database!;
            }
        }
    }
}

using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Ratings
{
    internal class CosmosHelper
    {
       static IConfiguration config;
        static CosmosHelper() 
        {
            config = new ConfigurationBuilder()
                  .AddEnvironmentVariables()
                  .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                  .Build();
        }
        private static CosmosClient _cosmosClient;
        // The database we will create
        private static Database _database;
        // The container we will create.
        private static Container _container;
        private static string _cosmosKey;

        
        private static CosmosClient CosmosClient 
        { 
            get
            {
                if (_cosmosClient == null)
                {
                    _cosmosClient = new CosmosClient("https://t3-cosmos-db-account.documents.azure.com:443/", CosmosKey, new CosmosClientOptions());
                }
                return _cosmosClient;
            }
            set => _cosmosClient = value; }
        private static Database Database
        {
            get
            {
                if (_database == null)
                {
                    _database = CosmosClient.GetDatabase("icratinghk3db");
                }
                return _database;
            }
            set => _database = value; }
        public static Container Container
        {
            get
            {
                if (_container == null)
                {
                    _container = Database.GetContainer("icratingcontainer");
                }
                return _container;
            }
            set => _container = value; }

        private static string CosmosKey
        {
            get
            {
                if (_cosmosKey == null)
                {
                    _cosmosKey = config["cosmoskey"];
                }
                return _cosmosKey;
            }
            set => _cosmosKey = value; }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using st = System.Text.Json;

namespace Ratings
{
    public class InitialPayload
    {
        public InitialPayload()
        {

        }

        public string id { get; set; }  // must be lowercase
        public string userId { get; set; }
        public string productId { get; set; }
        public DateTime timestamp { get; set; }
        public string locationName { get; set; }
        public int rating { get; set; }
        public string userNotes { get; set; }
    }
    public class Product
    {
        public Product()
        {

        }
        public int productId { get; set; }
        public string productName { get; set; }
        public string productDescription { get; set; }


    }
    public static class CreateRating
    {
        const string getProductURL = "https://serverlessohapi.azurewebsites.net/api/GetProduct?productId=";
        const string getUserURL = "https://serverlessohapi.azurewebsites.net/api/GetUser?userId=";

        [FunctionName("CreateRating")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = null)] HttpRequest req,
            ILogger log,
            [CosmosDB(
            databaseName:"icratinghk3db",
            collectionName:"icratingcontainer",
            ConnectionStringSetting ="cosmosconnection")] out InitialPayload payload)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            //string name = req.Query["name"];

            string requestBody =  new StreamReader(req.Body).ReadToEndAsync().GetAwaiter().GetResult();
            payload = JsonConvert.DeserializeObject<InitialPayload>(requestBody);
            //InitialPayload payload = new InitialPayload();

            //payload.userId = "cc20a6fb-a91f-4192-874d-132493685376";
            //payload.productId = "4c25613a-a3c2-4ef3-8e02-9c335eb23204";
            //payload.locationName = "Sample ice cream shop";
            //payload.rating = 5;
            //payload.userNotes = "I love the subtle notes of orange in this ice cream!";


            string getProductPath = "";
            string getUserPath = "";

            if (payload.productId != null)
            {
                getProductPath = string.Concat(getProductURL, payload.productId.ToString());
            }

            bool productStatus = ValidateGetProductAsync(getProductPath).GetAwaiter().GetResult();

            if (payload.userId != null)
            {
                getUserPath = string.Concat(getUserURL, payload.userId.ToString());
            }
            bool userStatus = ValidateGetUserAsync(getUserPath).GetAwaiter().GetResult();

            payload.id = Guid.NewGuid().ToString();
            payload.timestamp = DateTime.UtcNow;




            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

            string responseMessage = JsonConvert.SerializeObject(payload);

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("GetRating")]
        public static async Task<IActionResult> getRating(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
    ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a get request.");

            string ratingId = req.Query["ratingId"];

            var returnValue = await CosmosHelper.Container.ReadItemAsync<InitialPayload>(ratingId, new Microsoft.Azure.Cosmos.PartitionKey(ratingId));

            var serializedValue = st.JsonSerializer.Serialize(returnValue.Resource);

            return new OkObjectResult(serializedValue);
        }


        //https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client
        //
        static async Task<bool> ValidateGetProductAsync(string path )
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                //product = await response.Content.ReadAsAsync<Product>();
                return true;
            }
            return false;
        }

        static async Task<bool> ValidateGetUserAsync(string path)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Polly;
using RestSharp;
// using static RestSharp.RestRequest;

namespace ApiRetry.Controllers;

[ApiController]
[Route("[controller]")]
public class ServiceRequestsController : ControllerBase
{
    private readonly ILogger<ServiceRequestsController> _logger;

    public ServiceRequestsController(ILogger<ServiceRequestsController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetData")]
    public async Task<IActionResult> Get()
    {
        // var retryPolicy = Policy
        //     .Handle<Exception>()
        //     .RetryAsync(5, onRetry: (exception, retryCount) => {
        //         Console.WriteLine("Error: " +exception.Message+"...Retry Count" + retryCount);
        //     });

        // await retryPolicy.ExecuteAsync(async () => {
        //    await ConnectToApi(); 
        // });

        var amountToPause = TimeSpan.FromSeconds(15);

        // var retryWaitPolicy = Policy
        //     .Handle<Exception>()
        //     .WaitAndRetryAsync(5, i => amountToPause , onRetry: (exception, retryCount) => {
        //             Console.WriteLine("Error: " + exception.Message + ".. Retry Count :"+ retryCount);
        //     });

        // await retryWaitPolicy.ExecuteAsync(async () => {
        //     await ConnectToApi();
        // });

        var retryPolicy = Policy
            .Handle<Exception>()
                .WaitAndRetry(5, i => amountToPause, (exception, retryCount) => {
                    Console.WriteLine("Error: " + exception.Message + ".. Retry Count : " + retryCount);
                });
        
        var circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreaker(3, TimeSpan.FromSeconds(30));

        var finalPolicy = retryPolicy.Wrap(circuitBreakerPolicy);

        finalPolicy.Execute( () =>  {
            Console.WriteLine("Executing");
            ConnectToApiSync();
        });

        // await ConnectToApi();
        return Ok();
    }

    private void ConnectToApiSync()
    {
        var url = "https://matchilling-chuck-norris-jokes-v1.p.rapidapi.com/jokes/random";

        var client = new RestClient();

        var request = new RestRequest(url, Method.Get);

        request.AddHeader("accept", "application/json");
        request.AddHeader("X-RapidAPI-Key", "fa8946b5e6msh517be52504e13e6p189bd2jsn06062a93600d");
        request.AddHeader("X-RapidAPI-Host", "matchilling-chuck-norris-jokes-v1.p.rapidapi.com");

        var response = client.Execute(request);

        // Console.WriteLine("Error:" + response.ErrorMessage);
        // Console.WriteLine("Error Status: " + response.IsSuccessful );

        if(response.IsSuccessful)
        {
            Console.WriteLine(response.Content);
        }
        else
        {
            Console.WriteLine(response.ErrorMessage);
            throw new Exception("Not able to connect to the service");
        }

    }

    private async Task ConnectToApi()
    {
        var url = "https://matchilling-chuck-norris-jokes-v1.p.rapidapi.com/jokes/random";

        var client = new RestClient();

        var request = new RestRequest(url, Method.Get);

        request.AddHeader("accept", "application/json");
        request.AddHeader("X-RapidAPI-Key", "fa8946b5e6msh517be52504e13e6p189bd2jsn06062a93600d");
        request.AddHeader("X-RapidAPI-Host", "matchilling-chuck-norris-jokes-v1.p.rapidapi.com");

        var response = await client.ExecuteAsync(request);

        // Console.WriteLine("Error:" + response.ErrorMessage);
        // Console.WriteLine("Error Status: " + response.IsSuccessful );

        if (response.IsSuccessful)
        {
            Console.WriteLine(response.Content);
        }
        else
        {
            Console.WriteLine(response.ErrorMessage);
            throw new Exception("Not able to connect to the service");
        }

    }


    // private object RestRequest(string url, Method get)
    // {
    //     throw new NotImplementedException();
    // }
}
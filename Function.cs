using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    /// <summary>
    /// Lambda function handler to process GitHub webhook events and send notifications to Slack.
    /// </summary>
    /// <param name="input">The event data from the GitHub webhook.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns>The response from the Slack API call.</returns>
    public string FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogInformation($"FunctionHandler received: {input}");

        try
        {
            dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());
            string payload = $"{{\"text\":\"Issue Created: {json.issue.html_url}\"}}";
        
            var client = new HttpClient();
            var webRequest = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
    
            var response = client.Send(webRequest);
            using var reader = new StreamReader(response.Content.ReadAsStream());
            
            return reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error processing webhook: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }
}
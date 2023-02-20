using Amazon.SQS;
using Amazon.SQS.Model;
using SQSPublisherExample;
using System.Text.Json;

string sqsQueueName = "users";
var sqsClient = new AmazonSQSClient();
var queueUrlResponse = await sqsClient.GetQueueUrlAsync(sqsQueueName);

Guid newUserId = Guid.NewGuid();
UserCreatedMessage userCreatedMessage = new(newUserId, "juanmg", "Juan M.", "juanmg@domain.com");
var userCreatedMessageRequest = new SendMessageRequest
{
    QueueUrl = queueUrlResponse.QueueUrl,
    MessageBody = JsonSerializer.Serialize(userCreatedMessage),
    MessageAttributes = new()
    {
        {
            "MessageType",
            new() { DataType = "String", StringValue = typeof(UserCreatedMessage).Name }
        }
    }
};

Console.WriteLine("Sending UserCreatedMessage...");

var response = await sqsClient.SendMessageAsync(userCreatedMessageRequest);

Console.WriteLine($"UserCreatedMessage sent. Response StatusCode: {response.HttpStatusCode}");

UserDeletedMessage userDeletedMessage = new(newUserId);
var userDeletedMessageRequest = new SendMessageRequest
{
    QueueUrl = queueUrlResponse.QueueUrl,
    MessageBody = JsonSerializer.Serialize(userDeletedMessage),
    MessageAttributes = new()
    {
        {
            "MessageType",
            new() { DataType = "String", StringValue = typeof(UserDeletedMessage).Name }
        }
    }
};

Console.WriteLine("Sending UserCreatedMessage...");

response = await sqsClient.SendMessageAsync(userDeletedMessageRequest);

Console.WriteLine($"UserCreatedMessage sent. Response StatusCode: {response.HttpStatusCode}");

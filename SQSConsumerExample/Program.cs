using Amazon.SQS;
using Amazon.SQS.Model;

string queueName = "users";
var sqsClient = new AmazonSQSClient();
var cancellationTokenSource = new CancellationTokenSource();

var sqsQueueUrlResponse = await sqsClient.GetQueueUrlAsync(queueName);

var usersReceiveMessageRequest = new ReceiveMessageRequest()
{
    QueueUrl = sqsQueueUrlResponse.QueueUrl,
    MessageAttributeNames = new() { "All" },
    AttributeNames = new() { "SenderId", "ApproximateReceiveCount", "SentTimestamp", "ApproximateFirstReceiveTimestamp" },
    MaxNumberOfMessages = 2
};

while (!cancellationTokenSource.IsCancellationRequested)
{
    var receivedMessage = await sqsClient.ReceiveMessageAsync(usersReceiveMessageRequest, cancellationTokenSource.Token);
    Console.WriteLine($"Received {receivedMessage.Messages.Count} messages. Status Code of operation = {receivedMessage.HttpStatusCode}");

    foreach (var message in receivedMessage.Messages)
    {
        Console.WriteLine($"Message ID: {message.MessageId}");
        Console.WriteLine($"Message Body: {message.Body}");
        Console.WriteLine($"Message Type: {(message.MessageAttributes.TryGetValue("MessageType", out var messageType) ? messageType.StringValue : "Unknown")}");
        Console.WriteLine($"\tSender ID: {(message.Attributes.TryGetValue("SenderId", out var senderId) ? senderId : "Unknown")}");
        Console.WriteLine($"\tReceive Count: {(message.Attributes.TryGetValue("ApproximateReceiveCount", out var receiveCount) ? receiveCount : "Unknown")}");
        Console.WriteLine($"\tSent Timestamp: {(message.Attributes.TryGetValue("SentTimestamp", out var sentTimestamp) ? sentTimestamp : "Unknown")}");
        Console.WriteLine($"\tFirst Receive Timestamp: {(message.Attributes.TryGetValue("ApproximateFirstReceiveTimestamp", out var firstReceiveTimestamp) ? firstReceiveTimestamp : "Unknown")}");
        Console.WriteLine();

        var deletedMessageResponse = await sqsClient.DeleteMessageAsync(sqsQueueUrlResponse.QueueUrl, message.ReceiptHandle, cancellationTokenSource.Token);
        Console.WriteLine($"Deleted MessageId = {message.MessageId} with Status Code = {deletedMessageResponse.HttpStatusCode}");
    }

    await Task.Delay(3000);
}

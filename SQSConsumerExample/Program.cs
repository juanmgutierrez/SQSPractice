using Amazon.SQS;
using Amazon.SQS.Model;

string queueName = "users";
var sqsClient = new AmazonSQSClient();
var cancellationTokenSource = new CancellationTokenSource();

var sqsQueueUrlResponse = await sqsClient.GetQueueUrlAsync(queueName);

var usersReceiveMessageRequest = new ReceiveMessageRequest()
{
    QueueUrl = sqsQueueUrlResponse.QueueUrl,
    MessageAttributeNames = new List<string>() { "ALL" }
};

while (!cancellationTokenSource.IsCancellationRequested)
{
    var receivedMessage = await sqsClient.ReceiveMessageAsync(usersReceiveMessageRequest, cancellationTokenSource.Token);
    Console.WriteLine($"Received {receivedMessage.Messages.Count} messages. Status Code of operation = {receivedMessage.HttpStatusCode}");

    foreach (var message in receivedMessage.Messages)
    {
        var messageId = message.MessageId;
        Console.WriteLine(messageId);
        Console.WriteLine(message.Body);
        Console.WriteLine();

        var deletedMessageResponse = await sqsClient.DeleteMessageAsync(sqsQueueUrlResponse.QueueUrl, message.ReceiptHandle, cancellationTokenSource.Token);
        Console.WriteLine($"Deleted MessageId = {messageId} with Status Code = {deletedMessageResponse.HttpStatusCode}");
    }

    await Task.Delay(3000);
}

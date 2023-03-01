using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;

namespace SQSConsumerAppExample;

public class SQSConsumerService : BackgroundService
{
    private readonly IAmazonSQS _sqs;
    private readonly SQSSettings _sqsSettings;
    private readonly ILogger<SQSConsumerService> _logger;

    public SQSConsumerService(IAmazonSQS sqs, IOptions<SQSSettings> sqsSettings, ILogger<SQSConsumerService> logger)
    {
        _sqs = sqs;
        _sqsSettings = sqsSettings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int SecondsBetweenQueueReads = 3;
        var queueURLResponse = await _sqs.GetQueueUrlAsync(_sqsSettings.QueueName);
        string queueURL = queueURLResponse.QueueUrl;

        var usersReceiveMessageRequest = new ReceiveMessageRequest()
        {
            QueueUrl = queueURL,
            MessageAttributeNames = new() { "All" },
            AttributeNames = new() { "SenderId", "ApproximateReceiveCount", "SentTimestamp", "ApproximateFirstReceiveTimestamp" },
            MaxNumberOfMessages = 2
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            var receivedMessage = await _sqs.ReceiveMessageAsync(usersReceiveMessageRequest, stoppingToken);

            foreach (var message in receivedMessage.Messages)
            {

                if(!message.MessageAttributes.TryGetValue("MessageType", out var messageAttributeValue))
                {
                    _logger.LogWarning("Received message of Unknown type.");
                    continue;
                }

                _logger.LogDebug("Received message of type {MessageType}", messageAttributeValue.StringValue);

                await _sqs.DeleteMessageAsync(queueURL, message.ReceiptHandle, stoppingToken);
                _logger.LogDebug("Message deleted from queue");
            }

            _logger.LogDebug("Waiting for another 3 seconds...");
            await Task.Delay(SecondsBetweenQueueReads*1000, stoppingToken);
        }
    }
}

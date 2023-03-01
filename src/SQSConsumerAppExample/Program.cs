using Amazon.SQS;
using SQSConsumerAppExample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SQSSettings>(builder.Configuration.GetSection(SQSSettings.Key));
builder.Services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
builder.Services.AddHostedService<SQSConsumerService>();

var app = builder.Build();

app.Run();

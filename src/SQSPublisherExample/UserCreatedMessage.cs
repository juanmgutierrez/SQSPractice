namespace SQSPublisherExample;

public record UserCreatedMessage(Guid Id, string UserName, string FullName, string Email);

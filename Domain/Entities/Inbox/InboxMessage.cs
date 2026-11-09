namespace Studium_Audit.Domain.Entities.Inbox;

public class InboxMessage
{
    public Guid Id { get; private set; }
    public string HandlerName { get; private set; } = string.Empty;
    public DateTime ReceivedAtUtc { get; private set; }

    private InboxMessage() { }

    public InboxMessage(Guid id, string handlerName, DateTime receivedAtUtc)
    {
        Id = id;
        HandlerName = handlerName;
        ReceivedAtUtc = receivedAtUtc;
    }
}

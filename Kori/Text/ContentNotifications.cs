using MediatR;

namespace Sparc.Kori;

public record Notification : INotification
{
    public Notification(string? subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public string? SubscriptionId { get; set; }
}

public record MessageAudioChanged(Content Content) : Notification(Content.PageId + "|" + Content.Language);
public record MessageTextChanged(Content Content) : Notification(Content.PageId + "|" + Content.Language);
public record MessageDeleted(Content Content) : Notification(Content.PageId + "|" + Content.Language);

namespace Career.Api.Hubs.Responses
{
    public sealed record NotificationSentResponse
    {
        public NotificationSentResponse(Guid id, string senderId, string title, DateTime createdAt)
        {
            Id = id;
            SenderId = senderId;
            Title = title;
            CreatedAt = createdAt;
        }

        public Guid Id { get; set; }
        public string SenderId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    }

    public sealed record InformationNotificationSentResponse
    {
        public InformationNotificationSentResponse(Guid id, string title, DateTime createdAt)
        {
            Id = id;
            Title = title;
            CreatedAt = createdAt;
        }

        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    }
}

namespace Project2EmailNight.Entities
{
    public class Message
    {
        public int MessageId { get; set; }
        public string SenderEmail { get; set; }
        public string? SenderName { get; set; }
        public string? SenderSurname { get; set; }
        public string ReceiverEmail { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverSurname { get; set; }
        public string Subject { get; set; }
        public string? Body { get; set; }
        public DateTime SendDate { get; set; }
        public bool IsInbox { get; set; }
        public bool IsSent { get; set; }
        public bool IsDraft { get; set; }
        public bool IsStarred { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
        public string Category { get; set; } = "Birincil";

    }
}

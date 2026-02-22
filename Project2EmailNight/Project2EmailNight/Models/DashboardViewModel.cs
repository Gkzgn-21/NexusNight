
namespace Project2EmailNight.Models
{
    public class DashboardViewModel
    {
        public int InboxCount { get; set; }
        public int SentCount { get; set; }
        public int DraftCount { get; set; }
        public int TotalInboxCount { get; set; }
        public int TrashCount { get; set; }
        public int StarredCount { get; set; }

        public string[] DayLabels { get; set; } = new string[7];
        public int[] InboxLast7Days { get; set; } = new int[7];
        public int[] SentLast7Days { get; set; } = new int[7];
    }
}
namespace HealthcareCRM.API.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalPatients { get; set; }
        public int AppointmentsToday { get; set; }
        public int AppointmentsThisWeek { get; set; }
        public int PendingCount { get; set; }
        public Dictionary<string, int> StatusBreakdown { get; set; } = new();
    }
}
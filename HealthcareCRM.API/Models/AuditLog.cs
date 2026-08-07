namespace HealthcareCRM.API.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }       // admin who performed the action
        public string Action { get; set; } = "";   // e.g. "RoleChanged", "UserDeactivated"
        public int TargetId { get; set; }     // the user affected
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
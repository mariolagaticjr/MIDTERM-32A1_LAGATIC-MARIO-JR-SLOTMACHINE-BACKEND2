using System;
namespace SlotMachineApi.DTOs
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; }
        public string Action { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace SlotMachineApi.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; }
        public string Action { get; set; }
        public string Status { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
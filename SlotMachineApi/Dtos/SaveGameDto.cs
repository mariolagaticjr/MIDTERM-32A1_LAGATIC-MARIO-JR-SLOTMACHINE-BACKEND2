using System;

namespace SlotMachineApi.DTOs
{
    public class SaveGameDto
    {
        public string StudentNumber { get; set; }
        public string Result { get; set; }
        public int RetryCount { get; set; }
        public DateTime DatePlayed { get; set; }
    }
}
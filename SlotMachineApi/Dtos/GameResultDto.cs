using System;
namespace SlotMachineApi.DTOs
{
    public class GameResultDto
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; }
        public string StudentName { get; set; }
        public string Result { get; set; }
        public int RetryCount { get; set; }
        public DateTime DatePlayed { get; set; }
    }
}
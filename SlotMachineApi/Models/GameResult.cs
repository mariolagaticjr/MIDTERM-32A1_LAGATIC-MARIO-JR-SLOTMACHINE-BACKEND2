using System;
using System.ComponentModel.DataAnnotations;

namespace SlotMachineApi.Models
{
    public class GameResult
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string StudentNumber { get; set; }

        [Required]
        public string Result { get; set; } 

        public int RetryCount { get; set; }

        [Required]
        public DateTime DatePlayed { get; set; }

        
        public Player Player { get; set; }
    }
}
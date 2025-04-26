using System;
using System.ComponentModel.DataAnnotations;

namespace SlotMachineApi.Models
{
    public class Player
    {
        [Key]
        public string StudentNumber { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public DateTime RegistrationDate { get; set; }

        // Full name property for convenience
        public string FullName => $"{FirstName} {LastName}";
    }
}
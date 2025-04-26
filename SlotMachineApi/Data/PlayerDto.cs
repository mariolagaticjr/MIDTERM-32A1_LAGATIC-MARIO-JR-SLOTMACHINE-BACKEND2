using System;
namespace SlotMachineApi.DTOs
{
    public class PlayerDto
    {
        public string StudentNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}

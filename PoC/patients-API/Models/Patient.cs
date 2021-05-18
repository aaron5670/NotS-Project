using System;

namespace patients_API.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int TelephoneNumber { get; set; }
        public DateTime Birthdate { get; set; }
        public string Gender { get; set; }
        public string BloodType { get; set; }
    }
}

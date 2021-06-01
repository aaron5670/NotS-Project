using System.Collections.Generic;

namespace patients_API.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public List<Medicine> Medicines { get; set; }
        public List<Care> Care { get; set; }
        public string bloodType { get; set; }
    }
}
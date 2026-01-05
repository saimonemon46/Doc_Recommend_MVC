using System.Collections.Generic;

namespace MedicalDoctorRecommender.Models
{
    public class Doctor
    {
        public string Name { get; set; }
        public string Education { get; set; }
        public string Speciality { get; set; }
        public double Experience { get; set; }
        public string Chamber { get; set; }
        public string Location { get; set; }
        public List<string> Concentrations { get; set; } = new();
    }
}

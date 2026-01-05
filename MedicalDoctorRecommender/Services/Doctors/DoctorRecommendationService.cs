using MedicalDoctorRecommender.Models;
using MedicalDoctorRecommender.Infrastructure;

namespace MedicalDoctorRecommender.Services.Doctors
{
    public class DoctorRecommendationService
    {
        private readonly List<Doctor> _doctors;

        public DoctorRecommendationService()
        {
            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Data", "Datasets", "doctors.csv");

            _doctors = CsvDoctorLoader.Load(path);
        }

        public List<Doctor> Recommend(string symptom, string location)
        {
            return _doctors
                .Where(d =>
                    d.Location.Equals(location, StringComparison.OrdinalIgnoreCase) &&
                    (d.Speciality.Contains(symptom, StringComparison.OrdinalIgnoreCase) ||
                     d.Concentrations.Any(c =>
                        c.Contains(symptom, StringComparison.OrdinalIgnoreCase))))
                .Take(3)
                .ToList();
        }
    }
}

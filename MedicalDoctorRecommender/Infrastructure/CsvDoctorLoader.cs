using MedicalDoctorRecommender.Models;

namespace MedicalDoctorRecommender.Infrastructure
{
    public static class CsvDoctorLoader
    {
        public static List<Doctor> Load(string path)
        {
            var doctors = new List<Doctor>();
            var lines = File.ReadAllLines(path);

            // skip header row (unchanged header)
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = SplitCsv(line);

                // Expecting EXACT order from your CSV
                if (parts.Length < 7)
                    continue;

                doctors.Add(new Doctor
                {
                    Name = parts[0],          // Doctor Name
                    Education = parts[1],
                    Speciality = parts[2],
                    Experience = double.TryParse(parts[3], out var exp) ? exp : 0,
                    Chamber = parts[4],
                    Location = parts[5],
                    Concentrations = parts[6]
                        .Split(',')
                        .Select(x => x.Trim())
                        .ToList()
                });
            }

            return doctors;
        }

        // Handles quoted commas correctly
        private static string[] SplitCsv(string input)
        {
            return System.Text.RegularExpressions.Regex
                .Split(input, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)")
                .Select(s => s.Trim().Trim('"'))
                .ToArray();
        }
    }
}

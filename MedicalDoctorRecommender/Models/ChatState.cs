using System.Collections.Generic;

namespace MedicalDoctorRecommender.Models
{
    public class ChatState
    {
        public List<string> UserMessages { get; set; } = new();
        public int QuestionCount { get; set; }
        public bool GuidanceGiven { get; set; }
        public bool LocationAsked { get; set; }
        public bool Finished { get; set; }
    }
}

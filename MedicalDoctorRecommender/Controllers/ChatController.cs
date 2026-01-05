using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MedicalDoctorRecommender.Models;
using MedicalDoctorRecommender.Services;
using MedicalDoctorRecommender.Services.Doctors;
using MedicalDoctorRecommender.Helpers;
using System.Threading.Tasks;
using System.Linq;

namespace MedicalDoctorRecommender.Controllers
{
    public class ChatController : Controller
    {
        private readonly GroqLLMService _llm;
        private readonly DoctorRecommendationService _doctorService;

        public ChatController(
            GroqLLMService llm,
            DoctorRecommendationService doctorService)
        {
            _llm = llm;
            _doctorService = doctorService;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            // reset chat on page load
            HttpContext.Session.Remove("chat_state");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send(string message)
        {
            var state = HttpContext.Session.GetObject<ChatState>("chat_state")
                        ?? new ChatState();

            // store user message
            state.UserMessages.Add("User: " + message);

            // if location already asked → recommend doctors
            if (state.LocationAsked)
            {
                var doctors = _doctorService
                    .Recommend(message, message)
                    .Take(3)
                    .ToList();

                state.Finished = true;
                Save(state);

                return Json(doctors);
            }

            // ask LLM for next response
            var reply = await _llm.GetNextResponse(
                state.UserMessages,
                state.QuestionCount,
                state.GuidanceGiven);

            // update state based on reply
            if (reply.Trim().EndsWith("?"))
            {
                state.QuestionCount++;
            }
            else if (!state.GuidanceGiven)
            {
                state.GuidanceGiven = true;
            }

            if (state.GuidanceGiven &&
                reply.ToLower().Contains("location"))
            {
                state.LocationAsked = true;
            }

            state.UserMessages.Add("Bot: " + reply);
            Save(state);

            return Json(reply);
        }

        private void Save(ChatState state)
        {
            HttpContext.Session.SetObject("chat_state", state);
        }
    }
}

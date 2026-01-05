using MedicalDoctorRecommender.Models;
using MedicalDoctorRecommender.Data;
using Microsoft.AspNetCore.Mvc;
using System;

namespace MedicalDoctorRecommender.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return View();

            HttpContext.Session.SetInt32("UserId", user.Id);
            return RedirectToAction("Index", "Chat");
        }

        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
                return View();

            var user = new User
            {
                Email = email,
                PasswordHash = password // hash in real life, relax
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            return RedirectToAction("Login");
        }
    }


}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyclinicDB.Services;

namespace PolyclinicDB.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly DatabaseService _dbService;
        private readonly IAuthService _authService;

        public AdminController(DatabaseService dbService, IAuthService authService)
        {
            _dbService = dbService;
            _authService = authService;
        }

        public IActionResult Dashboard()
        {
            ViewBag.UserCount = GetUserCount();
            ViewBag.PatientCount = GetPatientCount();
            ViewBag.DoctorCount = GetDoctorCount();
            ViewBag.AppointmentCount = GetAppointmentCount();

            return View();
        }

        public async Task<IActionResult> Users()
        {
            var users = await GetAllUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> Doctors()
        {
            var doctors = await _dbService.GetDoctorsAsync();
            return View(doctors);
        }

        // Вспомогательные методы
        private int GetUserCount()
        {
            // Реализация получения количества пользователей
            return 0;
        }

        private int GetPatientCount()
        {
            // Реализация получения количества пациентов
            return 0;
        }

        private int GetDoctorCount()
        {
            // Реализация получения количества врачей
            return 0;
        }

        private int GetAppointmentCount()
        {
            // Реализация получения количества приемов
            return 0;
        }

        private async Task<IEnumerable<dynamic>> GetAllUsersAsync()
        {
            // Реализация получения всех пользователей
            return new List<dynamic>();
        }
    }
}
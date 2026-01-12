using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyclinicDB.Services;
using PolyclinicDB.ViewModels;
using System.Security.Claims;

namespace PolyclinicDB.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseService _dbService;

        public HomeController(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<IActionResult> Index()
        {
            // Перенаправляем аутентифицированных пользователей в их личные кабинеты
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Dashboard", "Admin");
                else if (User.IsInRole("Doctor"))
                    return RedirectToAction("Dashboard", "Doctor");
                else if (User.IsInRole("Patient"))
                    return RedirectToAction("Dashboard", "Patient");
            }

            // Для неаутентифицированных пользователей показываем общую страницу
            var patients = await _dbService.GetPatientsAsync() ?? new List<Models.Patient>();
            var doctors = await _dbService.GetDoctorsAsync() ?? new List<Models.Doctor>();

            ViewBag.LongestName = await _dbService.GetLongestPatientNameAsync() ?? "Нет данных";
            ViewBag.Top10Patients = await _dbService.GetTop10PatientsAsync() ?? new List<Models.Patient>();

            return View(new DashboardViewModel
            {
                Patients = patients,
                Doctors = doctors,
                Appointments = new List<Models.Appointment>()
            });
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
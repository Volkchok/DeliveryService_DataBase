using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyclinicDB.Services;
using System.Security.Claims;

namespace PolyclinicDB.Controllers
{
    [Authorize(Policy = "PatientOnly")]
    public class PatientController : Controller
    {
        private readonly DatabaseService _dbService;
        private readonly IAuthService _authService;

        public PatientController(DatabaseService dbService, IAuthService authService)
        {
            _dbService = dbService;
            _authService = authService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var patientId = GetCurrentPatientId();
            if (!patientId.HasValue)
                return RedirectToAction("AccessDenied", "Account");

            var patient = await _dbService.GetPatientByIdAsync(patientId.Value);
            ViewBag.UpcomingAppointments = await GetUpcomingAppointments(patientId.Value);

            return View(patient);
        }

        public async Task<IActionResult> MyAppointments()
        {
            var patientId = GetCurrentPatientId();
            if (!patientId.HasValue)
                return RedirectToAction("AccessDenied", "Account");

            var appointments = await GetPatientAppointments(patientId.Value);
            return View(appointments);
        }

        public async Task<IActionResult> Profile()
        {
            var patientId = GetCurrentPatientId();
            if (!patientId.HasValue)
                return RedirectToAction("AccessDenied", "Account");

            var patient = await _dbService.GetPatientByIdAsync(patientId.Value);
            return View(patient);
        }

        public async Task<IActionResult> BookAppointment()
        {
            var doctors = await _dbService.GetDoctorsAsync();
            ViewBag.Doctors = doctors;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BookAppointment(int doctorId, DateTime appointmentDate, string complaints)
        {
            var patientId = GetCurrentPatientId();
            if (!patientId.HasValue)
                return RedirectToAction("AccessDenied", "Account");

            try
            {
                var appointment = new Models.Appointment
                {
                    PatientId = patientId.Value,
                    DoctorId = doctorId,
                    AppointmentDate = appointmentDate,
                    Complaints = complaints,
                    Status = "scheduled"
                };

                await _dbService.CreateAppointmentAsync(appointment);
                TempData["Message"] = "Запись на прием успешно создана";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка: {ex.Message}";
            }

            return RedirectToAction("MyAppointments");
        }

        // Вспомогательные методы
        private int? GetCurrentPatientId()
        {
            var patientIdClaim = User.FindFirst("PatientId")?.Value;
            if (int.TryParse(patientIdClaim, out int patientId))
                return patientId;
            return null;
        }

        private async Task<IEnumerable<dynamic>> GetUpcomingAppointments(int patientId)
        {
            // Реализация получения предстоящих приемов
            return new List<dynamic>();
        }

        private async Task<IEnumerable<dynamic>> GetPatientAppointments(int patientId)
        {
            // Реализация получения всех приемов пациента
            return new List<dynamic>();
        }
    }
}
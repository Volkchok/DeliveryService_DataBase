using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyclinicDB.Services;
using System.Security.Claims;

namespace PolyclinicDB.Controllers
{
	[Authorize(Policy = "DoctorOnly")]
	public class DoctorController : Controller
	{
		private readonly DatabaseService _dbService;
		private readonly IAuthService _authService;

		public DoctorController(DatabaseService dbService, IAuthService authService)
		{
			_dbService = dbService;
			_authService = authService;
		}

		public async Task<IActionResult> Dashboard()
		{
			var doctorId = GetCurrentDoctorId();
			if (!doctorId.HasValue)
				return RedirectToAction("AccessDenied", "Account");

			ViewBag.TodaysAppointments = await GetTodaysAppointments(doctorId.Value);
			ViewBag.PatientCount = await GetPatientCount(doctorId.Value);

			return View();
		}

		public async Task<IActionResult> Schedule()
		{
			var doctorId = GetCurrentDoctorId();
			if (!doctorId.HasValue)
				return RedirectToAction("AccessDenied", "Account");

			var appointments = await GetDoctorAppointments(doctorId.Value);
			return View(appointments);
		}

		public async Task<IActionResult> MyPatients()
		{
			var doctorId = GetCurrentDoctorId();
			if (!doctorId.HasValue)
				return RedirectToAction("AccessDenied", "Account");

			var patients = await GetDoctorPatients(doctorId.Value);
			return View(patients);
		}

		// Вспомогательные методы
		private int? GetCurrentDoctorId()
		{
			var doctorIdClaim = User.FindFirst("DoctorId")?.Value;
			if (int.TryParse(doctorIdClaim, out int doctorId))
				return doctorId;
			return null;
		}

		private async Task<IEnumerable<dynamic>> GetTodaysAppointments(int doctorId)
		{
			// Реализация получения сегодняшних приемов
			return new List<dynamic>();
		}

		private async Task<int> GetPatientCount(int doctorId)
		{
			// Реализация получения количества пациентов врача
			return 0;
		}

		private async Task<IEnumerable<dynamic>> GetDoctorAppointments(int doctorId)
		{
			// Реализация получения приемов врача
			return new List<dynamic>();
		}

		private async Task<IEnumerable<dynamic>> GetDoctorPatients(int doctorId)
		{
			// Реализация получения пациентов врача
			return new List<dynamic>();
		}
	}
}
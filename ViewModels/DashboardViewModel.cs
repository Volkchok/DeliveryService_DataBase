using PolyclinicDB.Models;

namespace PolyclinicDB.ViewModels
{
    public class DashboardViewModel
    {
        public IEnumerable<Patient> Patients { get; set; }
        public IEnumerable<Doctor> Doctors { get; set; }
        public IEnumerable<Appointment> Appointments { get; set; }

        public DashboardViewModel()
        {
            Patients = new List<Patient>();
            Doctors = new List<Doctor>();
            Appointments = new List<Appointment>();
        }
    }
}
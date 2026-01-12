namespace PolyclinicDB.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int? EmployeeId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Complaints { get; set; }
        public string Diagnosis { get; set; }
        public string Prescription { get; set; }
        public string Status { get; set; } = "scheduled";

        // Убрали навигационные свойства EF
        // public virtual Patient Patient { get; set; }
        // public virtual Doctor Doctor { get; set; }
        // public virtual Employee Employee { get; set; }
    }
}
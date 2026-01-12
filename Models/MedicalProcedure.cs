namespace PolyclinicDB.Models
{
    public class MedicalProcedure
    {
        public int ProcedureId { get; set; }
        public int AppointmentId { get; set; }
        public string ProcedureName { get; set; }
        public string ProcedureResult { get; set; }
        public DateTime ProcedureDate { get; set; }
        public int? DoctorId { get; set; }

        // Ќавигационные свойства (если используете Entity Framework)
        public virtual Appointment Appointment { get; set; }
        public virtual Doctor Doctor { get; set; }
    }
}
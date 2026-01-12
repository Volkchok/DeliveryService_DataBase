namespace PolyclinicDB.Models
{
    public class Patient
    {
        public int PatientId { get; set; }
        public string FullName { get; set; }
        public DateTime Birthdate { get; set; }
        public string Address { get; set; }
        public string Passport { get; set; }
        public string Policy { get; set; }
        public DateTime CreatedDate { get; set; }

        // Убрали виртуальные свойства EF
        // public virtual ICollection<Appointment> Appointments { get; set; }
    }
}
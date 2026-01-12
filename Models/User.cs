namespace PolyclinicDB.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public int? EmployeeId { get; set; }

        // Навигационные свойства
        public Role Role { get; set; }
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public Employee Employee { get; set; }
    }
}
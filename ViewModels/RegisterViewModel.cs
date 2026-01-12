using System.ComponentModel.DataAnnotations;

namespace PolyclinicDB.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите имя пользователя")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов")]
        [Display(Name = "Имя пользователя")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтвердите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Выберите тип учетной записи")]
        [Display(Name = "Тип учетной записи")]
        public string AccountType { get; set; } // "patient", "doctor", "admin"

        // Дополнительные поля для пациентов
        [Display(Name = "ФИО")]
        public string FullName { get; set; }

        [Display(Name = "Дата рождения")]
        [DataType(DataType.Date)]
        public DateTime? Birthdate { get; set; }

        // Дополнительные поля для врачей
        [Display(Name = "Специализация")]
        public string Specialization { get; set; }

        [Display(Name = "Номер лицензии")]
        public string LicenseNumber { get; set; }
    }
}
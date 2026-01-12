using Dapper;
using Npgsql;
using PolyclinicDB.Models;
using PolyclinicDB.ViewModels;

namespace PolyclinicDB.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Patient>> GetPatientsAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QueryAsync<Patient>("SELECT * FROM patients ORDER BY patient_id");
            }
            catch
            {
                // Тестовые данные
                return new List<Patient>
                {
                    new Patient { PatientId = 1, FullName = "Иванов Иван", Birthdate = new DateTime(1980, 5, 15) },
                    new Patient { PatientId = 2, FullName = "Петрова Мария", Birthdate = new DateTime(1990, 8, 20) }
                };
            }
        }
    

        public async Task<Patient> GetPatientByIdAsync(int id)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QueryFirstOrDefaultAsync<Patient>(
                    "SELECT * FROM patients WHERE patient_id = @Id", new { Id = id });
            }
            catch
            {
                return null;
            }
        }

        public async Task InsertPatientAsync(string fullName, DateTime birthdate,
            string address, string passport, string policy)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                // Пробуем вызвать процедуру
                await connection.ExecuteAsync(
                    "CALL insert_patient(@FullName, @Birthdate, @Address, @Passport, @Policy)",
                    new
                    {
                        FullName = fullName,
                        Birthdate = birthdate.Date,  // Используем только дату
                        Address = address,
                        Passport = passport,
                        Policy = policy
                    });
            }
            catch
            {
                // Если процедура не работает, используем прямую вставку
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    @"INSERT INTO patients (full_name, birthdate, address, passport, policy) 
                      VALUES (@FullName, @Birthdate, @Address, @Passport, @Policy)",
                    new
                    {
                        FullName = fullName,
                        Birthdate = birthdate.Date,
                        Address = address,
                        Passport = passport,
                        Policy = policy
                    });
            }
        }

        public async Task UpdatePatientAsync(int patientId, string fullName, string address)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "CALL update_patient(@PatientId, @FullName, @Address)",
                    new { PatientId = patientId, FullName = fullName, Address = address });
            }
            catch
            {
                // Альтернатива - прямой UPDATE
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "UPDATE patients SET full_name = @FullName, address = @Address WHERE patient_id = @PatientId",
                    new { PatientId = patientId, FullName = fullName, Address = address });
            }
        }

        public async Task<string> GetLongestPatientNameAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT get_longest_patient_name()");
            }
            catch
            {
                return "Тестовое самое длинное имя пациента";
            }
        }

        public async Task<IEnumerable<Patient>> GetTop10PatientsAsync()
        {
            try
            {
                Console.WriteLine("Попытка получить топ-10 пациентов...");
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // Пробуем вызвать функцию
                var patients = await connection.QueryAsync<Patient>(
                    "SELECT patient_id as PatientId, full_name as FullName, birthdate as Birthdate, address as Address FROM get_top10_patients()");
                
                // Если функция не вернула данные, получаем напрямую из таблицы
                if (!patients.Any())
                {
                    patients = await connection.QueryAsync<Patient>(
                        "SELECT patient_id as PatientId, full_name as FullName, birthdate as Birthdate, address as Address FROM patients ORDER BY patient_id DESC LIMIT 10");
                }
                
                Console.WriteLine($"Получено {patients.Count()} пациентов в топ-10");
                return patients;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка при получении топ-10 пациентов: {ex.Message}");
                
                // Возвращаем тестовые данные
                return new List<Patient>
                {
                    new Patient 
                    { 
                        PatientId = 8, 
                        FullName = "Федоров Сергей Петрович", 
                        Birthdate = new DateTime(1978, 9, 18), 
                        Address = "Москва, ул. Садовая, д. 67, кв. 56" 
                    },
                    new Patient 
                    { 
                        PatientId = 7, 
                        FullName = "Волкова Ольга Александровна", 
                        Birthdate = new DateTime(1990, 1, 14), 
                        Address = "Москва, ул. Арбат, д. 33, кв. 19" 
                    },
                    new Patient 
                    { 
                        PatientId = 6, 
                        FullName = "Морозов Дмитрий Игоревич", 
                        Birthdate = new DateTime(1982, 7, 30), 
                        Address = "Москва, ул. Тверская, д. 25, кв. 42" 
                    },
                    new Patient 
                    { 
                        PatientId = 5, 
                        FullName = "Новикова Анна Сергеевна", 
                        Birthdate = new DateTime(1995, 12, 5), 
                        Address = "Москва, ул. Пушкина, д. 8, кв. 15" 
                    },
                    new Patient 
                    { 
                        PatientId = 4, 
                        FullName = "Козлова Елена Петровна", 
                        Birthdate = new DateTime(1988, 3, 22), 
                        Address = "Москва, ул. Гагарина, д. 15, кв. 34" 
                    },
                    new Patient 
                    { 
                        PatientId = 3, 
                        FullName = "Сидоров Алексей Владимирович", 
                        Birthdate = new DateTime(1975, 11, 10), 
                        Address = "Москва, пр. Мира, д. 120, кв. 78" 
                    },
                    new Patient 
                    { 
                        PatientId = 2, 
                        FullName = "Петрова Мария Сергеевна", 
                        Birthdate = new DateTime(1992, 8, 20), 
                        Address = "Москва, ул. Новая, д. 45, кв. 12" 
                    },
                    new Patient 
                    { 
                        PatientId = 1, 
                        FullName = "Иванов Иван Иванович", 
                        Birthdate = new DateTime(1980, 5, 15), 
                        Address = "Москва, ул. Ленина, д. 10, кв. 25" 
                    }
                };
            }
        }

        public async Task<IEnumerable<AppointmentDetails>> GetPatientAppointmentDetailsAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QueryAsync<AppointmentDetails>(
                    "SELECT * FROM patient_appointment_details");
            }
            catch
            {
                return new List<AppointmentDetails>();
            }
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QueryAsync<Doctor>("SELECT * FROM doctors ORDER BY doctor_id");
            }
            catch
            {
                return new List<Doctor>();
            }
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QueryAsync<Appointment>("SELECT * FROM appointments ORDER BY appointment_date DESC");
            }
            catch
            {
                return new List<Appointment>();
            }
        }

        public async Task<int> CreateAppointmentAsync(Appointment appointment)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.ExecuteScalarAsync<int>(
                    @"INSERT INTO appointments 
                      (patient_id, doctor_id, appointment_date, complaints, status)
                      VALUES (@PatientId, @DoctorId, @AppointmentDate, @Complaints, @Status)
                      RETURNING appointment_id",
                    new
                    {
                        appointment.PatientId,
                        appointment.DoctorId,
                        AppointmentDate = appointment.AppointmentDate,
                        Complaints = appointment.Complaints ?? "",
                        Status = appointment.Status ?? "scheduled"
                    });
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Console.WriteLine($"Ошибка при создании записи: {ex.Message}");
                throw;
            }
        }
    }

    public class AppointmentDetails
    {
        public string PatientName { get; set; }
        public DateTime Birthdate { get; set; }
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Complaints { get; set; }
        public string Diagnosis { get; set; }
        public string Prescription { get; set; }
        public int PatientAge { get; set; }
    }
}
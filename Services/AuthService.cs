using System.Security.Cryptography;
using System.Text;
using Dapper;
using Npgsql;
using PolyclinicDB.Models;
using PolyclinicDB.ViewModels;

namespace PolyclinicDB.Services
{
    public interface IAuthService
    {
        Task<User> AuthenticateAsync(string username, string password);
        Task<User> RegisterAsync(RegisterViewModel model);
        Task<User> GetUserByIdAsync(int userId);
        Task<bool> UserExistsAsync(string username, string email);
        string HashPassword(string password);
    }

    public class AuthService : IAuthService
    {
        private readonly string _connectionString;

        public AuthService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var user = await connection.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM users WHERE username = @Username AND password_hash = @PasswordHash",
                    new { Username = username, PasswordHash = HashPassword(password) });

                return user;
            }
            catch
            {
                return null;
            }
        }


        public async Task<User> RegisterAsync(RegisterViewModel model)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Определяем role_id на основе типа аккаунта
                int roleId = model.AccountType.ToLower() switch
                {
                    "admin" => 1,
                    "doctor" => 2,
                    "patient" => 3,
                    _ => 3 // По умолчанию пациент
                };

                int? patientId = null;
                int? doctorId = null;

                // Создаем запись в соответствующей таблице
                if (model.AccountType.ToLower() == "patient" && !string.IsNullOrEmpty(model.FullName))
                {
                    // Создаем пациента
                    patientId = await connection.ExecuteScalarAsync<int>(
                        @"INSERT INTO patients (full_name, birthdate, address, passport, policy) 
                          VALUES (@FullName, @Birthdate, '', 'temp', 'temp')
                          RETURNING patient_id",
                        new { model.FullName, model.Birthdate });
                }
                else if (model.AccountType.ToLower() == "doctor" && !string.IsNullOrEmpty(model.FullName))
                {
                    // Создаем врача
                    doctorId = await connection.ExecuteScalarAsync<int>(
                        @"INSERT INTO doctors (full_name, specialization, license_number) 
                          VALUES (@FullName, @Specialization, @LicenseNumber)
                          RETURNING doctor_id",
                        new { model.FullName, model.Specialization, model.LicenseNumber });
                }

                // Создаем пользователя
                var userId = await connection.ExecuteScalarAsync<int>(
                    @"INSERT INTO users (username, email, password_hash, role_id, patient_id, doctor_id) 
                      VALUES (@Username, @Email, @PasswordHash, @RoleId, @PatientId, @DoctorId)
                      RETURNING user_id",
                    new
                    {
                        model.Username,
                        model.Email,
                        PasswordHash = HashPassword(model.Password),
                        RoleId = roleId,
                        PatientId = patientId,
                        DoctorId = doctorId
                    });

                await transaction.CommitAsync();

                return await GetUserByIdAsync(userId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QueryFirstOrDefaultAsync<User>(
                    @"SELECT u.*, r.role_name as RoleName, r.description 
                      FROM users u 
                      INNER JOIN roles r ON u.role_id = r.role_id 
                      WHERE u.user_id = @UserId",
                    new { UserId = userId });
            }
            catch
            {
                return null;
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.QueryFirstOrDefaultAsync<User>(
                    @"SELECT u.*, r.role_name as RoleName, r.description 
                      FROM users u 
                      INNER JOIN roles r ON u.role_id = r.role_id 
                      WHERE u.username = @Username",
                    new { Username = username });
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UserExistsAsync(string username, string email)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                var exists = await connection.ExecuteScalarAsync<bool>(
                    @"SELECT EXISTS(SELECT 1 FROM users WHERE username = @Username OR email = @Email)",
                    new { Username = username, Email = email });
                return exists;
            }
            catch
            {
                return false;
            }
        }

        public string HashPassword(string password)
        {
            // Простая хеш-функция для демонстрации
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            var hash = HashPassword(password);
            return hash == passwordHash;
        }
    }
}
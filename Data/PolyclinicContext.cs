using Microsoft.EntityFrameworkCore;
using PolyclinicDB.Models;

namespace PolyclinicDB.Data
{
    public class PolyclinicContext : DbContext
    {
        public PolyclinicContext(DbContextOptions<PolyclinicContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalProcedure> MedicalProcedures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация Patient
            modelBuilder.Entity<Patient>()
                .HasKey(p => p.PatientId);
            modelBuilder.Entity<Patient>()
                .Property(p => p.PatientId)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.Passport)
                .IsUnique();
            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.Policy)
                .IsUnique();

            // Конфигурация Doctor
            modelBuilder.Entity<Doctor>()
                .HasKey(d => d.DoctorId);
            modelBuilder.Entity<Doctor>()
                .Property(d => d.DoctorId)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Doctor>()
                .HasIndex(d => d.LicenseNumber)
                .IsUnique();

            // Конфигурация Employee
            modelBuilder.Entity<Employee>()
                .HasKey(e => e.EmployeeId);
            modelBuilder.Entity<Employee>()
                .Property(e => e.EmployeeId)
                .ValueGeneratedOnAdd();

            // Конфигурация Appointment
            modelBuilder.Entity<Appointment>()
                .HasKey(a => a.AppointmentId);
            modelBuilder.Entity<Appointment>()
                .Property(a => a.AppointmentId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany()
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Employee)
                .WithMany()
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Конфигурация MedicalProcedure
            modelBuilder.Entity<MedicalProcedure>()
                .HasKey(mp => mp.ProcedureId);
            modelBuilder.Entity<MedicalProcedure>()
                .Property(mp => mp.ProcedureId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<MedicalProcedure>()
                .HasOne(mp => mp.Appointment)
                .WithMany()
                .HasForeignKey(mp => mp.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MedicalProcedure>()
                .HasOne(mp => mp.Doctor)
                .WithMany()
                .HasForeignKey(mp => mp.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
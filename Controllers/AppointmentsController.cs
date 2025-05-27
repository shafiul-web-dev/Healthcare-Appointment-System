using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Healthcare_Appointment_System.Data;
using Healthcare_Appointment_System.DTO;
using Healthcare_Appointment_System.Models;

namespace HealthcareAPI.Controllers
{
	[Route("api/appointments")]
	[ApiController]
	public class AppointmentController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public AppointmentController(ApplicationDbContext context)
		{
			_context = context;
		}

		// 🔹 Get All Appointments with Sorting, Filtering, and Pagination
		[HttpGet]
		public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments(
			[FromQuery] string doctorSpecialization,
			[FromQuery] string status,
			[FromQuery] string sortBy = "appointmentDate",
			[FromQuery] string sortDirection = "asc",
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 10)
		{
			var query = _context.Appointments
				.Include(a => a.Doctor)
				.Include(a => a.Patient)
				.AsQueryable();

			// 🔹 Filtering by Doctor Specialization
			if (!string.IsNullOrEmpty(doctorSpecialization))
			{
				query = query.Where(a => a.Doctor.Specialization == doctorSpecialization);
			}

			// 🔹 Filtering by Appointment Status
			if (!string.IsNullOrEmpty(status))
			{
				query = query.Where(a => a.Status == status);
			}

			// 🔹 Sorting Logic
			query = sortBy switch
			{
				"doctorName" => sortDirection == "asc" ? query.OrderBy(a => a.Doctor.Name) : query.OrderByDescending(a => a.Doctor.Name),
				"appointmentDate" => sortDirection == "asc" ? query.OrderBy(a => a.AppointmentDate) : query.OrderByDescending(a => a.AppointmentDate),
				_ => query
			};

			// 🔹 Pagination
			var totalRecords = await query.CountAsync();
			var appointments = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
				.Select(a => new AppointmentDto
				{
					DoctorName = a.Doctor.Name,
					PatientName = a.Patient.Name,
					AppointmentDate = a.AppointmentDate,
					Status = a.Status
				})
				.ToListAsync();

			return Ok(new { TotalRecords = totalRecords, PageNumber = pageNumber, PageSize = pageSize, Data = appointments });
		}

		// 🔹 Get Single Appointment by ID
		[HttpGet("{id}")]
		public async Task<ActionResult<AppointmentDto>> GetAppointmentById(int id)
		{
			var appointment = await _context.Appointments
				.Include(a => a.Doctor)
				.Include(a => a.Patient)
				.Where(a => a.Id == id)
				.Select(a => new AppointmentDto
				{
					DoctorName = a.Doctor.Name,
					PatientName = a.Patient.Name,
					AppointmentDate = a.AppointmentDate,
					Status = a.Status
				})
				.FirstOrDefaultAsync();

			return appointment == null ? NotFound(new { message = "Appointment not found" }) : Ok(appointment);
		}

		// 🔹 Create New Appointment
		[HttpPost]
		public async Task<ActionResult<Appointment>> AddAppointment(CreateAppointmentDto appointmentDto)
		{
			var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == appointmentDto.DoctorId);
			var patientExists = await _context.Patients.AnyAsync(p => p.Id == appointmentDto.PatientId);

			if (!doctorExists || !patientExists)
			{
				return BadRequest(new { message = "Invalid DoctorId or PatientId" });
			}

			// 🔹 Check if the doctor already has an appointment at the same time
			var isDoctorBusy = await _context.Appointments.AnyAsync(a =>
				a.DoctorId == appointmentDto.DoctorId && a.AppointmentDate == appointmentDto.AppointmentDate);

			if (isDoctorBusy)
			{
				return BadRequest(new { message = "Doctor is already booked for this time." });
			}

			var appointment = new Appointment
			{
				DoctorId = appointmentDto.DoctorId,
				PatientId = appointmentDto.PatientId,
				AppointmentDate = appointmentDto.AppointmentDate,
				Status = appointmentDto.Status
			};

			_context.Appointments.Add(appointment);
			await _context.SaveChangesAsync();
			return CreatedAtAction(nameof(GetAppointmentById), new { id = appointment.Id }, appointment);
		}

		// 🔹 Update Appointment
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateAppointment(int id, CreateAppointmentDto appointmentDto)
		{
			var appointment = await _context.Appointments.FindAsync(id);
			if (appointment == null)
			{
				return NotFound(new { message = "Appointment not found" });
			}

			// 🔹 Prevent updating past appointments
			if (appointment.AppointmentDate < DateTime.Now)
			{
				return BadRequest(new { message = "Past appointments cannot be rescheduled." });
			}

			appointment.AppointmentDate = appointmentDto.AppointmentDate;
			appointment.Status = appointmentDto.Status;

			await _context.SaveChangesAsync();
			return Ok(new { message = "Appointment updated successfully" });
		}

		// 🔹 Delete Appointment
		[HttpDelete("{id}")]
		public async Task<IActionResult> CancelAppointment(int id)
		{
			var appointment = await _context.Appointments.FindAsync(id);
			if (appointment == null)
			{
				return NotFound(new { message = "Appointment not found" });
			}

			appointment.Status = "Canceled";
			await _context.SaveChangesAsync();

			return Ok(new { message = "Appointment marked as Canceled." });
		}
	}
}
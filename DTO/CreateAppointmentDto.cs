using HealthcareAPI.Validations;
using System.ComponentModel.DataAnnotations;

public class CreateAppointmentDto
{
	[Required(ErrorMessage = "DoctorId is required.")]
	public int DoctorId { get; set; }

	[Required(ErrorMessage = "PatientId is required.")]
	public int PatientId { get; set; }

	[Required(ErrorMessage = "Appointment Date is required.")]
	[FutureDate(ErrorMessage = "Appointment date must be in the future.")]
	public DateTime AppointmentDate { get; set; }

	[Required(ErrorMessage = "Status is required.")]
	[RegularExpression("Scheduled|Completed|Canceled", ErrorMessage = "Invalid status.")]
	public string Status { get; set; }
}
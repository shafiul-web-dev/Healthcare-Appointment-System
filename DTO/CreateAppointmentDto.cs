namespace Healthcare_Appointment_System.DTO
{
	public class CreateAppointmentDto
	{
		public int DoctorId { get; set; }
		public int PatientId { get; set; }
		public DateTime AppointmentDate { get; set; }
		public string Status { get; set; }
	}
}

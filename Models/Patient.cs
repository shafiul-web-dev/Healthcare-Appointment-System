namespace Healthcare_Appointment_System.Models
{
	public class Patient
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime DateOfBirth { get; set; }
		public ICollection<Appointment> Appointments { get; set; }
	}
}

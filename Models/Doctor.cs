namespace Healthcare_Appointment_System.Models
{
	public class Doctor
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Specialization { get; set; }
		public ICollection<Appointment> Appointments { get; set; }
	}
}

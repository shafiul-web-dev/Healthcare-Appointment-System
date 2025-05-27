using System.ComponentModel.DataAnnotations;

namespace HealthcareAPI.Validations
{
	public class FutureDateAttribute : ValidationAttribute
	{
		public override bool IsValid(object value)
		{
			if (value is DateTime date)
			{
				return date > DateTime.Now;
			}
			return false;
		}
	}
}
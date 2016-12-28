namespace Ombi.UI.Models
{
	public class ApiModel<T>
	{
		public T Data{ get; set; }
		public bool Error{get;set;}
		public string ErrorMessage{ get; set; }
	}
}


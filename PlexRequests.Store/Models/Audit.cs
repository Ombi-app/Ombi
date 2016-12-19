using System;
using Dapper.Contrib.Extensions;

namespace Ombi.Store.Models
{
	[Table("Audit")]
	public class Audit : Entity
	{
		public string Username{get;set;}
		public DateTime Date {get;set;}
		public string ChangeType {get;set;}
		public string OldValue {get;set;}
		public string NewValue{get;set;}
	}
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControleCaixaWeb.Models
{
	public class DateRange : System.ComponentModel.DataAnnotations.ValidationAttribute
	{
		public string StartDate { get; set; }
		public string EndDate { get; set; }

		public DateRange( )
		{
			this.StartDate = new DateTime(2000, 1, 1).ToString( );
			this.EndDate = new DateTime(2099, 1, 1).ToString( );
		}

		public override bool IsValid(object value)
		{
			var valueToString = value as string;

			if (!string.IsNullOrEmpty(valueToString))
			{
				DateTime dateTimeResult;

				if (DateTime.TryParse(valueToString, out dateTimeResult))
				{
					return ((dateTimeResult >= DateTime.Parse(this.StartDate)) && (dateTimeResult <= DateTime.Parse(this.EndDate)));
				}

				return false;
			}
			return true;
		}
	}
}
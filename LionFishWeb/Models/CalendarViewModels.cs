using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LionFishWeb.Models
{
    public class CalendarViewModel
    {
		public CalendarViewModel()
		{
			ID = "";
			//DateT = new DateTime();
		}
		public string ID { get; set; }
		public DateTime DateT { get; set; }
	}

    public class SetEventViewModel
    {
		
        public string ID { get; set; }
    }
}
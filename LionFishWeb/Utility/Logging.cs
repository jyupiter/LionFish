using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LionFishWeb.Utility
{
	public class Logging
	{
		public static void Log(string input)
		{
			using (System.IO.StreamWriter file =
			new System.IO.StreamWriter(@"Logs.txt", true))
			{
				file.WriteLine(input);
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace LionFishWeb.Utility
{
	public class Logging
	{
		public static void Log(string input)
		{
			Debug.WriteLine(input);
            string startupPath = HttpRuntime.AppDomainAppPath;
            using (System.IO.StreamWriter file =
			new System.IO.StreamWriter(startupPath + "\\Logs.txt", true))
			{
				file.WriteLine(input);
			}
		}
	}
}
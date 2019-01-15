using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LionFishWeb.Models
{
    public class DashboardViewModel
    {
        public string ProfileImg { get; set; }
    }

    public class UpdatePrivacyViewModel
    {
        public string Private { get; set; }
    }

    public class UpdatePasswordViewModel
    {
        public string Password { get; set; }
    }
}
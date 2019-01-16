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

    public class UpdateProfileImgViewModel
    {
        public string ProfileImg { get; set; }
    }

    public class UpdateProfileInfoViewModel
    {
        public string Name { get; set; }
        public string Info { get; set; }
    }

    public class UpdatePasswordViewModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
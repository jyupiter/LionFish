using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LionFishWeb.Models
{
    public class GroupUser
    {
        [Key, Column(Order = 1)]
        public string UserID { get; set; }
        [Key, Column(Order = 2)]
        public string GroupID { get; set; }
    }
}
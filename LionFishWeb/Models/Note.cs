using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LionFishWeb.Models
{
    public class Note
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public string UserID { get; set; }
        public int EventID { get; set; }

        public virtual User User { get; set; }
        public virtual Event Event { get; set; }

        public Note(User user)
        {
            Title = "New note";
            Content = "";
            UserID = user.Id;
        }
    }
}
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

        public int FolderID { get; set; }
        public string UserID { get; set; }
        public string EventID { get; set; }

        public Note()
        {
            Title = "New note";
        }

        public Note(string Id)
        {
            Title = "New note";
            Content = "";
            UserID = Id;
        }
    }
}
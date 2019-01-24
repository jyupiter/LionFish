using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LionFishWeb.Models
{
    public class Folder
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ID { get; set; }
        public string Name { get; set; }

        public string UserID { get; set; }

        public virtual ICollection<Note> Notes { get; set; }
        public virtual User User { get; set; }

        public Folder()
        {
            Name = "New folder";
            ID = "" + Guid.NewGuid();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LionFishWeb.Models
{
    public class Group
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ID { get; set; }      // NOTE THIS IS STRING AND NOT INT
        public string GroupName { get; set; }
        public string GroupDesc { get; set; }
        public string GroupImage { get; set; }
        public string Owner_Id { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<User> User { get; set; }
        public virtual ICollection<Note> Notes { get; set; }
        public virtual ICollection<Event> Events { get; set; }

        public Group() { }
        public Group(string GroupName, string GroupDesc, string GroupImage)
        {
            GroupName = this.GroupName;
            GroupDesc = this.GroupDesc;
            GroupImage = this.GroupImage;
            User = new List<User>();
            ID = generateID();
        }

        public string generateID()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
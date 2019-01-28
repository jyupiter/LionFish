using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LionFishWeb.Models
{
    public class Group
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ID { get; set; }
        public string GroupName { get; set; }
        public string GroupDesc { get; set; }
        public string GroupImage { get; set; }
        public string GroupMessage { get; set; }
        public string Owner_Id { get; set; }
        public virtual ICollection<User> User { get; set; }
        public virtual ICollection<Note> Notes { get; set; }
        public virtual ICollection<Event> Events { get; set; }
        public Group(string ID, string GroupName, string GroupDesc, string GroupImage, string GroupMessage, string Owner_Id)
        {
            this.ID = ID;
            this.GroupName = GroupName;
            this.GroupDesc = GroupDesc;
            this.GroupImage = GroupImage;
            this.GroupMessage = GroupMessage;
            this.Owner_Id = Owner_Id;
        }
        public Group()
        {

        }
        public override string ToString()
        {
            return GroupName + " " + GroupName;
            ;
        }
    }
}
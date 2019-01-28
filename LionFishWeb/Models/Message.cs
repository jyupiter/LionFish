using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LionFishWeb.Models;
namespace LionFishWeb.Models
{
    public class Message
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string message { get; set; }

        public string User_Id { get; set; }
        public string UserName { get; set; }

        public Message(int ID, string message, string User_Id, string UserName)
        {
            this.message = message;
            this.ID = ID;
            this.User_Id = User_Id;
            this.UserName = UserName;

        }
        public Message()
        {


        }
    }
}
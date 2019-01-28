using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LionFishWeb.Models
{
    public class Feedback
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int feedback_Id { get; set; }
        public string rate { get; set; }
        public string feedbackMessage { get; set; }

        public Feedback(string rate, string feedbackMessage)
        {
            this.rate = rate;
            this.feedbackMessage = feedbackMessage;




        }
        public Feedback()
        {
        }


        public virtual User Users { get; set; }
    }
}
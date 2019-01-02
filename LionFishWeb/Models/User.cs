using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace LionFishWeb.Models
{
    public class User : IdentityUser
    {
       public string ProfileImg { get; set; }
        public string ProfileBio { get; set; }

        public virtual ICollection<Note> Notes { get; set; }
        public virtual ICollection<Group> Groups { get; set; }
        public virtual ICollection<User> Friends { get; set; }
        public virtual ICollection<Event> Events { get; set; }

        public User()
        {
            UserName = "";
            Notes = new List<Note>();
            Groups = new List<Group>();
            Friends = new List<User>();
            Events = new List<Event>();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}
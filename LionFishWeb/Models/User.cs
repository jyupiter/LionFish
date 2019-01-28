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
        public string Private { get; set; }

        public ICollection<string> Notes { get; set; }
        public ICollection<string> Groups { get; set; }
        public ICollection<string> Friends { get; set; }
        public ICollection<string> Events { get; set; }

        public User()
        {
            UserName = "";
            ProfileImg = "https://profiles.utdallas.edu/img/default.png";
            Notes = new List<string>();
            Groups = new List<string>();
            Friends = new List<string>();
            Events = new List<string>();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }
}
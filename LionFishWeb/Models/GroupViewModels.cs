using System.Collections.Generic;

namespace LionFishWeb.Models
{
    public class GroupViewModels
    {
        public class GroupViewModel
        {
            public Group group { get; set; }
            public string username { get; set; }
            public SearchViewModel search { get; set; }
        }


        public class GroupsViewModel
        {
            public IEnumerable<Group> Grouped { get; set; }
            public Group Groups { get; set; }
            public SearchViewModel SearchView { get; set; }
            public UserModel UserM { get; set; }
        }

        public class SearchViewModel
        {
            public string Search { get; set; }

        }

        public class UserModel
        {
            public string UserName;

        }

        public class UpdateGroupModel
        {
            public string Name;
            public string Data;

        }
    }
}
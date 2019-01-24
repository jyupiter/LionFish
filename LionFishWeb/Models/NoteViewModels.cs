using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LionFishWeb.Models
{
    public class CreateNoteViewModel
    {
        public string Title { get; set; }
        public string Name { get; set; }
    }

    public class CreateFolderViewModel
    {
        public string Name { get; set; }
    }

    public class UpdateNoteViewModel
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class NoteViewModel {
        public List<Note> Notes { get; set; }
    }

    public class FolderViewModel
    {
        public List<Folder> Folders { get; set; }
    }

    public class NoteFolderViewModel
    {
        public NoteViewModel NVM { get; set; }
        public FolderViewModel FVM { get; set; }

        public NoteFolderViewModel()
        {
            NVM = new NoteViewModel();
            FVM = new FolderViewModel();
        }
    }
}
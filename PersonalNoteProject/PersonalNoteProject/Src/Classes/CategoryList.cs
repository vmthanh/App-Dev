using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalNoteProject.Src.Classes
{
    public class CategoryList
    {
       public string CategoryName { get; set; }
       public string CategoryImage { get; set; }
       public List<Note> listNotes { get; set; }

        public CategoryList()
        {
            CategoryImage = "";
            CategoryName = "";
            listNotes = new List<Note>();
        }
    }
}

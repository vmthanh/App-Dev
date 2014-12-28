using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;
using Microsoft.Xna.Framework;

namespace PersonalNoteProject.Src.Classes
{
    public class Note
    {

        public string Title { get; set; }
        public string Content { get; set; }
        public string Keywords { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime ExpirationTime { get; set; }
        public RecurrenceInterval RecurrenceType { get; set; }
        public bool IsScheduled { get; set; }
        // public ListPicker RecurrenceType { get; set; }
        public string IdName { get; set; }

      
        public double longtitue { get; set; }
        public double langtitue { get; set; }
        public Brush imageSouce { get; set; }

        public Note()
        {
            Title = "";
            Content = "";
            Keywords = "";
            IdName = "";
        }


    }
}

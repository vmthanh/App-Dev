using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Primitives;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using PersonalNoteProject.Src.Classes;
using PersonalNoteProject.ViewModels;

namespace PersonalNoteProject.Src.Pages
{
    public partial class NotesPage : PhoneApplicationPage
    {
        public static List<Note> listNotes;
        public static string CategoryName;
        private int index;
        public static bool isChanged =false;
        public NotesPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            

          
            
            if (NoteEditor.isSaveClick == true )
            {
                
                listNotes.Add(NoteEditor.itemNote);
                NoteEditor.isSaveClick = false;
                isChanged = true;
            }
            if (NoteEditor.isReplace == true)
            {
                listNotes[index] = NoteEditor.itemNote;
               
                NoteEditor.isReplace = false;
                isChanged = true;
            }
            if (NoteEditor.isDelete == true)
            {
                listNotes.RemoveAt(index);
                NoteEditor.isDelete = false;
                isChanged = true;
            }
          
       

            List<AlphaKeyGroup<Note>> DataSource = AlphaKeyGroup<Note>.CreateGroups(listNotes,
               System.Threading.Thread.CurrentThread.CurrentUICulture,
               (Note s) => { return s.Title; }, true);
            listNote.ItemsSource = DataSource;
            listNote.SelectedItem = null;

        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
          
            NavigationService.Navigate(new Uri("/Src/Pages/NoteEditor.xaml", UriKind.Relative));

        }

        private void listNote_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            LongListSelector parent = (LongListSelector)sender;
        
            Note selectedItem;
            if (parent.SelectedItem != null)
            {
                selectedItem = (Note) parent.SelectedItem;
            }
            else return;
            
         
            if (selectedItem != null)
            {
                

                index = listNotes.IndexOf(selectedItem);
                NoteEditor.sorceImageExist = selectedItem.imageSouce;
                NoteEditor.OldKeyWorld = selectedItem.Keywords;
                NoteEditor.coordinate = new GeoCoordinate(selectedItem.langtitue,selectedItem.longtitue);
                NavigationService.Navigate(new Uri("/Src/Pages/NoteEditor.xaml?name=" + selectedItem.IdName, UriKind.Relative));
            }
        }
    }
}
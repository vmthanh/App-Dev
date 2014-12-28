using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Toolkit;
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using PersonalNoteProject.Resources;
using PersonalNoteProject.Src.Classes;
using PersonalNoteProject.Src.Pages;
using System.Windows.Media;

namespace PersonalNoteProject
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        private ObservableCollection<CategoryList> categoriesList;
        private CategoryRepository repository = new CategoryRepository();
        private int index;
        private bool isSearchClicked = false;
        private List<Note> listNoteOfCate;
        private double _accuracy = 0.0;


        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;


            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }


        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            var results = repository.Load();
            categoriesList = new ObservableCollection<CategoryList>(results);
            if (CategoryEditor.categoryItem != null)
            {
                var categoryItem = CategoryEditor.categoryItem;
                categoriesList.Add(categoryItem);

                repository.Save(categoriesList.ToList());
                CategoryEditor.categoryItem = null;


            }
            if (isSearchClicked == true)
            {
                listNoteOfCate[index] = NoteEditor.itemNote;
            }

            listCategory.ItemsSource = categoriesList;
            if (listCategory.SelectedItem != null)
                listCategory.SelectedItem = null;
            if (listSearch.SelectedItem != null)
                listSearch.SelectedItem = null;
            if (NoteEditor.isReplace == true)
            {
                foreach (CategoryList category in categoriesList)
                {
                    List<Note> listNotes = new List<Note>();
                    listNotes = category.listNotes;

                    for (int i = 0; i < listNotes.Count; ++i)
                    {
                        if (listNotes[i].Title == NoteEditor.previousNoteName)
                        {
                            listNotes[i] = NoteEditor.itemNote;
                            break;
                        }
                    }
                    break;
                }
            }
            if (NotesPage.isChanged == true)
            {
                repository.Save(categoriesList.ToList());
                NotesPage.isChanged = false;

            }

        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {

            NavigationService.Navigate(new Uri("/Src/Pages/CategoryEditor.xaml", UriKind.Relative));
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            categoriesList.Clear();
            repository.Clear();
            listCategory.ItemsSource = categoriesList;
            var notifications = ScheduledActionService
               .GetActions<ScheduledNotification>()
               .OrderBy((item) => item.BeginTime);

            foreach (ScheduledNotification notification in notifications)
            {
                if (ScheduledActionService.Find(notification.Name) != null)
                    ScheduledActionService.Remove(notification.Name);
            }

        }

        private void listCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            LongListSelector parent = (LongListSelector)sender;
            CategoryList selectedItem;

            if (parent.SelectedItem != null)
            {
                selectedItem = (CategoryList)parent.SelectedItem;
            }
            else return;

            NotesPage.listNotes = selectedItem.listNotes;
            NotesPage.CategoryName = selectedItem.CategoryName;

            NavigationService.Navigate(new Uri("/Src/Pages/NotesPage.xaml", UriKind.Relative));


        }



        private void Search_tbl_TextChanged(object sender, TextChangedEventArgs e)
        {
            var notifications = ScheduledActionService
            .GetActions<ScheduledNotification>()
            .OrderBy((item) => item.BeginTime);
            List<ScheduledAction> items = new List<ScheduledAction>();
            foreach (ScheduledNotification notification in notifications)
            {
                if (notification.Title.Contains(Search_tbl.Text))
                {
                    ScheduledAction item = ScheduledActionService.Find(notification.Name);
                    items.Add(item);
                }

            }
            listSearch.ItemsSource = items;
        }

        private void listSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            LongListSelector parent = (LongListSelector)sender;
            ScheduledNotification selectedItem;

            if (parent.SelectedItem != null)
            {
                selectedItem = (ScheduledNotification)parent.SelectedItem;
            }
            else return;


            if (selectedItem != null)
            {
                for (int i = 0; i < categoriesList.Count; ++i)
                {
                    listNoteOfCate = categoriesList[i].listNotes;
                    for (int j = 0; j < listNoteOfCate.Count; ++j)
                        if (listNoteOfCate[j].Title == selectedItem.Title &&
                            listNoteOfCate[j].Content == selectedItem.Content &&
                            listNoteOfCate[j].BeginTime == selectedItem.BeginTime)
                        {
                            index = i;
                            isSearchClicked = true;
                            NavigationService.Navigate(new Uri("/Src/Pages/NoteEditor.xaml?name=" + selectedItem.Name, UriKind.Relative));
                            break;

                        }
                    break;

                }

            }
        }

        private async Task ReDraw(int mode, List<Note> listNoteOneCate)
        {
            if (mode == 0)
            {
                MapNotes.Layers.Clear();
                Geolocator locator = new Geolocator();
                Geoposition geoPosition = await locator.GetGeopositionAsync();
                GeoCoordinate coordinate = new GeoCoordinate();
                coordinate = geoPosition.Coordinate.ToGeoCoordinate();
                MapLayer mapLayer = new MapLayer();
                foreach (CategoryList category in categoriesList)
                {
                    List<Note> listNotes = new List<Note>();
                    listNotes = category.listNotes;

                    for (int i = 0; i < listNotes.Count; ++i)
                    {
                        // DrawMapMarkers(category.CategoryImage,listNotes[i].langtitue,listNotes[i].longtitue);
                        UCCustomToolTip _toolTip = new UCCustomToolTip();
                        _toolTip.Description = listNotes[i].Title + "\n" + listNotes[i].Content;
                        Note temp = new Note();
                        temp = listNotes[i];
                        _toolTip.DataContext = temp;


                        _toolTip.UpdateItem.Click += UpdateItem_Click;
                        _toolTip.DeleteItem.Click += DeleteItem_Click;
                        _toolTip.imgmarker.Tap += imgmarker_Tap;

                        MapOverlay overlay = new MapOverlay();
                        overlay.Content = _toolTip;
                        overlay.GeoCoordinate = new GeoCoordinate(listNotes[i].langtitue, listNotes[i].longtitue);
                        overlay.PositionOrigin = new Point(0.0, 1.0);
                        mapLayer.Add(overlay);
                    }
                }
                MapNotes.Center = coordinate;
                MapNotes.ZoomLevel = 10;
                MapNotes.Layers.Add(mapLayer);
            }
            else
            {
                MapNotes.Layers.Clear();
                Geolocator locator = new Geolocator();
                Geoposition geoPosition = await locator.GetGeopositionAsync();
                GeoCoordinate coordinate = new GeoCoordinate();
                coordinate = geoPosition.Coordinate.ToGeoCoordinate();
                MapLayer mapLayer = new MapLayer();


                for (int i = 0; i < listNoteOneCate.Count; ++i)
                {
                    // DrawMapMarkers(category.CategoryImage,listNotes[i].langtitue,listNotes[i].longtitue);
                    UCCustomToolTip _toolTip = new UCCustomToolTip();
                    _toolTip.Description = listNoteOneCate[i].Title + "\n" + listNoteOneCate[i].Content;
                    Note temp = new Note();
                    temp = listNoteOneCate[i];
                    _toolTip.DataContext = temp;


                    _toolTip.UpdateItem.Click += UpdateItem_Click;
                    _toolTip.DeleteItem.Click += DeleteItem_Click;
                    _toolTip.imgmarker.Tap += imgmarker_Tap;

                    MapOverlay overlay = new MapOverlay();
                    overlay.Content = _toolTip;
                    overlay.GeoCoordinate = new GeoCoordinate(listNoteOneCate[i].langtitue, listNoteOneCate[i].longtitue);
                    overlay.PositionOrigin = new Point(0.0, 1.0);
                    mapLayer.Add(overlay);
                }
                MapNotes.Center = coordinate;
                MapNotes.ZoomLevel = 10;
                MapNotes.Layers.Add(mapLayer);
            }


        }


        private async void MapNotes_Loaded(object sender, RoutedEventArgs e)
        {
            NoteViewBtn.IsChecked = true;
            CateViewBtn.IsChecked = false;
            pickerCategory.IsEnabled = false;
            List<Note> temp = new List<Note>();
            await ReDraw(0, temp);
        }

        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you really want to delete note?", "Confirm", MessageBoxButton.OKCancel) ==
                MessageBoxResult.OK)
            {
                try
                {
                    MenuItem item = (MenuItem)sender;
                    string selecteditem = item.Tag.ToString();
                    foreach (CategoryList category in categoriesList)
                    {
                        List<Note> listNotes = new List<Note>();
                        listNotes = category.listNotes;

                        for (int i = 0; i < listNotes.Count; ++i)
                        {
                            if (listNotes[i].Title == selecteditem)
                            {
                                listNotes.RemoveAt(i);
                                List<Note> temp = new List<Note>(); 
                                await ReDraw(0,temp);
                                return;
                            }

                        }
                    }
                }
                catch
                {

                }
            }
        }

        void UpdateItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem item = (MenuItem)sender;
                string selecteditem = item.Tag.ToString();
                foreach (CategoryList category in categoriesList)
                {
                    List<Note> listNotes = new List<Note>();
                    listNotes = category.listNotes;

                    for (int i = 0; i < listNotes.Count; ++i)
                    {
                        if (listNotes[i].Title == selecteditem)
                        {
                            index = i;
                            NoteEditor.sorceImageExist = listNotes[i].imageSouce;
                            NoteEditor.previousNoteName = listNotes[i].Title;
                            
                            NoteEditor.OldKeyWorld = listNotes[i].Keywords;
                            NoteEditor.coordinate = new GeoCoordinate(listNotes[i].langtitue, listNotes[i].longtitue);
                            NavigationService.Navigate(new Uri("/Src/Pages/NoteEditor.xaml?name=" + listNotes[i].IdName, UriKind.Relative));
                        }
                        break;
                    }
                    break;
                }
            }
            catch
            {
                MessageBox.Show("Sorry! Sometimes shit happens :'( ", "Failed", MessageBoxButton.OK);
            }
        }

        private void imgmarker_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                Image item = (Image)sender;
                string selecteditem = item.Tag.ToString();

                foreach (CategoryList category in categoriesList)
                {
                    List<Note> listNotes = new List<Note>();
                    listNotes = category.listNotes;

                    for (int i = 0; i < listNotes.Count; ++i)
                    {
                        if (listNotes[i].Title == selecteditem)
                        {
                            ContextMenu contextMenu =
                                ContextMenuService.GetContextMenu(item);
                            contextMenu.DataContext = listNotes[i];
                            if (contextMenu.Parent == null)
                            {
                                contextMenu.IsOpen = true;

                            }
                            break;
                        }

                    }
                }


            }
            catch
            {
                MessageBox.Show("Sorry! Sometimes shit happens :'( ", "Failed", MessageBoxButton.OK);
            }
        }




        private Color ConvertStringToColor(String hex)
        {
            //remove the # at the front
            hex = hex.Replace("#", "");

            byte a = 255;
            byte r = 255;
            byte g = 255;
            byte b = 255;

            int start = 0;

            //handle ARGB strings (8 characters long)
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                start = 2;
            }

            //convert RGB characters to bytes
            r = byte.Parse(hex.Substring(start, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }

        private async void RadioButtonCheck(object sender, RoutedEventArgs e)
        {
            if (CateViewBtn.IsChecked == true)
            {
                pickerCategory.IsEnabled = true;
                List<ListPickerItem> items = new List<ListPickerItem>();
                for (int i = 0; i < categoriesList.Count; ++i)
                {
                    ListPickerItem item = new ListPickerItem();
                    item.Content = categoriesList[i].CategoryName;
                    items.Add(item);
                }
                pickerCategory.ItemsSource = items;

            }
            else
            {
                pickerCategory.IsEnabled = false;
                List<Note> temp = new List<Note>(); 
                await ReDraw(0,temp);

            }
        }

        private void pickerCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListPicker parent = (ListPicker)sender;
            ListPickerItem selectedItem;

            if (parent.SelectedItem != null)
            {
                selectedItem = (ListPickerItem)parent.SelectedItem;
            }
            else return;

            if (selectedItem.Content.ToString() != "")
            {
                List<Note> listNoteOneCate = new List<Note>();
                foreach (CategoryList item in categoriesList )
                {
                    if (item.CategoryName == selectedItem.Content.ToString())
                    {
                        listNoteOneCate = item.listNotes;
                        ReDraw(1, listNoteOneCate);
                    }
                }
            }

        }









        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PersonalNoteProject.Src.Classes;

namespace PersonalNoteProject.Src.Pages
{
    public class ColorSet
    {
        public string FillColor { get; set; }
        public  string CorlorName { get; set; }
    }

    public partial class CategoryEditor : PhoneApplicationPage
    {
        public static CategoryList categoryItem;
        private List<ColorSet> source; 
        public CategoryEditor()
        {
            InitializeComponent();
            source = new List<ColorSet>();
            source.Add(new ColorSet() { CorlorName = "Red", FillColor = "#FF0000" });
            source.Add(new ColorSet() { CorlorName = "Blue", FillColor = "#3333CC" });
            source.Add(new ColorSet() { CorlorName = "Yellow", FillColor = "#FFFF00" });
            source.Add(new ColorSet() { CorlorName = "Orange", FillColor = "#FF6600" });
            source.Add(new ColorSet() { CorlorName = "Purple", FillColor = "#CC00CC" });
            this.listPickerIcon.ItemsSource = source;
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            categoryItem = new CategoryList();
            categoryItem.CategoryName = CateogryName.Text;
            ColorSet item = (ColorSet) listPickerIcon.SelectedItem;

            categoryItem.CategoryImage = item.FillColor;
             NavigationService.GoBack();

        }

     
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Microsoft.Phone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Services;
using Microsoft.Phone.Maps.Toolkit;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Audio;
using PersonalNoteProject.Src.Classes;
using System.Device.Location;
using Windows.Devices.Geolocation;
using Reminder = Microsoft.Phone.Scheduler.Reminder;

namespace PersonalNoteProject.Src.Pages
{
    public class VoiceRecording
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
    }

    public partial class NoteEditor : PhoneApplicationPage
    {
        public static Note itemNote;
        public static string previousNoteName;
        public static string NoteIdName;
        public static string OldKeyWorld = null;
        private Reminder reminder;
       public UserLocationMarker maker;
      public static  GeoCoordinate coordinate = null;
        public static bool isSaveClick = false;
        public static bool isReplace = false;
        public static bool isDelete = false;
        private WriteableBitmap currentImage;
        public static Brush sorceImageExist;
        MemoryStream audioStream = null;
        byte[] audioBuffer = null;
        SoundEffectInstance audioPlayerInstance = null;
        private GeoCoordinate MyCoordinate = null;
        private GeocodeQuery MyGeocodeQuery = null;
        private Geolocator service;
        private GeoCoordinate previous;
        private double Distance;

        private bool _isRouteSearch = false; 
        public NoteEditor()
        {
            InitializeComponent();
            RecurrenceInterval[] values = (RecurrenceInterval[])Enum.GetValues(typeof(RecurrenceInterval));
            listPicker.ItemsSource = values;
            Microphone.Default.BufferDuration = TimeSpan.FromSeconds(1);
            Microphone.Default.BufferReady += microphone_BufferReady;
            recordingList.ItemsSource = new ObservableCollection<VoiceRecording>();

        }

        private void microphone_BufferReady(object sender, EventArgs e)
        {
            int count = Microphone.Default.GetData(audioBuffer);
            audioStream.Write(audioBuffer, 0, count);
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {

            reminder.Title = titleBox.Text;

            reminder.Content = titleDescription.Text;
            DateTime date = datePicker.Value.Value;
            DateTime time = timePicker.Value.Value;
            reminder.BeginTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
           
            try
            {
                if (ScheduledActionService.Find(reminder.Name) == null)
                {


                    ScheduledActionService.Add(reminder);

                    isSaveClick = true;
                }
                else
                {
                    ScheduledActionService.Replace(reminder);

                    isReplace = true;
                }
                itemNote = new Note();
                itemNote.Title = reminder.Title;
                itemNote.Content = reminder.Content;
                itemNote.BeginTime = reminder.BeginTime;
                itemNote.Keywords = titleKeywords.Text;
                itemNote.IdName = reminder.Name;
                if (coordinate != null)
                {
                    itemNote.langtitue = coordinate.Latitude;
                    itemNote.longtitue = coordinate.Longitude;
                }
                else
                {
                    itemNote.langtitue = 0;
                    itemNote.longtitue = 0;
                }
           

                NavigationService.GoBack();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                if (NavigationContext.QueryString.ContainsKey("name"))
                {
                    string name = NavigationContext.QueryString["name"];
                    reminder = (Reminder)ScheduledActionService.Find(name);
                    this.titleKeywords.Text = OldKeyWorld;
                    photoContainer.Fill = sorceImageExist;   
                }
                else
                {
                    reminder = new Reminder(Guid.NewGuid().ToString());
                    reminder.Title = "Reminder";
                    reminder.BeginTime = DateTime.Now;
                    reminder.NavigationUri = new Uri(
                            "/MainPage.xaml?reminder=" + reminder.Name, UriKind.Relative);
                }
               
                this.DataContext = reminder;
                await DisplayRecordingNames();
            }
          
        }

        private async Task DisplayRecordingNames()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;

            if (folder != null)
            {
                IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
                string FileName = reminder.Name + ".wav";
                foreach (StorageFile file in files)
                {
                    if (file.Name == FileName)
                        recordingList.ItemsSource.Add(new VoiceRecording { Title = file.Name, Date = file.DateCreated.DateTime });
                }     
            }
           
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            if (ScheduledActionService.Find(reminder.Name) != null)
            {
                ScheduledActionService.Remove(reminder.Name);
                isDelete = true;
            }
             
            NavigationService.GoBack();
        }

        private async void mapControl_Loaded(object sender, RoutedEventArgs e)
        {
            Geolocator locator = new Geolocator();
            Geoposition geoPosition = await locator.GetGeopositionAsync();
            if (coordinate == null)
            {

                coordinate = new GeoCoordinate();
                coordinate = geoPosition.Coordinate.ToGeoCoordinate();
            }
            maker = new UserLocationMarker();
            maker.GeoCoordinate = coordinate;
            previous = new GeoCoordinate();
            previous = coordinate;
            MapExtensions.GetChildren(mapControl).Add(maker);
            mapControl.Center = coordinate;
            mapControl.ZoomLevel = 10;
        }

        private async void mapControl_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MapExtensions.GetChildren(mapControl).Clear();
            var point = e.GetPosition(mapControl);
            coordinate = new GeoCoordinate();
            coordinate = mapControl.ConvertViewportPointToGeoCoordinate(point);
            maker = new UserLocationMarker();
            maker.GeoCoordinate = coordinate;
            previous = new GeoCoordinate();
            previous = coordinate;
            MapExtensions.GetChildren(mapControl).Add(maker);
            
            string position = "Do you want to choose this place? \n ";
            position += string.Format("Latitude: {0}\nLongitude: {1}\n",
            FormatCoordinate(coordinate.Latitude, 'N', 'S'),
            FormatCoordinate(coordinate.Longitude, 'E', 'W'));
            ReverseGeocodeQuery query = new ReverseGeocodeQuery();
            query.GeoCoordinate = coordinate;
            IList<MapLocation> results = await query.GetMapLocationsAsync();
            position += string.Format("{0} locations found.\n", results.Count);
            MapLocation location = results.FirstOrDefault();
            if (location != null)
            {
                position += FormatAddress(location.Information.Address);
            }
            MessageBox.Show(position, "Address", MessageBoxButton.OK);

        }

      

        private string FormatAddress(MapAddress address)
        {
            StringBuilder b = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(address.HouseNumber))
                b.AppendFormat("{0} ", address.HouseNumber);
            if (!string.IsNullOrWhiteSpace(address.Street))
                b.AppendFormat("{0}\n", address.Street);
            if (!string.IsNullOrWhiteSpace(address.City))
                b.AppendFormat("{0}, ", address.City);
            b.AppendFormat("{0} {1}", address.State, address.PostalCode);
            return b.ToString();
        }

        private string FormatCoordinate(double coordinate, char positive, char negative)
        {
            char direction = coordinate >= 0 ? positive : negative;
            coordinate = Math.Abs(coordinate);
            double degrees = Math.Floor(coordinate);
            double minutes = Math.Floor((coordinate - degrees) * 60.0D);
            double seconds = (((coordinate - degrees) * 60.0D) - minutes) * 60.0D;
            string result = string.Format("{0}{1:F0}° {2:F0}' {3:F1}\"",
            direction, degrees, minutes, seconds);
            return result;
        }

        private void ChoosePhoto_Click(object sender, EventArgs e)
        {
            var task = new PhotoChooserTask();
            task.ShowCamera = true;
            task.Completed += task_Completed;
            task.Show();
        }


        void task_Completed(object sender, PhotoResult e)
        {

            if (e.TaskResult == TaskResult.OK)
            {
                currentImage = PictureDecoder.DecodeJpeg(e.ChosenPhoto);
                photoContainer.Fill = new
                ImageBrush { ImageSource = currentImage };


            }
            else
            {
                photoContainer.Fill = new SolidColorBrush(Colors.Gray);

            }


        }

        private void play_Click(object sender, RoutedEventArgs e)
        {
            if (audioPlayerInstance != null &&
             audioPlayerInstance.State == SoundState.Playing)
            {
                audioPlayerInstance.Pause();
            }
            else
            {
                var button = (Button)sender;
                string filename = (string)button.Tag;
                PlayFile(filename);
            }
        }

        private void ApplicationBarIconButton_Click_2(object sender, EventArgs e)
        {
            if (Microphone.Default.State == MicrophoneState.Stopped)
            {
                try
                {
                    recordingList.IsEnabled = false;


                    audioStream = new MemoryStream();
                    audioBuffer = new byte[Microphone.Default.GetSampleSizeInBytes(TimeSpan.FromSeconds(1))];

                    Microphone.Default.Start();
                }
                catch (Exception)
                {
                    MessageBox.Show("Sorry. Sometimes shit happens", "Error", MessageBoxButton.OK);
                    
                }
                
            }
        }

        private async void ApplicationBarIconButton_Click_3(object sender, EventArgs e)
        {
            if (Microphone.Default.State == MicrophoneState.Started)
            {
                try
                {
                    Microphone.Default.Stop();
                    string filename = await WriteFile();
                    audioBuffer = null;
                    audioStream = null;

                    recordingList.IsEnabled = true;
                    recordingList.ItemsSource.Add(new VoiceRecording { Title = filename, Date = DateTime.Now });
                }
                catch(Exception )
                {
                    MessageBox.Show("Sorry. Sometimes shit happens", "Error", MessageBoxButton.OK);
                }
                
            }
        }

        private async Task<string> WriteFile()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string FileName = reminder.Name + ".wav";
            StorageFile file = await localFolder.CreateFileAsync(
           FileName,
            CreationCollisionOption.GenerateUniqueName);
            using (Stream fileStream = await file.OpenStreamForWriteAsync())
            {
                using (var writer = new BinaryWriter(fileStream))
                {
                    writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                    writer.Write((Int32)(36 + audioStream.Length));
                    writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
                    writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                    writer.Write((Int32)16);
                    writer.Write((UInt16)1);
                    writer.Write((UInt16)1);
                    writer.Write((UInt32)16000);
                    writer.Write((UInt32)32000);
                    writer.Write((UInt16)2);
                    writer.Write((UInt16)16);
                    writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                    writer.Write((Int32)audioStream.Length);
                    writer.Write(audioStream.GetBuffer(), 0,
                    (int)audioStream.Length);
                    writer.Flush();
                }
            }
            return file.Name;
        }
        async void PlayFile(string filename)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            using (Stream fileStream = await
            localFolder.OpenStreamForReadAsync(filename))
            {
                var soundEffect = SoundEffect.FromStream(fileStream);
                audioPlayerInstance = soundEffect.CreateInstance();
                audioPlayerInstance.Play();
            }
        }

      
        private void StartTrackBtn_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            StartTrackBtn.IsEnabled = false;
            StopTrackBtn.IsEnabled = true;
            service = new Geolocator();
            service.DesiredAccuracy = PositionAccuracy.High;
            service.MovementThreshold = 1.0;
            service.PositionChanged += service_PositionChanged;
            if (maker == null)
            {
                MessageBox.Show("Choose a location", "Erro", MessageBoxButton.OK);
                NavigationService.GoBack();

            }
            var startPin = new Pushpin
            {
                GeoCoordinate = maker.GeoCoordinate,
                Content = "Start"
            };
            MapExtensions.GetChildren(mapControl).Add(startPin);
            
            mapControl.Pitch = 45.0;
            if (SetDistanceTbx.Text == "")
                Distance = 0;
            else
            {
                Distance = Convert.ToDouble(SetDistanceTbx.Text);    
            }
            SetDistanceTbx.IsEnabled = false;

        }

       

        private void service_PositionChanged(Windows.Devices.Geolocation.Geolocator sender, Windows.Devices.Geolocation.PositionChangedEventArgs args)
        {
            GeoCoordinate location = args.Position.Coordinate.ToGeoCoordinate();

            double d = location.GetDistanceTo(previous);
            if (location.GetDistanceTo(previous)<= Distance)
            {

                Alarm alarm = new Alarm("Distance Alarm");
                alarm.Content = "Here we are";

                alarm.BeginTime = DateTime.Now;

              

                ScheduledActionService.Add(alarm);
            }
        }

        private async  void CalcualteButton_Click(object sender, RoutedEventArgs e)
        {
            Geolocator locator = new Geolocator();
            Geoposition geoPosition = await locator.GetGeopositionAsync();
             GeoCoordinate location = new GeoCoordinate();
             location = geoPosition.Coordinate.ToGeoCoordinate();
             LabeledMapLocation start = null;
             LabeledMapLocation end = null;
            string startAddress = "";
            string endAddress = "";


            ReverseGeocodeQuery query = new ReverseGeocodeQuery();
            query.GeoCoordinate = location;
            IList<MapLocation> results1 = await query.GetMapLocationsAsync();
            MapLocation locationMap = results1.FirstOrDefault();
            if (locationMap != null)
            {
                startAddress += FormatAddress(locationMap.Information.Address);
            }

            query.GeoCoordinate = previous;
            IList<MapLocation> results2 = await query.GetMapLocationsAsync();
            MapLocation locationMap2 = results2.FirstOrDefault();
            if (locationMap2 != null)
            {
                endAddress += FormatAddress(locationMap2.Information.Address);
            }


            start = new LabeledMapLocation { Label = startAddress };
            end = new LabeledMapLocation{Label = endAddress};
            var task = new MapsDirectionsTask {Start = start, End = end};
            task.Show();
        }

        private void StopTrackBtn_Click(object sender, RoutedEventArgs e)
        {
            StartTrackBtn.IsEnabled = true;
            StopTrackBtn.IsEnabled = false;
            service.PositionChanged -= service_PositionChanged;
          
            service = null;
          
            mapControl.Pitch = 0;
            previous = new GeoCoordinate();
            SetDistanceTbx.IsEnabled = true;
        }

        private void MappDownload_Click(object sender, EventArgs e)
        {
            MapDownloaderTask task = new MapDownloaderTask();
            task.Show();
        }

      

    }
}
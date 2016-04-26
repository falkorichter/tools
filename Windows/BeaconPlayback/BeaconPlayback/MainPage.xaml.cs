using BeaconPlayback.Player;
using System;
using System.Collections.ObjectModel;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using BeaconPlayback;

namespace BeaconPlayback
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //private LocalAdvertiser _publisher;
        EventPlayer _player = null;

        private ObservableCollection<LogEntryItem> LogEntryItemCollection
        {
            get
            {
                return (ObservableCollection<LogEntryItem>)GetValue(LogEntryItemCollectionProperty);
            }
            set
            {
                SetValue(LogEntryItemCollectionProperty, value);
            }
        }

        public static readonly DependencyProperty LogEntryItemCollectionProperty =
            DependencyProperty.Register("LogEntryItemCollection", typeof(ObservableCollection<LogEntryItem>), typeof(MainPage),
                new PropertyMetadata(null));

        public MainPage()
        {
            this.InitializeComponent();
            LogEntryItemCollection = new ObservableCollection<LogEntryItem>();
        }

        private async void OnAdvertiserStateChanged(object sender, EventPlayer.State e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                if (e == EventPlayer.State.Initialized)
                {
                    Start.Label = "Start";
                    Start.IsEnabled = true;
                    Start.Icon = new SymbolIcon(Symbol.Play);
                }
                else if (e == EventPlayer.State.Playing)
                {
                    Start.Label = "Stop";
                    Start.IsEnabled = true;
                    Start.Icon = new SymbolIcon(Symbol.Stop);
                }
            });
        }

        private void OnMessage(object sender, string e)
        {
            AddLogEntry(e);
        }

        private async void AddLogEntry(string message)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => 
                {   
                    // Removes the last item from the log entries list
                    if (LogEntryItemCollection.Count > 20)
                    {
                        LogEntryItemCollection.RemoveAt(LogEntryItemCollection.Count - 1);
                    }
                    LogEntryItem logEntryItem = new LogEntryItem(message);
                    LogEntryItemCollection.Insert(0, logEntryItem);
                });
        }

        private async void OnOpen(object sender, RoutedEventArgs e)
        {
            _player?.Stop();

            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".csv");
            var file = await picker.PickSingleFileAsync();
            
            if (file == null)
            {
                return;
            }

            Source src = await PlayerEngine.InitializeSource(file);
            if(src != null)
            {
                if(src.Validate())
                {
                    _player = new EventPlayer(src);
                    _player.Message += OnMessage;
                    _player.StateChanged += OnAdvertiserStateChanged;
                    AddLogEntry("CSV file was processed succesfully.");
                }
            }

            if(_player == null)
            {
                AddLogEntry("Failed to process the CSV file.");
            }
        }

        private void OnStart(object sender, RoutedEventArgs e)
        {
            if(_player.AdvertiserState == EventPlayer.State.Initialized)
            {
                _player.Start();
                
            }
            else if (_player.AdvertiserState == EventPlayer.State.Playing)
            {
                _player.Stop();
                Start.Label = "Start";
            }
        }

        public class LogEntryItem
        {
            public string Timestamp
            {
                get;
                private set;
            }

            public string Message
            {
                get;
                set;
            }

            public LogEntryItem(string message)
            {
                Timestamp = string.Format("{0:H:mm:ss}", DateTime.Now);
                Message = message;
            }
        }

    }
}

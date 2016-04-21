using System;
using System.Collections.ObjectModel;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace BeaconPlayback
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Advertiser _publisher;

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
            _publisher = new Advertiser();
            _publisher.Message += OnMessage;
            _publisher.StateChanged += OnAdvertiserStateChanged;
            LogEntryItemCollection = new ObservableCollection<LogEntryItem>();
        }

        private async void OnAdvertiserStateChanged(object sender, Advertiser.State e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                if (e == Advertiser.State.Initialized)
                {
                    Start.Label = "Start";
                    Start.IsEnabled = true;
                    Start.Icon = new SymbolIcon(Symbol.Play);
                }
                else if (e == Advertiser.State.UnInitialized)
                {
                    Start.Label = "Start";
                    Start.IsEnabled = false;
                }
                else if (e == Advertiser.State.Advertising)
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
            _publisher.Clear();

            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".csv");
            var file = await picker.PickSingleFileAsync();
            
            if (file == null)
            {
                return;
            }

            var readFile = await Windows.Storage.FileIO.ReadLinesAsync(file);

            foreach (var line in readFile)
            {
                var items = line.Split(',');

                if(items[0] == "HEADER")
                {
                    UInt32 replies = 0;
                    UInt32.TryParse(items[1], out replies);
                    _publisher.AddHeader(replies);

                }
                else if (items[0] == "EVENT")
                {
                    UInt32 duration = 20;
                    uint result;
                    if (UInt32.TryParse(items[4], out result))
                    {
                        duration = result;
                    }

                    UInt32 sleep = 20;
                    if (UInt32.TryParse(items[5], out result))
                    {
                        sleep = result;
                    }

                    _publisher.AddEvent(items[1].Trim(), items[2].Trim(), items[3].Trim(), duration, sleep);

                }
            }

            if(_publisher.Validate())
            {
                AddLogEntry("CSV file was processed succesfully. \nThe total number of events loaded: " + _publisher.EventCount);
            } else
            {
                AddLogEntry("Failed to process the CSV file.");
            }
        }

        private void OnStart(object sender, RoutedEventArgs e)
        {
            if(_publisher.AdvertiserState == Advertiser.State.Initialized)
            {
                _publisher.Start();
                
            }
            else if (_publisher.AdvertiserState == Advertiser.State.Advertising)
            {
                _publisher.Stop();
                Start.Label = "Start";
            }
            else if (_publisher.AdvertiserState == Advertiser.State.UnInitialized)
            {
                AddLogEntry("Load events from the CSV file first. \nEvents in the file needs to be in the following format:\nID1, ID2, ID3, duration, sleep time");
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

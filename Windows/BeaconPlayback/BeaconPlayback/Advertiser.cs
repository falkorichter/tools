using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace BeaconPlayback
{
    class Advertiser
    {
        public enum State { UnInitialized, Initialized, Advertising };
        public event EventHandler<string> Message;
        public event EventHandler<State> StateChanged;
        
        private const UInt16 ManufacturerId = 0x004c;
        private const UInt16 BeaconCode = 0x0215;
        private Timer _timer;
        private List<Event> _events = new List<Event>();
        private BluetoothLEAdvertisementPublisher _bluetoothLEAdvertisementPublisher;
        private int _nextToAdvertise = 0;
        private State _state;
        private uint _replies = 0;
        private uint _repliesConsumed = 0;
        private uint _totalNumberofEvents = 0;

        public Advertiser()
        {
            AdvertiserState = State.UnInitialized;
        }

        public State AdvertiserState
        {
            get
            {
                return _state;
            }
            set
            {
                if(_state == value)
                {
                    return;
                }
                _state = value;
                StateChanged?.Invoke(this, _state);
            }
        }

        public int EventCount
        {
            get
            {
                return _events.Count;
            }
        }

        public void Start()
        {
            _nextToAdvertise = 0;
            _repliesConsumed = 0;
            _totalNumberofEvents = 0;
            AdvertiserState = State.Advertising;
            AdvertiseNext();
        }

        public void Stop()
        {
            if(AdvertiserState == State.Advertising)
            {
                Message?.Invoke(this, "Finished. Total number of events: " + _totalNumberofEvents);
            }

            AdvertiserState = State.Initialized;

            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            if (_bluetoothLEAdvertisementPublisher != null)
            {
                _bluetoothLEAdvertisementPublisher.Stop();
                _bluetoothLEAdvertisementPublisher = null;
            }

            
        }

        public void Clear()
        {
            Stop();
            _events.Clear();
            AdvertiserState = State.UnInitialized;
        }

        public bool AddEvent(string ID1, string ID2, string ID3, UInt32 duration, UInt32 sleep)
        {
            Beacon beacon = new Beacon();
            beacon.ManufacturerId = ManufacturerId;
            beacon.Code = BeaconCode;
            beacon.Id1 = ID1;

            try
            {
                beacon.Id2 = UInt16.Parse(ID2);
                beacon.Id3 = UInt16.Parse(ID3);
            }
            catch (Exception)
            {
                return false;
            }

            beacon.MeasuredPower = -58;
            Event e = new Event() { Duration = duration, Beacon = beacon, Sleep = sleep };
            _events.Add(e);
            
            return true;
        }

        public void AddHeader(uint replies)
        {
            _replies = replies;
        }
        public bool Validate()
        {
            if(_events.Count > 0)
            {
                AdvertiserState = State.Initialized;
                return true;
            }
            return false;
        }

        private void AdvertiseNext()
        {
            var ev = _events[_nextToAdvertise];
            _bluetoothLEAdvertisementPublisher = new BluetoothLEAdvertisementPublisher();
            BluetoothLEAdvertisementDataSection dataSection = BeaconFactory.BeaconToSecondDataSection(ev.Beacon);
            _bluetoothLEAdvertisementPublisher.Advertisement.DataSections.Add(dataSection);

            try
            {
                _bluetoothLEAdvertisementPublisher.Start();
                string msg = "New event:\n" +
                    ev.Beacon.Id1 + " " + ev.Beacon.Id2 + " " + ev.Beacon.Id3 +
                    "\nDuration: " + ev.Duration + ", Sleep: " + ev.Sleep;

                Message?.Invoke(this,msg);
            }
            catch (Exception ex)
            {
                _bluetoothLEAdvertisementPublisher = null;
            }
            _totalNumberofEvents++;
            var timespan = TimeSpan.FromSeconds(ev.Duration).TotalMilliseconds;
            _timer = new Timer(advertisementOver, null, (int)timespan, Timeout.Infinite);
        }

        private void advertisementOver(object state)
        {
            _timer.Dispose();

            if (_bluetoothLEAdvertisementPublisher != null) {
                _bluetoothLEAdvertisementPublisher.Stop();
                _bluetoothLEAdvertisementPublisher = null;
            }

            var ev = _events[_nextToAdvertise];
            var timespan = TimeSpan.FromSeconds(ev.Sleep).TotalMilliseconds;
            _timer = new Timer(readyToAdvertiseNext, null, (int)timespan, Timeout.Infinite);
        }

        private void readyToAdvertiseNext(object state)
        {
            _timer.Dispose();

            if(_nextToAdvertise + 1 < _events.Count)
            {
                _nextToAdvertise = _nextToAdvertise + 1;
                AdvertiseNext();
                return;
            }

            _nextToAdvertise = 0;
            if (_repliesConsumed + 1 >= _replies && _replies != 0)
            {
                AdvertiserState = State.Initialized;
                Message?.Invoke(this, "Finished. Total number of events: " + _totalNumberofEvents);
            }
            else
            {
                _repliesConsumed++;
                if(_replies == 0)
                {
                    Message?.Invoke(this, "Reply round: " + (_repliesConsumed+1));
                }
                else
                {
                    Message?.Invoke(this, "Reply round: " + (_repliesConsumed+1) + " / " + _replies + ".");
                }
                
                AdvertiseNext();
            }
        }
    }
}

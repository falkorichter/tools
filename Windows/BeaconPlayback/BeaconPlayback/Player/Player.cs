using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeaconPlayback.Player
{
    class EventPlayer
    {
        public enum State { Initialized, Playing };
        public event EventHandler<string> Message;
        public event EventHandler<State> StateChanged;

        private uint _totalNumberofEvents = 0;
        private Advertise _advertise;
        private Source _source;
        private State _state;

        public EventPlayer(Source source)
        {
            _source = source;
            AdvertiserState = State.Initialized;
            _source.SourceMessage += OnSourceMessage;
        }

        public State AdvertiserState
        {
            get
            {
                return _state;
            }
            set
            {
                if (_state == value)
                {
                    return;
                }
                _state = value;
                StateChanged?.Invoke(this, _state);
            }
        }


        public async Task Start()
        {
            Event ev = await _source.getCurrentAsync();
            Advertise(ev);
            AdvertiserState = State.Playing;
        }

        public void Stop()
        {
            if (AdvertiserState == State.Playing)
            {
                Message?.Invoke(this, "Finished. Total number of events: ");
            }

            AdvertiserState = State.Initialized;
            _advertise?.Stop();
        }

        private void Advertise(Event ev)
        {
            _advertise = new Advertise(ev);
            _advertise.Finished += OnAdvertisingFinished;
            _advertise.Start();

            string msg = "New event:\n" +
                   ev.Beacon.Id1 + " " + ev.Beacon.Id2 + " " + ev.Beacon.Id3 +
                   "\nDuration: " + ev.Duration + ", Sleep: " + ev.Sleep;

            Message?.Invoke(this, msg);

            _totalNumberofEvents++;
        }

        private async void OnAdvertisingFinished(object sender, bool e)
        {
            var ev = await _source.getNextAsync();
            Advertise(ev);
        }
        private void OnSourceMessage(object sender, string e)
        {
            Message?.Invoke(this, e);
        }
    }
}

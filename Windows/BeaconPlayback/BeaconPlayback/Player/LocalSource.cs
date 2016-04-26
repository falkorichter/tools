using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeaconPlayback.Player
{
    class LocalEventSource : Source
    {

        private List<Event> _events = new List<Event>();

        private int _nextToAdvertise = 0;
        private uint _replies = 0;
        private uint _repliesConsumed = 0;

        public int EventCount
        {
            get
            {
                return _events.Count;
            }
        }

        public LocalEventSource(uint replies)
        {
            _replies = replies;
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

        public override bool Validate()
        {
            return _events.Count > 0 ? true : false;
        }

        public override async Task<Event> getCurrentAsync()
        {
            return _events[_nextToAdvertise];
        }


        public override async Task<Event> getNextAsync()
        {
            if (_nextToAdvertise + 1 < _events.Count)
            {
                _nextToAdvertise = _nextToAdvertise + 1;
                return _events[_nextToAdvertise];
            }

            _nextToAdvertise = 0;
            if (_repliesConsumed + 1 >= _replies && _replies != 0)
            {
                return null;
            }
            else
            {
                _repliesConsumed++;
                if (_replies == 0)
                {
                    OnMessage("Reply round: " + (_repliesConsumed + 1));
                }
                else
                {
                    OnMessage("Reply round: " + (_repliesConsumed + 1) + " / " + _replies + ".");
                }
                return _events[_nextToAdvertise];
            }
        }
    }
}

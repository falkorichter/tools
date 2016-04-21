using System;

namespace BeaconPlayback
{
    class Event
    {
        public UInt32 Duration
        {
            get;
            set;
        }

        public UInt32 Sleep
        {
            get;
            set;
        }

        public Beacon Beacon
        {
            get;
            set;
        }
    }
}

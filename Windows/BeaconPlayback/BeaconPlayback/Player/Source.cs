using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeaconPlayback.Player
{
    public class Source
    {
        public event EventHandler<string> SourceMessage;

        protected const UInt16 ManufacturerId = 0x004c;
        protected const UInt16 BeaconCode = 0x0215;

        public virtual async Task<Event> getNextAsync()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<Event> getCurrentAsync()
        {
            throw new NotImplementedException();
        }

        public virtual void Stop()
        {
            throw new NotImplementedException();
        }

        public virtual bool Validate()
        {
            throw new NotImplementedException();
        }
        protected virtual void OnMessage(string message)
        {
            SourceMessage?.Invoke(this, message);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace BeaconPlayback.Player
{
    public class PlayerEngine
    {

        public static async Task<Source> InitializeSource(StorageFile file)
        {
            var fileLines = await Windows.Storage.FileIO.ReadLinesAsync(file);
            var firstLine = fileLines[0];
            var firstLineItems = firstLine.Split(',');

            if (firstLineItems[0] == "LOCAL")
            {
                return InitializeLocal(fileLines);   
            }
            else if(firstLineItems[0] == "REMOTE")
            {
                return null;
            }

            return null;

        }

        private static Source InitializeLocal(IList<string> fileLines)
        {
            var firstLine = fileLines[0];
            var firstLineItems = firstLine.Split(',');
            UInt32 replies = 0;
            UInt32.TryParse(firstLineItems[1], out replies);

            LocalEventSource localSrc = new LocalEventSource(replies);

            foreach (var line in fileLines)
            {
                var items = line.Split(',');

                if(items[0] == "EVENT")
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

                    localSrc.AddEvent(items[1].Trim(), items[2].Trim(), items[3].Trim(), duration, sleep);
                }
            }

            return localSrc;
        }
    }
}

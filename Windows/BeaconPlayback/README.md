# BeaconPlayback for for Windows 10 #
This tool can be used to emit a series of beacon events for a certain number of times. The main purpose of the tool is to allow the user to test any beacon receiver application for a longer time periods and with a longer beacon event sequences. 

Beacon, duration and a delay before the next event are specified in the CSV file. Also the number how many times the sequence of events is replied is defined in the csv. Sample csv file (tests.csv) is included into the project.

### Header line format and a sample ###

| TYPE | replies |
| ---- | ------- |
| HEADER | 1 |

If the replies count is set to 0, then the sequence of the events will be replied infinitely.

### Event line format and a sample ###

| TYPE | ID1 | ID2 | ID3 | Duration | Delay |
| ---- | --- | --- | --- | -------- | ----- |
| EVENT | 73676723-7400-0000-ffff-0000ffff0007 | 24888 | 23777 | 5 | 1 |

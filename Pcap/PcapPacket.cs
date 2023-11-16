﻿using PcapngUtils.Common;


namespace PcapngUtils.Pcap
{
    [ToString]
    public class PcapPacket:IPacket
    {
        /// <summary>
        /// ts_sec: the date and time when this packet was captured. This value is in seconds since January 1, 1970 00:00:00 GMT; 
        /// this is also known as a UN*X time_t. You can use the ANSI C time() function from time.h to get this value, but you might use 
        /// a more optimized way to get this timestamp value. If this timestamp isn't based on GMT (UTC), use thiszone from the global header 
        /// for adjustments.
        /// </summary>
        public ulong Seconds
        {
            get;
            set;
        }

        /// <summary>
        /// ts_usec: in regular pcap files, the microseconds when this packet was captured, as an offset to ts_sec. 
        /// In nanosecond-resolution files, this is, instead, the nanoseconds when the packet was captured, as an offset to ts_sec /!\ 
        /// Beware: this value shouldn't reach 1 second (in regular pcap files 1 000 000; in nanosecond-resolution files, 1 000 000 000); 
        /// in this case ts_sec must be increased instead!
        /// </summary>
        public ulong Microseconds
        {
            get;
            set;
        }

        /// <summary>
        /// packet data 
        /// </summary>
        [IgnoreDuringToString]
        public byte[] Data
        {
            get;
            set;
        }

        /// <summary>
        /// packet position in the stream  (set when reading from the stream. )
        /// </summary>
        public long PositionInStream 
        { 
            get; 
            set;
        }
        public PcapPacket(ulong secs, ulong usecs, byte[] data,long positionInStream)
        {
            Seconds = secs;
            Microseconds = usecs;
            Data = data;
            PositionInStream = positionInStream;
        } 
    }
}

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using PcapngUtils.Extensions;

namespace PcapngUtils.PcapNG.CommonTypes
{
    [ToString]    
    public sealed class TimestampHelper
    {
        #region nUnitTest
        [TestFixture]
        private static class TimestampHelper_Test
        {
            [Test]
            public static void TimestampHelper_Simple_Test()
            {
                byte[]? testData = { 1, 0, 0, 0, 1, 0, 0, 0 };
                var TimestampHight = BitConverter.ToUInt32(testData, 0);
                var timestamp = new TimestampHelper(testData, false);
                Assert.AreEqual(timestamp.TimestampHight, 1);
                Assert.AreEqual(timestamp.TimestampLow, 1);
                Assert.AreEqual(timestamp.Seconds, 4294);
                Assert.AreEqual(timestamp.Microseconds, 967297);
            }

            [TestCase(true)]
            [TestCase(false)]
            public static void TimestampHelper_ConvertToByte_Test(bool reorder)
            {
                byte[]? preData = { 1, 0, 0, 0, 1, 0, 0, 0 };

                var preTimestamp = new TimestampHelper(preData, false);
                var postData = preTimestamp.ConvertToByte(reorder);
                var postTimestamp = new TimestampHelper(postData, reorder);
                Assert.AreEqual(preTimestamp, postTimestamp);
            }
        }
        #endregion

        #region fields && properties
        private static readonly DateTime epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        [IgnoreDuringToString]
        public uint TimestampHight
        {
            get;
            private set;
        }

        [IgnoreDuringToString]
        public uint TimestampLow
        {
            get;
            private set;
        }
        public ulong Seconds
        {
            get;
            private set;
        }

        public ulong Microseconds
        {
            get;
            private set;
        }

        #endregion

        #region ctor
        public TimestampHelper(IReadOnlyCollection<byte> timestampAsByte,bool reverseByteOrder)
        {
            if (timestampAsByte.Count != 8) throw new ArgumentException("timestamp must have length = 8");

            TimestampHight = (BitConverter.ToUInt32(timestampAsByte.Take(4).ToArray(), 0)).ReverseByteOrder(reverseByteOrder);
            TimestampLow = (BitConverter.ToUInt32(timestampAsByte.Skip(4).Take(4).ToArray(), 0)).ReverseByteOrder(reverseByteOrder);            

            var timestamp = ((TimestampHight * 4294967296) + TimestampLow);
            Seconds = (ulong)(timestamp / 1000000);
            Microseconds = (ulong)(timestamp % 1000000);
        }

        public TimestampHelper(ulong Seconds, ulong Microseconds)
        {
            this.Seconds = Seconds;
            this.Microseconds = Microseconds;

            var timestamp = Seconds * 1000000 + Microseconds;
            TimestampHight = (uint)(timestamp / 4294967296);
            TimestampLow = (uint)(timestamp % 4294967296);            
        }
        #endregion

        #region method
        public DateTime ToUtc()
        {
            var ticks = (Microseconds * (TimeSpan.TicksPerMillisecond / 1000)) +
                        (Seconds * TimeSpan.TicksPerSecond);
            return epochDateTime.AddTicks((long)ticks);
        }

        public byte[] ConvertToByte(bool reverseByteOrder)
        {
            var timestamp = Seconds * 1000000 + Microseconds;
            var TimestampHight =(uint)(timestamp / 4294967296);
            var TimestampLow = (uint)(timestamp % 4294967296);           

            var ret = new List<byte>();
            ret.AddRange(BitConverter.GetBytes(TimestampHight.ReverseByteOrder(reverseByteOrder)));
            ret.AddRange(BitConverter.GetBytes(TimestampLow.ReverseByteOrder(reverseByteOrder)));

            return ret.ToArray();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var p = (TimestampHelper)obj;
            return (Seconds == p.Seconds) && (Microseconds == p.Microseconds);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}

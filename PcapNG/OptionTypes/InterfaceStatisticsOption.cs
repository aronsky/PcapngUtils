﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PcapngUtils.Extensions;
using PcapngUtils.PcapNG.CommonTypes;
using System.IO;
using System.Diagnostics.Contracts;
using NUnit.Framework;

namespace PcapngUtils.PcapNG.OptionTypes
{
    [ToString]          
    public sealed class InterfaceStatisticsOption : AbstractOption
    {
        #region nUnitTest
        [TestFixture]
        private static class InterfaceStatisticsOption_Test
        {
            [TestCase(true)]
            [TestCase(false)]
            [ContractVerification(false)]
            public static void InterfaceStatisticsOption_ConvertToByte_Test(bool reorder)
            {
                var preOption = new InterfaceStatisticsOption();
                InterfaceStatisticsOption postOption;
                preOption.Comment = "Test Comment";
                preOption.DeliveredToUser = 25;
                preOption.EndTime = new TimestampHelper(new byte[] { 1, 0, 0, 0, 2, 0, 0, 0 }, false);
                preOption.StartTime = new TimestampHelper(new byte[] { 1, 0, 0, 3, 2, 0, 0, 4 }, false);
                preOption.FilterAccept = 30;
                preOption.InterfaceDrop = 35;
                preOption.InterfaceReceived = 40;
                preOption.SystemDrop = 45;

                var preOptionByte = preOption.ConvertToByte(reorder, null);
                using (var stream = new MemoryStream(preOptionByte))
                {
                    using (var binaryReader = new BinaryReader(stream))
                    {
                        postOption = Parse(binaryReader, reorder, null);
                    }
                }

                Assert.IsNotNull(postOption);
                Assert.AreEqual(preOption.Comment, postOption.Comment);
                Assert.AreEqual(preOption.DeliveredToUser, postOption.DeliveredToUser);
                Assert.AreEqual(preOption.EndTime.Seconds, postOption.EndTime?.Seconds);
                Assert.AreEqual(preOption.EndTime.Microseconds, postOption.EndTime?.Microseconds);
                Assert.AreEqual(preOption.StartTime.Seconds, postOption.StartTime?.Seconds);
                Assert.AreEqual(preOption.StartTime.Microseconds, postOption.StartTime?.Microseconds);
                Assert.AreEqual(preOption.FilterAccept, postOption.FilterAccept);
                Assert.AreEqual(preOption.InterfaceDrop, postOption.InterfaceDrop);
                Assert.AreEqual(preOption.InterfaceReceived, postOption.InterfaceReceived);
                Assert.AreEqual(preOption.SystemDrop, postOption.SystemDrop);
            }
        }
        #endregion

        #region enum
        public enum InterfaceStatisticsOptionCode : ushort
        {
            EndOfOptionsCode = 0,
            CommentCode = 1,
            StartTimeCode = 2,
            EndTimeCode = 3,
            InterfaceReceivedCode = 4,
            InterfaceDropCode = 5,
            FilterAcceptCode = 6,
            SystemDropCode = 7,
            DeliveredToUserCode = 8
        };
        #endregion

        #region fields & properies
        /// <summary>
        /// A UTF-8 string containing a comment that is associated to the current block.
        /// </summary>
        public string? Comment
        {
            get;
            private set;
        }

        /// <summary>
        /// Time in which the capture started; time will be stored in two blocks of four bytes each. 
        /// Timestamp (High) and Timestamp (Low): high and low 32-bits of a 64-bit quantity representing the timestamp. The timestamp is a 
        /// single 64-bit unsigned integer representing the number of units since 1/1/1970. The way to interpret this field is specified by the 
        /// 'if_tsresol' option (see Figure 9) of the Interface Description block referenced by this packet. Please note that differently 
        /// from the libpcap file format, timestamps are not saved as two 32-bit values accounting for the seconds and microseconds since 
        /// 1/1/1970. They are saved as a single 64-bit quantity saved as two 32-bit words.
        /// </summary>
        public TimestampHelper? StartTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Time in which the capture ended; ; time will be stored in two blocks of four bytes each
        /// Timestamp (High) and Timestamp (Low): high and low 32-bits of a 64-bit quantity representing the timestamp. The timestamp is a 
        /// single 64-bit unsigned integer representing the number of units since 1/1/1970. The way to interpret this field is specified by the 
        /// 'if_tsresol' option (see Figure 9) of the Interface Description block referenced by this packet. Please note that differently 
        /// from the libpcap file format, timestamps are not saved as two 32-bit values accounting for the seconds and microseconds since 
        /// 1/1/1970. They are saved as a single 64-bit quantity saved as two 32-bit words.
        /// </summary>
        public TimestampHelper? EndTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Number of packets received from the physical interface starting from the beginning of the capture.
        /// </summary>
        public long? InterfaceReceived
        {
            get;
            private set;
        }

        /// <summary>
        /// Number of packets dropped by the interface due to lack of resources starting from the beginning of the capture.
        /// </summary>
        public long? InterfaceDrop
        {
            get;
            private set;
        }

        /// <summary>
        /// Number of packets accepted by filter starting from the beginning of the capture.
        /// </summary>
        public long? FilterAccept
        {
            get;
            private set;
        }

        /// <summary>
        /// Number of packets dropped by the operating system starting from the beginning of the capture.
        /// </summary>
        public long? SystemDrop
        {
            get;
            private set;
        }
          
        /// <summary>
        /// Number of packets delivered to the user starting from the beginning of the capture. The value contained in this field can be different
        /// from the value 'isb_filteraccept - isb_osdrop' because some packets could still lay in the OS buffers when the capture ended.
        /// </summary>
        public long? DeliveredToUser
        {
            get;
            private set;
        }
        #endregion

        #region ctor
        public InterfaceStatisticsOption(string? Comment = null, TimestampHelper? StartTime = null, TimestampHelper? EndTime = null, long? InterfaceReceived = null,
            long? InterfaceDrop = null, long? FilterAccept = null, long? SystemDrop = null, long? DeliveredToUser =null) 
        {
            this.Comment = Comment;
            this.StartTime = StartTime;
            this.EndTime = EndTime;
            this.InterfaceReceived = InterfaceReceived;
            this.InterfaceDrop = InterfaceDrop;
            this.FilterAccept = FilterAccept;
            this.SystemDrop = SystemDrop;
            this.DeliveredToUser = DeliveredToUser;
        }
        #endregion

        #region method
        public static InterfaceStatisticsOption Parse(BinaryReader binaryReader, bool reverseByteOrder, Action<Exception>? ActionOnException)
        {
            var option = new InterfaceStatisticsOption();
            var optionsList = ExtractOptions(binaryReader, reverseByteOrder, ActionOnException);

            if (!optionsList.Any()) return option;
            foreach (var item in optionsList)
            {
                try
                {
                    switch (item.Key)
                    {
                        case (ushort)InterfaceStatisticsOptionCode.CommentCode:
                            option.Comment = Encoding.UTF8.GetString(item.Value);
                            break;
                        case (ushort)InterfaceStatisticsOptionCode.StartTimeCode:
                            if (item.Value.Length == 8)
                                option.StartTime = new TimestampHelper(item.Value, reverseByteOrder);
                            else
                                throw new ArgumentException(
                                    $"[InterfaceStatisticsOption.Parse] StartTimeCode contains invalid length. Received: {item.Value.Length} bytes, expected: {8}");
                            break;
                        case (ushort)InterfaceStatisticsOptionCode.EndTimeCode:
                            if (item.Value.Length == 8)
                                option.EndTime = new TimestampHelper(item.Value, reverseByteOrder);
                            else
                                throw new ArgumentException(
                                    $"[InterfaceStatisticsOption.Parse] EndTimeCode contains invalid length. Received: {item.Value.Length} bytes, expected: {8}");
                            break;
                        case (ushort)InterfaceStatisticsOptionCode.InterfaceReceivedCode:                                
                            if (item.Value.Length == 8)
                                option.InterfaceReceived = (BitConverter.ToInt64(item.Value, 0)).ReverseByteOrder(reverseByteOrder);
                            else
                                throw new ArgumentException(
                                    $"[InterfaceStatisticsOption.Parse] InterfaceReceivedCode contains invalid length. Received: {item.Value.Length} bytes, expected: {8}");
                            break;
                        case (ushort)InterfaceStatisticsOptionCode.InterfaceDropCode:                               
                            if (item.Value.Length == 8)
                                option.InterfaceDrop = (BitConverter.ToInt64(item.Value, 0)).ReverseByteOrder(reverseByteOrder);
                            else
                                throw new ArgumentException(
                                    $"[InterfaceStatisticsOption.Parse] InterfaceDropCode contains invalid length. Received: {item.Value.Length} bytes, expected: {8}");
                            break;
                        case (ushort)InterfaceStatisticsOptionCode.FilterAcceptCode:                               
                            if (item.Value.Length == 8)
                                option.FilterAccept = (BitConverter.ToInt64(item.Value, 0)).ReverseByteOrder(reverseByteOrder);
                            else
                                throw new ArgumentException(
                                    $"[InterfaceStatisticsOption.Parse] FilterAcceptCode contains invalid length. Received: {item.Value.Length} bytes, expected: {8}");
                            break;
                        case (ushort)InterfaceStatisticsOptionCode.SystemDropCode:                               
                            if (item.Value.Length == 8)
                                option.SystemDrop = (BitConverter.ToInt64(item.Value, 0)).ReverseByteOrder(reverseByteOrder);
                            else
                                throw new ArgumentException(
                                    $"[InterfaceStatisticsOption.Parse] SystemDropCode contains invalid length. Received: {item.Value.Length} bytes, expected: {8}");
                            break;
                        case (ushort)InterfaceStatisticsOptionCode.DeliveredToUserCode:                                
                            if (item.Value.Length == 8)
                                option.DeliveredToUser = (BitConverter.ToInt64(item.Value, 0)).ReverseByteOrder(reverseByteOrder);
                            else
                                throw new ArgumentException(
                                    $"[InterfaceStatisticsOption.Parse] DeliveredToUserCode contains invalid length. Received: {item.Value.Length} bytes, expected: {8}");
                            break;
                        case (ushort)InterfaceStatisticsOptionCode.EndOfOptionsCode:
                        default:
                            break;
                    }
                }
                catch (Exception exc)
                {
                    if (ActionOnException != null)
                        ActionOnException(exc);
                }
            }

            return option;
        }

        public override byte[] ConvertToByte(bool reverseByteOrder, Action<Exception>? ActionOnException)
        {    
            var ret = new List<byte>();

            if (Comment != null)
            {
                var comentValue = Encoding.UTF8.GetBytes(Comment);
                if (comentValue.Length <= ushort.MaxValue)
                    ret.AddRange(ConvertOptionFieldToByte((ushort)InterfaceStatisticsOptionCode.CommentCode, comentValue, reverseByteOrder, ActionOnException));
            }

            if (StartTime != null)
            { 
                ret.AddRange(ConvertOptionFieldToByte((ushort)InterfaceStatisticsOptionCode.StartTimeCode, StartTime.ConvertToByte(reverseByteOrder), reverseByteOrder, ActionOnException));
            }

            if (EndTime != null)
            {
                ret.AddRange(ConvertOptionFieldToByte((ushort)InterfaceStatisticsOptionCode.EndTimeCode, EndTime.ConvertToByte(reverseByteOrder), reverseByteOrder, ActionOnException));
            }

            if (InterfaceReceived.HasValue)
            {
                var receivedCountValue = BitConverter.GetBytes(InterfaceReceived.Value.ReverseByteOrder(reverseByteOrder));
                ret.AddRange(ConvertOptionFieldToByte((ushort)InterfaceStatisticsOptionCode.InterfaceReceivedCode, receivedCountValue, reverseByteOrder, ActionOnException));
            }

            if (InterfaceDrop.HasValue)
            {
                var dropCountValue = BitConverter.GetBytes(InterfaceDrop.Value.ReverseByteOrder(reverseByteOrder));
                ret.AddRange(ConvertOptionFieldToByte((ushort)InterfaceStatisticsOptionCode.InterfaceDropCode, dropCountValue, reverseByteOrder, ActionOnException));
            }

            if (FilterAccept.HasValue)
            {
                var filterValue = BitConverter.GetBytes(FilterAccept.Value.ReverseByteOrder(reverseByteOrder));
                ret.AddRange(ConvertOptionFieldToByte((ushort)InterfaceStatisticsOptionCode.FilterAcceptCode, filterValue, reverseByteOrder, ActionOnException));
            }

            if (SystemDrop.HasValue)
            {
                var systemDropValue = BitConverter.GetBytes(SystemDrop.Value.ReverseByteOrder(reverseByteOrder));
                ret.AddRange(ConvertOptionFieldToByte((ushort)InterfaceStatisticsOptionCode.SystemDropCode, systemDropValue, reverseByteOrder, ActionOnException));
            }

            if (DeliveredToUser.HasValue)
            {
                var deliverValue = BitConverter.GetBytes(DeliveredToUser.Value.ReverseByteOrder(reverseByteOrder));
                ret.AddRange(ConvertOptionFieldToByte((ushort)InterfaceStatisticsOptionCode.DeliveredToUserCode, deliverValue, reverseByteOrder, ActionOnException));
            }

            ret.AddRange(ConvertOptionFieldToByte((ushort)InterfaceStatisticsOptionCode.EndOfOptionsCode, Array.Empty<byte>(), reverseByteOrder, ActionOnException));
            return ret.ToArray();
        }
        #endregion
    }
}

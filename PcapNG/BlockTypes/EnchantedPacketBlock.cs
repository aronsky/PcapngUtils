﻿using System;
using System.Collections.Generic;
using System.IO;
using PcapngUtils.PcapNG.CommonTypes;
using PcapngUtils.Extensions;
using PcapngUtils.PcapNG.OptionTypes;
using NUnit.Framework;
using PcapngUtils.Common;
namespace PcapngUtils.PcapNG.BlockTypes
{
    [ToString]     
    public sealed class EnchantedPacketBlock : AbstractBlock, IPacket
    {
        #region nUnitTest
        [TestFixture]
        private static class EnchantedPacketBlock_Test
        {
            [TestCase(true)]
            [TestCase(false)]
            public static void EnchantedPacketBlock_ConvertToByte_Test(bool reorder)
            {
                EnchantedPacketBlock? prePacketBlock;
                byte[] byteblock = { 6, 0, 0, 0, 132, 0, 0, 0, 0, 0, 0, 0, 12, 191, 4, 0, 118, 176, 176, 8, 98, 0, 0, 0, 98, 0, 0, 0, 0, 0, 94, 0, 1, 177, 0, 33, 40, 5, 41, 186, 8, 0, 69, 0, 0, 84, 48, 167, 0, 0, 255, 1, 3, 72, 192, 168, 177, 160, 10, 64, 11, 49, 8, 0, 10, 251, 67, 168, 0, 0, 79, 161, 27, 41, 0, 2, 83, 141, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 0, 0, 132, 0, 0, 0 };
                using (var stream = new MemoryStream(byteblock))
                {
                    using (var binaryReader = new BinaryReader(stream))
                    {
                        var block = AbstractBlockFactory.ReadNextBlock(binaryReader, false, null);
                        Assert.IsNotNull(block);
                        prePacketBlock = block as EnchantedPacketBlock;
                        Assert.IsNotNull(prePacketBlock);
                        byteblock = prePacketBlock!.ConvertToByte(reorder, null);
                    }
                }
                using (var stream = new MemoryStream(byteblock))
                {
                    using (var binaryReader = new BinaryReader(stream))
                    {
                        var block = AbstractBlockFactory.ReadNextBlock(binaryReader, reorder, null);
                        Assert.IsNotNull(block);
                        var postPacketBlock = block as EnchantedPacketBlock;
                        Assert.IsNotNull(postPacketBlock);

                        Assert.AreEqual(prePacketBlock.BlockType, postPacketBlock!.BlockType);
                        Assert.AreEqual(prePacketBlock.Data, postPacketBlock.Data);
                        Assert.AreEqual(prePacketBlock.InterfaceID, postPacketBlock.InterfaceID);
                        Assert.AreEqual(prePacketBlock.Microseconds, postPacketBlock.Microseconds);
                        Assert.AreEqual(prePacketBlock.PacketLength, postPacketBlock.PacketLength);
                        Assert.AreEqual(prePacketBlock.PositionInStream, postPacketBlock.PositionInStream);
                        Assert.AreEqual(prePacketBlock.Seconds, postPacketBlock.Seconds);
                        Assert.AreEqual(prePacketBlock.Options.Comment, postPacketBlock.Options.Comment);
                        Assert.AreEqual(prePacketBlock.Options.DropCount, postPacketBlock.Options.DropCount);
                        Assert.AreEqual(prePacketBlock.Options.Hash, postPacketBlock.Options.Hash);
                        Assert.AreEqual(prePacketBlock.Options.PacketFlag, postPacketBlock.Options.PacketFlag);
                    }
                }
            }
        }
        #endregion

        #region IPacket
        public ulong Seconds
        {
            get { return Timestamp.Seconds; }
        }

        public ulong Microseconds
        {
            get { return Timestamp.Microseconds; }
        }
        #endregion

        #region Properties
        /// <summary>
        /// The block type
        /// </summary>
        public override BaseBlock.Types BlockType
        {
            get { return BaseBlock.Types.EnhancedPacket; }
        }

        /// <summary>
        /// Interface ID: it specifies the interface this packet comes from; the correct interface will be the one whose Interface Description 
        /// Block (within the current Section of the file) is identified by the same number (see Section 3.2) of this field.
        /// </summary>
        public int InterfaceID
        {
            get;
            set;
        }

        /// <summary>
        /// contains information about relations between packet and interface  on which it was captured 
        /// </summary>
        public override int? AssociatedInterfaceID
        {
            get { return InterfaceID; }
        }

        /// <summary>
        /// Timestamp (High) and Timestamp (Low): high and low 32-bits of a 64-bit quantity representing the timestamp. The timestamp is a 
        /// single 64-bit unsigned integer representing the number of units since 1/1/1970. The way to interpret this field is specified by the 
        /// 'if_tsresol' option (see Figure 9) of the Interface Description block referenced by this packet. Please note that differently 
        /// from the libpcap file format, timestamps are not saved as two 32-bit values accounting for the seconds and microseconds since 
        /// 1/1/1970. They are saved as a single 64-bit quantity saved as two 32-bit words.
        /// </summary>
        public TimestampHelper Timestamp
        {
            get;
            set;
        }  

        /// <summary>
        /// Packet Len: actual length of the packet when it was transmitted on the network. It can be different from Captured Len if the user 
        /// wants only a snapshot of the packet.
        /// </summary>
        public int PacketLength
        {
            get;
            set;
        }

        /// <summary>
        /// Captured Len: number of bytes captured from the packet (i.e. the length of the Packet Data field). It will be the minimum value  
        /// among the actual Packet Length and the snapshot length (defined in Figure 9). The value of this field does not include the padding 
        /// bytes added at the end of the Packet Data field to align the Packet Data Field to a 32-bit boundary
        /// </summary>
        public int CaptureLength
        {
            get { return data != null ? data.Length : 0; }
        }

        private byte[] data;
        /// <summary>
        /// Packet Data: the data coming from the network, including link-layer headers. The actual length of this field is Captured Len. 
        /// The format of the link-layer headers depends on the LinkType field specified in the Interface Description Block
        /// </summary>
        [IgnoreDuringToString]
        public byte [] Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        private EnchantedPacketOption options;
        /// <summary>
        /// optional fields. Optional fields can be used to insert some information that may be useful when reading data, but that is not 
        /// really needed for packet processing. Therefore, each tool can either read the content of the optional fields (if any), 
        /// or skip some of them or even all at once.
        /// </summary>
        public EnchantedPacketOption Options
        {
            get
            {
                return options;
            }
            set
            {
                options = value;
            }
        } 
        #endregion

        #region ctor
        public static EnchantedPacketBlock Parse(BaseBlock baseBlock, Action<Exception>? ActionOnException)
        {
            if (baseBlock.BlockType != BaseBlock.Types.EnhancedPacket)
                throw new ArgumentException("Invalid packet type");

            var positionInStream = baseBlock.PositionInStream;
            using Stream stream = new MemoryStream(baseBlock.Body);
            using var binaryReader = new BinaryReader(stream);
            var interfaceID = binaryReader.ReadInt32().ReverseByteOrder(baseBlock.ReverseByteOrder);
            var timestamp = binaryReader.ReadBytes(8);
            var timestampHelper = new TimestampHelper(timestamp, baseBlock.ReverseByteOrder);
            var capturedLength = binaryReader.ReadInt32().ReverseByteOrder(baseBlock.ReverseByteOrder);
            var packetLength = binaryReader.ReadInt32().ReverseByteOrder(baseBlock.ReverseByteOrder);
            var data = binaryReader.ReadBytes(capturedLength);
            if (data.Length < capturedLength)
                throw new EndOfStreamException("Unable to read beyond the end of the stream");
            var remainderLength = capturedLength % BaseBlock.AlignmentBoundary;
            if (remainderLength > 0)
            {
                var paddingLength = BaseBlock.AlignmentBoundary - remainderLength;
                binaryReader.ReadBytes(paddingLength);
            }
            var option = EnchantedPacketOption.Parse(binaryReader, baseBlock.ReverseByteOrder, ActionOnException);
            var enchantedBlock = new EnchantedPacketBlock(interfaceID, timestampHelper, packetLength, data, option, positionInStream);
            return enchantedBlock;
        }

        public static EnchantedPacketBlock CreateEnchantedPacketFromIPacket(IPacket packet, Action<Exception> ActionOnException)
        {
            var timestampHelper = new TimestampHelper(packet.Seconds, packet.Microseconds);

            var enchantedBlock = new EnchantedPacketBlock(0, timestampHelper, packet.Data.Length, packet.Data, new EnchantedPacketOption(), 0);
            return enchantedBlock;             
        }
        
        /// <summary>
        /// An Enhanced Packet Block is the standard container for storing the packets coming from the network. The Enhanced Packet Block 
        /// is optional because packets can be stored either by means of this block or the Simple Packet Block, which can be used to speed 
        /// up dump generation. 
        /// The Enhanced Packet Block is an improvement over the original Packet Block: 
        /// it stores the Interface Identifier as a 32bit integer value. This is a requirement when a capture stores packets coming from 
        /// a large number of interfaces differently from the Packet Block, the number of packets dropped by the capture system between 
        /// this packet and the previous one is not stored in the header, but rather in an option of the block itself.
        /// </summary>        
        public EnchantedPacketBlock(int InterfaceID, TimestampHelper Timestamp, int PacketLength, byte[] Data, EnchantedPacketOption Options, long PositionInStream = 0)
        {
            this.InterfaceID = InterfaceID;
            this.Timestamp = Timestamp;
            this.PacketLength = PacketLength;
            data = Data;
            options = Options;
            this.PositionInStream = PositionInStream;
        }
        #endregion        

        #region method
        protected override BaseBlock ConvertToBaseBlock(bool reverseByteOrder, Action<Exception>? ActionOnException)
        {
            var body = new List<byte>();
            body.AddRange(BitConverter.GetBytes(InterfaceID.ReverseByteOrder(reverseByteOrder)));
            body.AddRange(Timestamp.ConvertToByte(reverseByteOrder)); 
            body.AddRange(BitConverter.GetBytes(Data.Length.ReverseByteOrder(reverseByteOrder)));
            body.AddRange(BitConverter.GetBytes(PacketLength.ReverseByteOrder(reverseByteOrder)));
            body.AddRange(Data);
            var remainderLength = (BaseBlock.AlignmentBoundary - Data.Length % BaseBlock.AlignmentBoundary) % BaseBlock.AlignmentBoundary;
            for (var i = 0; i < remainderLength; i++)
            {
                body.Add(0);
            }
            body.AddRange(Options.ConvertToByte(reverseByteOrder, ActionOnException));
            var baseBlock = new BaseBlock(BlockType,body.ToArray(),reverseByteOrder,0);
            return baseBlock;
        }   
        #endregion
    }
}

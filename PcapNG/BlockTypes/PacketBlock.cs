using System;
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
    public sealed class PacketBlock:AbstractBlock,IPacket
    {
        #region nUnitTest
        [TestFixture]
        private static class PacketBlock_Test
        {
            [TestCase(true)]
            [TestCase(false)]
            public static void PacketBlock_ConvertToByte_Test(bool reorder)
            {
                PacketBlock? prePacketBlock;
                byte[] byteblock = { 2, 0, 0, 0, 167, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 123, 0, 0, 0, 232, 3, 0, 0, 104, 83, 17, 243, 59, 0, 0, 0, 151, 143, 0, 243, 59, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 208, 241, 255, 191, 127, 0, 0, 0, 208, 79, 17, 243, 59, 0, 0, 0, 96, 5, 0, 243, 59, 0, 0, 0, 252, 6, 0, 243, 59, 0, 0, 0, 96, 2, 0, 243, 59, 0, 0, 0, 88, 6, 64, 0, 0, 0, 0, 0, 104, 83, 17, 243, 59, 0, 0, 0, 104, 83, 17, 243, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 167, 0, 0, 0 };
                using (var stream = new MemoryStream(byteblock))
                {
                    using (var binaryReader = new BinaryReader(stream))
                    {
                        var block = AbstractBlockFactory.ReadNextBlock(binaryReader, false, null);
                        Assert.IsNotNull(block);
                        prePacketBlock = block as PacketBlock;
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
                        var postPacketBlock = block as PacketBlock;
                        Assert.IsNotNull(postPacketBlock);

                        Assert.AreEqual(prePacketBlock.BlockType, postPacketBlock!.BlockType);
                        Assert.AreEqual(prePacketBlock.Data, postPacketBlock.Data);
                        Assert.AreEqual(prePacketBlock.InterfaceID, postPacketBlock.InterfaceID);
                        Assert.AreEqual(prePacketBlock.DropCount, postPacketBlock.DropCount);
                        Assert.AreEqual(prePacketBlock.Microseconds, postPacketBlock.Microseconds);
                        Assert.AreEqual(prePacketBlock.PacketLength, postPacketBlock.PacketLength);
                        Assert.AreEqual(prePacketBlock.PositionInStream, postPacketBlock.PositionInStream);
                        Assert.AreEqual(prePacketBlock.Seconds, postPacketBlock.Seconds);
                        Assert.AreEqual(prePacketBlock.Options.Comment, postPacketBlock.Options.Comment);
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
        public override BaseBlock.Types BlockType => BaseBlock.Types.Packet;

        /// <summary>
        /// Interface ID: iit specifies the interface this packet comes from; the correct interface will be the one whose Interface Description
        ///  Block (within the current Section of the file) is identified by the same number (see Section 3.2) of this field.
        /// </summary>
        public short InterfaceID
        {
            get;
        }

        /// <summary>
        /// contains information about relations between packet and interface  on which it was captured 
        /// </summary>
        public override int? AssociatedInterfaceID => InterfaceID;

        /// <summary>
        /// Drops Count: a local drop counter. It specifies the number of packets lost (by the interface and the operating system) between 
        /// this packet and the preceding one. The value xFFFF (in hexadecimal) is reserved for those systems in which this information is 
        /// not available.
        /// </summary>
        public short DropCount
        {
            get;
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
        /// Captured Len: number of bytes captured from the packet (i.e. the length of the Packet Data field). It will be the minimum value 
        /// among the actual Packet Length and the snapshot length (SnapLen defined in Figure 9). The value of this field does not include 
        /// the padding bytes added at the end of the Packet Data field to align the Packet Data Field to a 32-bit boundary
        /// </summary>
        public int CapturedLength => Data.Length;

        /// <summary>
        /// Packet Len: actual length of the packet when it was transmitted on the network. Can be different from Captured Len if the user 
        /// wants only a snapshot of the packet.
        /// </summary>
        public int PacketLength
        {
            get;
            set;
        }

        /// <summary>
        /// Packet Data: the data coming from the network, including link-layer headers. The format of the link-layer headers depends on the LinkType field specified in the Interface Description Block 
        /// </summary>
        [IgnoreDuringToString]
        public byte [] Data { get; }

        /// <summary>
        /// optional fields. Optional fields can be used to insert some information that may be useful when reading data, but that is not 
        /// really needed for packet processing. Therefore, each tool can either read the content of the optional fields (if any), 
        /// or skip some of them or even all at once.
        /// </summary>
        public PacketOption Options { get; set; }

        #endregion

        #region ctor
        public static PacketBlock Parse(BaseBlock baseBlock, Action<Exception>? ActionOnException)
        {
            if (baseBlock.BlockType != BaseBlock.Types.Packet) throw new ArgumentException("Invalid packet type");

            var positionInStream = baseBlock.PositionInStream;
            using Stream stream = new MemoryStream(baseBlock.Body);
            using var binaryReader = new BinaryReader(stream);
            var interfaceID = binaryReader.ReadInt16().ReverseByteOrder(baseBlock.ReverseByteOrder);
            var dropCount = binaryReader.ReadInt16().ReverseByteOrder(baseBlock.ReverseByteOrder);
            var timestamp = binaryReader.ReadBytes(8);
            if (timestamp.Length < 8)
                throw new EndOfStreamException("Unable to read beyond the end of the stream");
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
            var options = PacketOption.Parse(binaryReader, baseBlock.ReverseByteOrder, ActionOnException);
            var packetBlock = new PacketBlock(interfaceID, timestampHelper, data, packetLength, options, (short) positionInStream);
            return packetBlock;
        }
  
        /// <summary>
        /// The Packet Block is marked obsolete, better use the Enhanced Packet Block instead!
        /// A Packet Block is the standard container for storing the packets coming from the network. The Packet Block is optional because 
        /// packets can be stored either by means of this block or the Simple Packet Block, which can be used to speed up dump generation.
        /// </summary>          
        public PacketBlock(short InterfaceID, TimestampHelper Timestamp,  byte[] Data, int PacketLength, PacketOption Options, short dropCount, long PositionInStream = 0)
        {
            this.InterfaceID = InterfaceID;
            this.Timestamp = Timestamp;
            this.PacketLength = PacketLength;
            this.Data = Data;
            this.Options = Options;
            DropCount = dropCount;
            this.PositionInStream = PositionInStream;
        }
        #endregion

        #region method
        protected override BaseBlock ConvertToBaseBlock(bool reverseByteOrder, Action<Exception>? ActionOnException)
        {
            var body = new List<byte>();
            body.AddRange(BitConverter.GetBytes(InterfaceID.ReverseByteOrder(reverseByteOrder)));
            body.AddRange(BitConverter.GetBytes(DropCount.ReverseByteOrder(reverseByteOrder)));
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
            var baseBlock = new BaseBlock(BlockType, body.ToArray(), reverseByteOrder, 0);
            return baseBlock;
        }
        #endregion
    }
}

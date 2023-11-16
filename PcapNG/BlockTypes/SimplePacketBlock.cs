using System;
using System.Collections.Generic;
using System.IO;
using PcapngUtils.Extensions;
using NUnit.Framework;
using PcapngUtils.Common;
namespace PcapngUtils.PcapNG.BlockTypes
{
    [ToString]    
    public sealed class SimplePacketBlock : AbstractBlock, IPacket
    {   
        #region nUnitTest
        [TestFixture]
        private static class SimplePacketBlock_Test
        {
            [TestCase(true)]
            [TestCase(false)]
            public static void SimplePacketBlock_ConvertToByte_Test(bool reorder)
            {
                SimplePacketBlock? prePacketBlock;
                byte[] byteblock = { 3, 0, 0, 0, 139, 0, 0, 0, 123, 0, 0, 0, 104, 83, 17, 243, 59, 0, 0, 0, 151, 143, 0, 243, 59, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 208, 241, 255, 191, 127, 0, 0, 0, 208, 79, 17, 243, 59, 0, 0, 0, 96, 5, 0, 243, 59, 0, 0, 0, 252, 6, 0, 243, 59, 0, 0, 0, 96, 2, 0, 243, 59, 0, 0, 0, 88, 6, 64, 0, 0, 0, 0, 0, 104, 83, 17, 243, 59, 0, 0, 0, 104, 83, 17, 243, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 139, 0, 0, 0 };
                using (var stream = new MemoryStream(byteblock))
                {
                    using (var binaryReader = new BinaryReader(stream))
                    {
                        var block = AbstractBlockFactory.ReadNextBlock(binaryReader, false, null);
                        Assert.IsNotNull(block);
                        prePacketBlock = block as SimplePacketBlock;
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
                        var postPacketBlock = block as SimplePacketBlock;
                        Assert.IsNotNull(postPacketBlock);

                        Assert.AreEqual(prePacketBlock.BlockType, postPacketBlock!.BlockType);
                        Assert.AreEqual(prePacketBlock.Data, postPacketBlock.Data);
                    }
                }
            }
        }
        #endregion

        #region IPacket
        public ulong Seconds
        {
            get { return 0; }
        }

        public ulong Microseconds
        {
            get { return 0; }
        }
        #endregion

        #region Properties
        /// <summary>
        /// The block type
        /// </summary>
        public override BaseBlock.Types BlockType => BaseBlock.Types.SimplePacket;

        /// <summary>
        /// contains information about relations between packet and interface  on which it was captured 
        /// </summary>
        public override int? AssociatedInterfaceID => 0;

        /// <summary>
        /// Packet Len: actual length of the packet when it was transmitted on the network. Can be different from captured len if the packet  
        /// has been truncated by the capture process.
        /// </summary>
        public int PacketLength => Data.Length;

        /// <summary>
        /// Packet Data: the data coming from the network, including link-layers headers. The length of this field can be derived from the field  
        /// Block Total Length, present in the Block Header, and it is the minimum value among the SnapLen (present in the Interface Description
        ///  Block) and the Packet Len (present in this header).
        /// </summary>
        [IgnoreDuringToString]
        public byte[] Data { get; }

        #endregion

        #region ctor
        public static SimplePacketBlock Parse(BaseBlock baseBlock, Action<Exception>? ActionOnException)
        {
            if (baseBlock.BlockType != BaseBlock.Types.SimplePacket) throw new ArgumentException("Invalid packet type");

            var positionInStream = baseBlock.PositionInStream;
            using Stream stream = new MemoryStream(baseBlock.Body);
            using var binaryReader = new BinaryReader(stream);
            var packetLength = binaryReader.ReadInt32().ReverseByteOrder(baseBlock.ReverseByteOrder);

            var data = binaryReader.ReadBytes(packetLength);
            if (data.Length < packetLength)
                throw new EndOfStreamException("Unable to read beyond the end of the stream");
            var remainderLength = packetLength % BaseBlock.AlignmentBoundary;
            if (remainderLength > 0)
            {
                var paddingLength = BaseBlock.AlignmentBoundary - remainderLength;
                binaryReader.ReadBytes(paddingLength);
            }
            var simplePacketBlock = new SimplePacketBlock(data, positionInStream);
            return simplePacketBlock;
        }

        /// <summary>
        /// The Simple Packet Block is a lightweight container for storing the packets coming from the network. Its presence is optional.
        /// A Simple Packet Block is similar to a Packet Block (see Section 3.5), but it is smaller, simpler to process and contains only a 
        /// minimal set of information. This block is preferred to the standard Packet Block when performance or space occupation are 
        /// critical factors, such as in sustained traffic dump applications. A capture file can contain both Packet Blocks and Simple Packet 
        /// Blocks: for example, a capture tool could switch from Packet Blocks to Simple Packet Blocks when the hardware resources become 
        /// critical. The Simple Packet Block does not contain the Interface ID field. Therefore, it must be assumed that all the Simple Packet 
        /// Blocks have been captured on the interface previously specified in the first Interface Description Block.
        /// </summary>       
        public SimplePacketBlock( byte[] Data, long PositionInStream = 0)
        {
            this.Data = Data;                    
            this.PositionInStream = PositionInStream;
        }
        #endregion

        #region method
        protected override BaseBlock ConvertToBaseBlock(bool reverseByteOrder, Action<Exception>? ActionOnException)
        {
            var body = new List<byte>();
            body.AddRange(BitConverter.GetBytes(Data.Length.ReverseByteOrder(reverseByteOrder)));
            body.AddRange(Data);
            var remainderLength = (BaseBlock.AlignmentBoundary - Data.Length % BaseBlock.AlignmentBoundary) % BaseBlock.AlignmentBoundary;
            for (var i = 0; i < remainderLength; i++)
            {
                body.Add(0);
            }            
            var baseBlock = new BaseBlock(BlockType, body.ToArray(), reverseByteOrder, 0);
            return baseBlock;
        }
        #endregion
    }
}

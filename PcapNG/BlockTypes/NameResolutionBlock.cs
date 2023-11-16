using System;
using System.Collections.Generic;
using System.IO;
using PcapngUtils.PcapNG.OptionTypes;
using NUnit.Framework;
namespace PcapngUtils.PcapNG.BlockTypes
{
    [ToString]    
    public sealed class NameResolutionBlock:AbstractBlock
    {
        #region nUnitTest
        [TestFixture]
        private static class NameResolutionBlock_Test
        {
            [TestCase(true)]
            [TestCase(false)]
            public static void NameResolutionBlock_ConvertToByte_Test(bool reorder)
            {
                byte[] option = { 1, 0, 12, 0, 84, 101, 115, 116, 32, 67, 111, 109, 109, 101, 110, 116, 2, 0, 8, 0, 68, 110, 115, 32, 78, 97, 109, 101, 3, 0, 4, 0, 127, 0, 0, 1, 4, 0, 16, 0, 32, 1, 13, 219, 0, 0, 0, 0, 0, 0, 0, 0, 20, 40, 87, 171, 0, 0, 0, 0 };
                byte[] records = { 1, 0, 13, 0, 127, 0, 0, 1, 108, 111, 99, 97, 108, 104, 111, 115, 116, 0, 0, 0, 2, 0, 25, 0, 32, 1, 13, 219, 0, 0, 0, 0, 0, 0, 0, 0, 20, 40, 87, 171, 116, 101, 115, 116, 32, 97, 100, 100, 114, 0, 0, 0, 0, 0, 0, 0 };

                NameResolutionBlock prePacketBlock;
                NameResolutionBlock? postPacketBlock;
                using (var optionStream = new MemoryStream(option))
                {
                    using (var recordsStream = new MemoryStream(records))
                    {
                        using (var optionBinaryReader = new BinaryReader(optionStream))
                        {
                            using (var recordsBinaryReader = new BinaryReader(recordsStream))
                            {
                                var rec = NameResolutionRecord.Parse(recordsBinaryReader, false, null);
                                Assert.IsNotNull(rec);
                                var opt = NameResolutionOption.Parse(optionBinaryReader, false, null);
                                Assert.IsNotNull(opt);
                                prePacketBlock = new NameResolutionBlock(rec, opt, 0);
                            }
                        }
                    }
                }
                Assert.IsNotNull(prePacketBlock);
                var byteblock = prePacketBlock.ConvertToByte(reorder, null);
                using (var stream = new MemoryStream(byteblock))
                {
                    using (var binaryReader = new BinaryReader(stream))
                    {
                        var block = AbstractBlockFactory.ReadNextBlock(binaryReader, reorder, null);
                        Assert.IsNotNull(block);
                        postPacketBlock = block as NameResolutionBlock;
                        Assert.IsNotNull(postPacketBlock);

                        Assert.AreEqual(prePacketBlock.BlockType, postPacketBlock!.BlockType);
                        Assert.AreEqual(prePacketBlock.NameResolutionRecords.Count, postPacketBlock.NameResolutionRecords.Count);
                        for (var i = 0; i < prePacketBlock.NameResolutionRecords.Count; i++)
                        {
                            Assert.AreEqual(prePacketBlock.NameResolutionRecords[i], postPacketBlock.NameResolutionRecords[i]);
                        }
                        Assert.AreEqual(prePacketBlock.Options.Comment, postPacketBlock.Options.Comment);
                        Assert.AreEqual(prePacketBlock.Options.DnsName, postPacketBlock.Options.DnsName);
                        Assert.AreEqual(prePacketBlock.Options.DnsIp4Addr, postPacketBlock.Options.DnsIp4Addr);
                        Assert.AreEqual(prePacketBlock.Options.DnsIp6Addr, postPacketBlock.Options.DnsIp6Addr);
                    }
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// The block type
        /// </summary>
        public override BaseBlock.Types BlockType
        {
            get { return BaseBlock.Types.NameResolution; }
        }

        /// <summary>
        /// contains information about relations between packet and interface  on which it was captured 
        /// </summary>
        public override int? AssociatedInterfaceID => null;

        [IgnoreDuringToString]
        public NameResolutionRecord NameResolutionRecords { get; }

        /// <summary>
        /// optional fields. Optional fields can be used to insert some information that may be useful when reading data, but that is not 
        /// really needed for packet processing. Therefore, each tool can either read the content of the optional fields (if any), 
        /// or skip some of them or even all at once.
        /// </summary>
        public NameResolutionOption Options { get; }

        #endregion

        #region ctor
        public static NameResolutionBlock Parse(BaseBlock baseBlock, Action<Exception>? ActionOnException)
        {
            if (baseBlock.BlockType != BaseBlock.Types.NameResolution)
                throw new ArgumentException("Invalid packet type");

            var positionInStream = baseBlock.PositionInStream;
            using Stream stream = new MemoryStream(baseBlock.Body);
            using var binaryReader = new BinaryReader(stream);
            var nameResolutionRecords = NameResolutionRecord.Parse(binaryReader, baseBlock.ReverseByteOrder, ActionOnException);
            var options = NameResolutionOption.Parse(binaryReader, baseBlock.ReverseByteOrder, ActionOnException);
            var nameResolutionBlock = new NameResolutionBlock(nameResolutionRecords,options, positionInStream);
            return nameResolutionBlock;
        }

        /// <summary>
        /// The Name Resolution Block is used to support the correlation of numeric addresses (present in the captured packets) and their 
        /// corresponding canonical names and it is optional. Having the literal names saved in the file, this prevents the need of a name 
        /// resolution in a delayed time, when the association between names and addresses can be different from the one in use at capture time. 
        /// Moreover, the Name Resolution Block avoids the need of issuing a lot of DNS requests every time the trace capture is opened, 
        /// and allows to have name resolution also when reading the capture with a machine not connected to the network.
        /// A Name Resolution Block is normally placed at the beginning of the file, but no assumptions can be taken about its position. 
        /// Name Resolution Blocks can be added in a second time by tools that process the file, like network analyzers.
        /// </summary>
        public NameResolutionBlock(NameResolutionRecord nameResolutionRecords, NameResolutionOption Options, long PositionInStream = 0)
        {              
            NameResolutionRecords = nameResolutionRecords;            
            this.Options = Options;
            this.PositionInStream = PositionInStream;
        }
        #endregion

        #region method
        protected override BaseBlock ConvertToBaseBlock(bool reverseByteOrder, Action<Exception>? ActionOnException)
        {
            var body = new List<byte>();
            body.AddRange(NameResolutionRecords.ConvertToByte(reverseByteOrder, ActionOnException));
            body.AddRange(Options.ConvertToByte(reverseByteOrder, ActionOnException));
            var baseBlock = new BaseBlock(BlockType, body.ToArray(), reverseByteOrder, 0);
            return baseBlock;
        }
        #endregion
    }
}

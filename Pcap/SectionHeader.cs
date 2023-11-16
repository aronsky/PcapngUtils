using System;
using System.Collections.Generic;
using System.IO;
using PcapngUtils.Extensions;
using NUnit.Framework;
using PcapngUtils.Common;

namespace PcapngUtils.Pcap
{
    [ToString]    
    public sealed class SectionHeader
    {
        #region nUnitTest
        [TestFixture]
        public static class SectionHeader_Test
        {
            [Test]
            public static void SectionHeader_ConvertToByte_Test()
            {
                var pre = CreateEmptyHeader(false, false);
                using var stream = new MemoryStream(pre.ConvertToByte());
                using var br = new BinaryReader(stream);
                var post = Parse(br);
                Assert.AreEqual(pre.MagicNumber, post.MagicNumber);
                Assert.AreEqual(pre.ReverseByteOrder, post.ReverseByteOrder);
                Assert.AreEqual(pre.MajorVersion, post.MajorVersion);
                Assert.AreEqual(pre.MinorVersion, post.MinorVersion);
                Assert.AreEqual(pre.LinkType, post.LinkType);
                Assert.AreEqual(pre.MaximumCaptureLength, post.MaximumCaptureLength);
                Assert.AreEqual(pre.NanoSecondResolution, post.NanoSecondResolution);
                Assert.AreEqual(pre.SignificantFigures, post.SignificantFigures);
                Assert.AreEqual(pre.TimezoneOffset, post.TimezoneOffset);
            }
        }
        #endregion

        #region enum
        public enum MagicNumbers:uint
        {
            nanosecondIdentical = 0xa1b23c4d,
            nanosecondSwapped = 0x4d3cb2a1,
            microsecondIdentical = 0xa1b2c3d4,
            microsecondSwapped = 0xd4c3b2a1 
        }
        #endregion

        #region Properties 
        /// <summary>
        /// version_major, version_minor: the version number of this file format (current version is 2.4)
        /// </summary>
        public ushort MajorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// version_major, version_minor: the version number of this file format (current version is 2.4)
        /// </summary>
        public ushort MinorVersion
        {
            get;
            set;
        }
        /// <summary>
        /// snaplen: the "snapshot length" for the capture (typically 65535 or even more, but might be limited by the user), 
        /// see: incl_len vs. orig_len below
        /// </summary>
        public uint MaximumCaptureLength
        {
            get;
            set;
        }
        /// <summary>
        /// thiszone: the correction time in seconds between GMT (UTC) and the local timezone of the following packet header timestamps. 
        /// Examples: If the timestamps are in GMT (UTC), thiszone is simply 0. If the timestamps are in Central European time 
        /// (Amsterdam, Berlin, ...) which is GMT + 1:00, thiszone must be -3600. In practice, time stamps are always in GMT, so thiszone is always 0.
        /// </summary>
        public int TimezoneOffset
        {
            get;
            set;
        }

        /// <summary>
        /// sigfigs: in theory, the accuracy of time stamps in the capture; in practice, all tools set it to 0
        /// </summary>
        public uint SignificantFigures
        {
            get;
            set;
        }

        /// <summary>
        /// network: link-layer header type, specifying the type of headers at the beginning of the packet (e.g. 1 for Ethernet, 
        /// see tcpdump.org's link-layer header types page for details); this can be various types such as 802.11, 802.11 with various 
        /// radio information, PPP, Token Ring, FDDI, etc.
        /// </summary>
        public LinkTypes LinkType
        {
            get;
            set;
        }

        /// <summary>
        /// magic_number: used to detect the file format itself and the byte ordering. The writing application writes 0xa1b2c3d4 
        /// with it's native byte ordering format into this field. The reading application will read either 0xa1b2c3d4 (identical) 
        /// or 0xd4c3b2a1 (swapped). If the reading application reads the swapped 0xd4c3b2a1 value, it knows that all the following 
        /// fields will have to be swapped too. For nanosecond-resolution files, the writing application writes 0xa1b23c4d, 
        /// with the two nibbles of the two lower-order bytes swapped, and the reading application will read either 0xa1b23c4d (identical) 
        /// or 0x4d3cb2a1 (swapped).
        /// </summary>
        public MagicNumbers MagicNumber
        {
            get;
            private set;
        }

        
        /// <summary>
        /// determines whether the computer and stream endianness are different (See comment MagicNumber)
        /// Examples:   System Endiannes -> LitleEndian, Stream Endiannes BigEndian -> ReverseByteOrder -> true 
        ///             System Endiannes -> LitleEndian, Stream Endiannes LitleEndian -> ReverseByteOrder -> false 
        /// </summary>
        public bool ReverseByteOrder
        {
            get
            {
                return MagicNumber == MagicNumbers.microsecondSwapped || MagicNumber == MagicNumbers.nanosecondSwapped;
            }
        } 

        /// <summary>
        /// determines whether packets are stored in the stream nanosecond resolution. (See comment MagicNumber)
        /// </summary>
        public bool NanoSecondResolution
        {
            get
            {
                return MagicNumber == MagicNumbers.nanosecondIdentical || MagicNumber == MagicNumbers.nanosecondSwapped;
            }
        }
        
        /// <summary>
        /// Header magic number converted to string
        /// </summary>
        public string MagicNumberAsString
        {
            get
            {
                return ((uint)MagicNumber).ToString("x");
            }
        }

        #endregion

        #region ctor
        
        public SectionHeader(MagicNumbers magicNumber = MagicNumbers.microsecondIdentical, ushort majorVersion = 2, ushort minorVersion = 4, int thiszone =0 , uint sigfigs = 0, uint snaplen = 65535, LinkTypes linkType = LinkTypes.Ethernet)
        {              
            MagicNumber = magicNumber;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            TimezoneOffset = thiszone;
            SignificantFigures = sigfigs;
            MaximumCaptureLength = snaplen;
            LinkType = linkType;
        }

        public static SectionHeader Parse(BinaryReader binaryReader)
        {
            var reverseByteOrder = false; 
            var positionInStream = binaryReader.BaseStream.Position;
            var tempMagicNumber = binaryReader.ReadUInt32();
            if (!Enum.IsDefined(typeof(MagicNumbers), tempMagicNumber))
                throw new ArgumentException(
                    $"[SectionHeader.Parse] Unrecognized pcap magic number: {tempMagicNumber.ToString("x")}");

            var magicNumber = (MagicNumbers)tempMagicNumber;

            if (magicNumber == MagicNumbers.nanosecondIdentical || magicNumber == MagicNumbers.microsecondIdentical)
                reverseByteOrder = false;
            else
                reverseByteOrder = true;
           
            
            var major = binaryReader.ReadUInt16().ReverseByteOrder(reverseByteOrder);
            var minor = binaryReader.ReadUInt16().ReverseByteOrder(reverseByteOrder);
            var thiszone = binaryReader.ReadInt32().ReverseByteOrder(reverseByteOrder);
            var sigfigs = binaryReader.ReadUInt32().ReverseByteOrder(reverseByteOrder);
            var snaplen = binaryReader.ReadUInt32().ReverseByteOrder(reverseByteOrder);
            var linktype = binaryReader.ReadUInt32().ReverseByteOrder(reverseByteOrder);

            if (!Enum.IsDefined(typeof(LinkTypes), (ushort)linktype))
                throw new ArgumentException(
                    $"[SectionHeader.Parse] Invalid LinkTypes: {linktype}, block begin on position {positionInStream} ");
            var LinkType = (LinkTypes)linktype;
            var header = new SectionHeader(magicNumber, major, minor, thiszone, sigfigs, snaplen, LinkType);
            return header;             
        }

        public static SectionHeader CreateEmptyHeader(bool nanosecond = false, bool reverseByteOrder = false)
        {
            MagicNumbers flag;
            if (nanosecond)
                flag = reverseByteOrder? MagicNumbers.nanosecondSwapped :MagicNumbers.nanosecondIdentical;            
            else
                flag = reverseByteOrder ? MagicNumbers.microsecondSwapped : MagicNumbers.microsecondIdentical;

            return new SectionHeader(flag, 2, 4, 0, 0, 65535, LinkTypes.Ethernet);
        }
        #endregion          

        #region method
        public byte[] ConvertToByte()
        {
            var ret = new List<byte>();
            var reverseByteOrder = ReverseByteOrder;
            ret.AddRange(BitConverter.GetBytes((uint)MagicNumber));
            ret.AddRange(BitConverter.GetBytes(MajorVersion.ReverseByteOrder(reverseByteOrder)));
            ret.AddRange(BitConverter.GetBytes(MinorVersion.ReverseByteOrder(reverseByteOrder)));
            ret.AddRange(BitConverter.GetBytes(TimezoneOffset.ReverseByteOrder(reverseByteOrder)));
            ret.AddRange(BitConverter.GetBytes(SignificantFigures.ReverseByteOrder(reverseByteOrder)));
            ret.AddRange(BitConverter.GetBytes(MaximumCaptureLength.ReverseByteOrder(reverseByteOrder)));
            ret.AddRange(BitConverter.GetBytes(((uint)LinkType).ReverseByteOrder(reverseByteOrder)));
            return ret.ToArray();
        }
        #endregion
    }
}

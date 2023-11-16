using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.IO;
using NUnit.Framework;
using System.Net;
using System.Net.Sockets;

namespace PcapngUtils.PcapNG.OptionTypes
{
    [ToString]      
    public sealed class NameResolutionOption : AbstractOption
    {
        #region nUnitTest
        [TestFixture]
        private static class NameResolutionOption_Test
        {
            [TestCase(true)]
            [TestCase(false)]
            [ContractVerification(false)]
            public static void NameResolutionOption_ConvertToByte_Test(bool reorder)
            {
                var preOption = new NameResolutionOption();
                NameResolutionOption postOption;
                preOption.Comment = "Test Comment";
                preOption.DnsName = "Dns Name";
                preOption.DnsIp4Addr = new IPAddress(new byte[] { 127, 0, 0, 1 });
                preOption.DnsIp6Addr = new IPAddress(new byte[] { 0x20, 0x01, 0x0d, 0x0db, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x14, 0x28, 0x57, 0xab });

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
                Assert.AreEqual(preOption.DnsName, postOption.DnsName);
                Assert.AreEqual(preOption.DnsIp4Addr, postOption.DnsIp4Addr);
                Assert.AreEqual(preOption.DnsIp6Addr, postOption.DnsIp6Addr);

            }
        }
        #endregion

        #region enum
        public enum NameResolutionOptionCode : ushort
        {
            EndOfOptionsCode = 0,
            CommentCode = 1,
            DnsNameCode = 2,
            DnsIp4AddrCode = 3,
            DnsIp6AddrCode = 4
        }
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
        /// A UTF-8 string containing the name of the machine (DNS server) used to perform the name resolution.
        /// </summary>
        public string? DnsName
        {
            get;
            private set;
        }

        private IPAddress? dnsIp4Addr;
        /// <summary>
        /// Specifies an IPv4 address (contained in the first 4 bytes), followed by one or more zero-terminated strings containing 
        /// the DNS entries for that address.
        /// </summary>
        public IPAddress? DnsIp4Addr
        {
            get => dnsIp4Addr;
            private set
            {
                if (value?.AddressFamily != AddressFamily.InterNetwork)
                    throw new ArgumentException("dnsIp4Addr is not AddressFamily.InterNetwork");
                dnsIp4Addr = value;
            }
        }

        private IPAddress? dnsIp6Addr;
        /// <summary>
        /// Specifies an IPv6 address (contained in the first 16 bytes), followed by one or more zero-terminated strings containing 
        /// the DNS entries for that address.
        /// </summary>
        public IPAddress? DnsIp6Addr
        {
            get => dnsIp6Addr;
            private set
            {
                if (value?.AddressFamily != AddressFamily.InterNetworkV6)
                    throw new ArgumentException("dnsIp6Addr is not AddressFamily.InterNetworkV6");
                dnsIp6Addr = value;
            }
        }
        #endregion

        #region ctor
        public NameResolutionOption(string? Comment = null, string? DnsName = null, IPAddress? DnsIp4Addr = null, IPAddress? DnsIp6Addr = null)
        {
            if (DnsIp4Addr is {AddressFamily: not AddressFamily.InterNetwork})
                throw new ArgumentException("dnsIp4Addr is not AddressFamily.InterNetwork");
            if (DnsIp6Addr is {AddressFamily: not AddressFamily.InterNetworkV6})
                throw new ArgumentException("dnsIp6Addr is not AddressFamily.InterNetworkV6");
                    
            this.Comment = Comment;
            this.DnsName = DnsName;
            dnsIp4Addr = DnsIp4Addr;
            dnsIp6Addr = DnsIp6Addr;
        }
        #endregion

        #region method
        public static NameResolutionOption Parse(BinaryReader binaryReader, bool reverseByteOrder, Action<Exception>? ActionOnException)
        {
            var option = new NameResolutionOption();
            var optionsList = ExtractOptions(binaryReader, reverseByteOrder, ActionOnException);
            if (!optionsList.Any()) return option;
            foreach (var item in optionsList)
            {
                try
                {
                    switch (item.Key)
                    {
                        case (ushort)NameResolutionOptionCode.CommentCode:
                            option.Comment = Encoding.UTF8.GetString(item.Value);
                            break;
                        case (ushort)NameResolutionOptionCode.DnsNameCode:
                            option.DnsName = Encoding.UTF8.GetString(item.Value);
                            break;
                        case (ushort)NameResolutionOptionCode.DnsIp4AddrCode:
                            if (item.Value.Length == 4)
                                option.DnsIp4Addr = new IPAddress(item.Value);
                            else
                                throw new ArgumentException(
                                    $"[NameResolutionOption.Parse] DnsIp4AddrCode contains invalid length. Received: {item.Value.Length} bytes, expected: {4}");
                            break;
                        case (ushort)NameResolutionOptionCode.DnsIp6AddrCode:                                
                            if (item.Value.Length == 16)
                                option.DnsIp6Addr = new IPAddress(item.Value);
                            else
                                throw new ArgumentException(
                                    $"[NameResolutionOption.Parse] DnsIp6AddrCode contains invalid length. Received: {item.Value.Length} bytes, expected: {16}");
                            break;
                        case (ushort)NameResolutionOptionCode.EndOfOptionsCode:
                        default:
                            break;
                    }
                }
                catch (Exception exc)
                {
                    ActionOnException?.Invoke(exc);
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
                    ret.AddRange(ConvertOptionFieldToByte((ushort)NameResolutionOptionCode.CommentCode, comentValue, reverseByteOrder, ActionOnException));
            }

            if (DnsName != null)
            {
                var nameValue = Encoding.UTF8.GetBytes(DnsName);
                if (nameValue.Length <= ushort.MaxValue)
                    ret.AddRange(ConvertOptionFieldToByte((ushort)NameResolutionOptionCode.DnsNameCode, nameValue, reverseByteOrder, ActionOnException));
            }

            if (DnsIp4Addr != null)
            {
                ret.AddRange(ConvertOptionFieldToByte((ushort)NameResolutionOptionCode.DnsIp4AddrCode, DnsIp4Addr.GetAddressBytes(), reverseByteOrder, ActionOnException));
            }

            if (DnsIp6Addr != null)
            {
                ret.AddRange(ConvertOptionFieldToByte((ushort)NameResolutionOptionCode.DnsIp6AddrCode, DnsIp6Addr.GetAddressBytes(), reverseByteOrder, ActionOnException));
            }

            ret.AddRange(ConvertOptionFieldToByte((ushort)NameResolutionOptionCode.EndOfOptionsCode, Array.Empty<byte>(), reverseByteOrder, ActionOnException));
            return ret.ToArray();
        }
        #endregion
    }
}

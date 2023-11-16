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
    public sealed class NameResolutionRecordEntry 
    {
        #region enum
        public enum NameResolutionRecordCode : ushort
        {
            EndOfRecord = 0,
            Ip4Record = 1,
            Ip6Record = 2,
        };
        #endregion

        #region fields & properies

        /// <summary>
        /// The IPv4 or IPv6 address of the DNS server.
         /// </summary>
        public IPAddress IpAddr { get; }

        /// <summary>
        /// A UTF-8 string containing the name of the machine (DNS server) used to perform the name resolution.
        /// </summary>        
        public string Description { get; }

        #endregion

        #region ctor
        public NameResolutionRecordEntry(IPAddress IpAddr, string Description)
        {
            if (IpAddr.AddressFamily is not (AddressFamily.InterNetwork or AddressFamily.InterNetworkV6))
                throw new ArgumentException("Invalid ip address family");
            this.IpAddr = IpAddr;
            this.Description = Description;
        }
        #endregion

        #region method
        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var p = (NameResolutionRecordEntry)obj;
          
           return Enumerable.SequenceEqual(IpAddr.GetAddressBytes(), p.IpAddr.GetAddressBytes()) && (Description.CompareTo(p.Description) == 0);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

    }

    [ToString]     
    public sealed class NameResolutionRecord : AbstractOption, IList<NameResolutionRecordEntry>
    {
        #region nUnitTest
        [TestFixture]
        private static class NameResolutionRecord_Test
        {
            [TestCase(true)]
            [TestCase(false)]
            [ContractVerification(false)]
            public static void NameResolutionRecord_ConvertToByte_Test(bool reorder)
            {
                NameResolutionRecord postNameResolution;
                var preNameResolution = new NameResolutionRecord(new List<NameResolutionRecordEntry>());
                preNameResolution.Add(new NameResolutionRecordEntry(new IPAddress(new byte[] { 127, 0, 0, 1 }), "localhost"));
                preNameResolution.Add(new NameResolutionRecordEntry(new IPAddress(new byte[] { 0x20, 0x01, 0x0d, 0x0db, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x14, 0x28, 0x57, 0xab }), "test addr"));

                var preNameResolutionRecord = preNameResolution.ConvertToByte(reorder, null);

                using (var stream = new MemoryStream(preNameResolutionRecord))
                {
                    using (var binaryReader = new BinaryReader(stream))
                    {
                        postNameResolution = Parse(binaryReader, reorder, null);
                    }
                }

                Assert.IsNotNull(postNameResolution);
                Assert.AreEqual(preNameResolution.Count, postNameResolution.Count);
                for (var i = 0; i < preNameResolution.Count; i++)
                {
                    Assert.AreEqual(preNameResolution[i].IpAddr, postNameResolution[i].IpAddr);
                    Assert.AreEqual(preNameResolution[i].Description, postNameResolution[i].Description);
                }
            }
        }
        #endregion        

        #region fields & properies
        private readonly List<NameResolutionRecordEntry> listRecords = new List<NameResolutionRecordEntry>();

        public int IndexOf(NameResolutionRecordEntry item)
        {
            return listRecords.IndexOf(item);
        }

        public void Insert(int index, NameResolutionRecordEntry item)
        {
            listRecords.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            listRecords.RemoveAt(index);
        }

        public NameResolutionRecordEntry this[int index]
        {
            get
            {
                return listRecords[index];
            }
            set
            {
                listRecords[index] = value;
            }
        }

        public void Add(NameResolutionRecordEntry item)
        {
            listRecords.Add(item);
        }

        public void Clear()
        {
            listRecords.Clear();
        }

        public bool Contains(NameResolutionRecordEntry item)
        {
            return listRecords.Contains(item);
        }

        public void CopyTo(NameResolutionRecordEntry[] array, int arrayIndex)
        {
            listRecords.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return listRecords.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(NameResolutionRecordEntry item)
        {
            return listRecords.Remove(item);
        }

        public IEnumerator<NameResolutionRecordEntry> GetEnumerator()
        {
            return listRecords.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return listRecords.GetEnumerator();
        }
        #endregion

        #region ctor
        private NameResolutionRecord(List<NameResolutionRecordEntry> listRecords)
        {
            this.listRecords = listRecords; 
        }
        #endregion

        #region method
        public static NameResolutionRecord Parse(BinaryReader binaryReader, bool reverseByteOrder, Action<Exception>? ActionOnException)
        {
            var listRecords = new List<NameResolutionRecordEntry>();
            var keyValueList = ExtractOptions(binaryReader, reverseByteOrder, ActionOnException);

            foreach (var item in keyValueList)
            {
                try
                {
                    switch (item.Key)
                    {
                        case (ushort)NameResolutionRecordEntry.NameResolutionRecordCode.Ip4Record:
                            {
                                if (item.Value.Length >= 4)
                                {
                                    var addrTemp = item.Value.Take(4).ToArray();
                                    var descTemp = item.Value.Skip(4).Take(item.Value.Length - 4).ToArray();
                                    var addr = new IPAddress(addrTemp);
                                    var desc = Encoding.UTF8.GetString(descTemp);
                                    var record = new NameResolutionRecordEntry(addr, desc);
                                    listRecords.Add(record);
                                }
                                break;
                            }
                        case (ushort)NameResolutionRecordEntry.NameResolutionRecordCode.Ip6Record:
                            {
                                if (item.Value.Length >= 16)
                                {
                                    var addrTemp = item.Value.Take(16).ToArray();
                                    var descTemp = item.Value.Skip(16).Take(item.Value.Length - 16).ToArray();
                                    var addr = new IPAddress(addrTemp);
                                    var desc = Encoding.UTF8.GetString(descTemp);
                                    var record = new NameResolutionRecordEntry(addr, desc);
                                    listRecords.Add(record);
                                }
                                break;
                            }
                        case (ushort)NameResolutionRecordEntry.NameResolutionRecordCode.EndOfRecord:
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
            return new NameResolutionRecord(listRecords);
        }

        public override byte[] ConvertToByte(bool reverseByteOrder, Action<Exception>? ActionOnException)
        {
            var ret = new List<byte>();
            foreach (var record in listRecords)
            {
                switch(record.IpAddr.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        {
                            var temp = new List<byte>();
                            temp.AddRange(record.IpAddr.GetAddressBytes());
                            temp.AddRange( Encoding.UTF8.GetBytes(record.Description));
                            if (temp.Count <= ushort.MaxValue)
                                ret.AddRange(ConvertOptionFieldToByte((ushort)NameResolutionRecordEntry.NameResolutionRecordCode.Ip4Record, temp.ToArray(), reverseByteOrder, ActionOnException));
                        }
                        break;
                    case AddressFamily.InterNetworkV6:
                        {
                            var temp = new List<byte>();
                            temp.AddRange(record.IpAddr.GetAddressBytes());
                            temp.AddRange(Encoding.UTF8.GetBytes(record.Description));
                            if (temp.Count <= ushort.MaxValue)
                                ret.AddRange(ConvertOptionFieldToByte((ushort)NameResolutionRecordEntry.NameResolutionRecordCode.Ip6Record, temp.ToArray(), reverseByteOrder, ActionOnException));
                        }
                        break;
                }                
            }

            ret.AddRange(ConvertOptionFieldToByte((ushort)NameResolutionRecordEntry.NameResolutionRecordCode.EndOfRecord, Array.Empty<byte>(), reverseByteOrder, ActionOnException));
            return ret.ToArray();
        }

        
        #endregion

       
    }
}

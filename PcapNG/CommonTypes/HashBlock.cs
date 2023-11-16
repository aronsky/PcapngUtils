﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PcapngUtils.PcapNG.CommonTypes
{
    [ToString]     
    public sealed class HashBlock
    {
        #region nUnitTest
        [TestFixture]
        private static class HashBlock_Test
        {
            [Test]
            public static void HashBlock_InvalidEnum_Test()
            {
                var md5Hash = "f59b7efafd800e27b47a488d30615c73";
                var utf8Hash = Encoding.UTF8.GetBytes(md5Hash);
                byte[]? test = { 5 };
                test = test.Concat(utf8Hash).ToArray();

                var hashBlock = new HashBlock(test);
                Assert.AreEqual(hashBlock.Algorithm, HashAlgorithm.Invalid);
            }

            [Test]
            public static void HashBlock_Md5Test_Test()
            {
                var md5Hash = "f59b7efafd800e27b47a488d30615c73";
                var utf8Hash = Encoding.UTF8.GetBytes(md5Hash);
                byte[]? test = { (byte)HashAlgorithm.Md5 };
                test = test.Concat(utf8Hash).ToArray();

                var hashBlock = new HashBlock(test);
                Assert.AreEqual(hashBlock.Algorithm, HashAlgorithm.Md5);
                Assert.AreEqual(hashBlock.StringValue, md5Hash);
            }
        }
        #endregion

        #region enum
        public enum HashAlgorithm : byte
        {
            TwoSComplement = 0,
            Xor = 1,
            Crc32 = 2,
            Md5 = 3,
            Sha1 = 4,
            Invalid = 7
        }
        #endregion

        #region fields && properties
        public HashAlgorithm Algorithm
        {
            get;
            private set;
        }

        public byte[] Value
        {
            get;
            private set;
        }

        public string StringValue
        {
            get
            {
                try
                {
                    var ret = Encoding.UTF8.GetString(Value);
                    return ret;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
        #endregion
        
        #region ctor
        public HashBlock(IReadOnlyList<byte> inputArray)
        {
            if (inputArray.Count < 2) throw new ArgumentException("HashBlock, inputArray length < 2");

            var tempAlgorithm = inputArray[0];
            Algorithm = Enum.IsDefined(typeof(HashAlgorithm), tempAlgorithm) ? (HashAlgorithm)tempAlgorithm : HashAlgorithm.Invalid;
            Value = inputArray.Skip(1).Take(inputArray.Count - 1).ToArray();
        }   
        #endregion       

        #region method
        public byte[] ConvertToByte()
        {
            var ret = new List<byte>();
            ret.Add((byte)Algorithm);
            ret.AddRange(Value);
            return ret.ToArray();
        }
        
        
        public override bool Equals(object? obj)
        {     
            if (obj == null || GetType() != obj.GetType())
                return false;

            var p = (HashBlock)obj;
            return (Algorithm == p.Algorithm) && (Value == p.Value);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}

using NUnit.Framework;
using System;

namespace PcapngUtils.Extensions
{    
    public static class ReverseByteOrderExtension
    {          
        #region nUnitTest
        [TestFixture]
        private static class ReverseByteOrderExtension_Test
        {
            [Test]
            public static void ReverseByteOrderExtension_UInt16_Test()
            {
                ushort origin = 0xABCD;
                var test = origin.ReverseByteOrder(false);
                Assert.AreEqual(test, origin);
                test = origin.ReverseByteOrder(true);
                Assert.AreEqual(test, 0xCDAB);
            }

            [Test]
            public static void ReverseByteOrderExtension_UInt32_Test()
            {
                var origin = 0xABCDEF01;
                uint test = origin.ReverseByteOrder(false);
                Assert.AreEqual(test, origin);
                test = origin.ReverseByteOrder(true);
                Assert.AreEqual(test, 0x01EFCDAB);
            }

            [Test]
            public static void ReverseByteOrderExtension_UInt64_Test()
            {
                ulong origin = 0x0123456789ABCDEF;
                ulong test = origin.ReverseByteOrder(false);
                Assert.AreEqual(test, origin);
                test = origin.ReverseByteOrder(true);
                Assert.AreEqual(test, 0xEFCDAB8967452301);
            }

            [Test]
            public static void ReverseByteOrderExtension_Int16_Test()
            {
                short origin = 0x3210;
                short test = origin.ReverseByteOrder(false);
                Assert.AreEqual(test, origin);
                test = origin.ReverseByteOrder(true);
                Assert.AreEqual(test, 0x1032);
            }

            [Test]
            public static void ReverseByteOrderExtension_Int32_Test()
            {
                var origin = 0x76543210;
                int test = origin.ReverseByteOrder(false);
                Assert.AreEqual(test, origin);
                test = origin.ReverseByteOrder(true);
                Assert.AreEqual(test, 0x10325476);
            }

            [Test]
            public static void ReverseByteOrderExtension_Int64_Test()
            {
                var origin = 0x7654321076543210;
                long test = origin.ReverseByteOrder(false);
                Assert.AreEqual(test, origin);
                test = origin.ReverseByteOrder(true);
                Assert.AreEqual(test, 0x1032547610325476);
            }
        }
        #endregion
        #region Extenstion method
        public static uint ReverseByteOrder(this uint value,bool reverseByteOrder)
        {
            if (!reverseByteOrder)
                return value;
            else
            {
                var bytes = BitConverter.GetBytes(value); 
                Array.Reverse(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
        }

        public static int ReverseByteOrder(this int value, bool reverseByteOrder)
        {
            if (!reverseByteOrder)
                return value;
            else
            {
                var bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                return BitConverter.ToInt32(bytes, 0);
            }
        }

        public static ushort ReverseByteOrder(this ushort value, bool reverseByteOrder)
        {
            if (!reverseByteOrder)
                return value;
            else
            {
                var bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                return BitConverter.ToUInt16(bytes, 0);
            }
        }

        public static short ReverseByteOrder(this short value, bool reverseByteOrder)
        {
            if (!reverseByteOrder)                
                return value;
            else
            {
                var bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                return BitConverter.ToInt16(bytes, 0);
            }
        }

        public static ulong ReverseByteOrder(this ulong value, bool reverseByteOrder)
        {
            if (!reverseByteOrder)
                return value;
            else
            {
                var bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                return BitConverter.ToUInt64(bytes, 0);
            }
        }

        public static long ReverseByteOrder(this long value, bool reverseByteOrder)
        {
            if (!reverseByteOrder)
                return value;
            else
            {
                var bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                return BitConverter.ToInt64(bytes, 0);
            }
        }
        #endregion
    }
}

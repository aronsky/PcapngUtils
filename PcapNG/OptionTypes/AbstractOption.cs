using System;
using System.Collections.Generic;
using System.IO;
using PcapngUtils.Extensions;

namespace PcapngUtils.PcapNG.OptionTypes
{
    public abstract class AbstractOption
    {
        #region fields && properties
        public static readonly int AlignmentBoundary = 4;
        private const short EndOfOption = 0;
        #endregion

        #region methods
        protected static List<KeyValuePair<ushort, byte[]>> ExtractOptions(BinaryReader binaryReader, bool reverseByteOrder, Action<Exception>? ActionOnException)
        {
            var ret = new List<KeyValuePair<ushort, byte[]>>();

            if (binaryReader.BaseStream.Position + 4 >= binaryReader.BaseStream.Length)
                return ret;
                
            try
            {
                while (binaryReader.BaseStream.Position + 4 <= binaryReader.BaseStream.Length)
                {
                    var optionCode = binaryReader.ReadUInt16().ReverseByteOrder(reverseByteOrder);
                    var valueLength = binaryReader.ReadUInt16().ReverseByteOrder(reverseByteOrder);
                    byte[] value;
                    if (valueLength > 0)
                    {
                        value = binaryReader.ReadBytes(valueLength);
                        if (value.Length < valueLength)
                            throw new EndOfStreamException("Unable to read beyond the end of the stream");
                        var remainderLength = valueLength % AlignmentBoundary;
                        if (remainderLength > 0)
                            binaryReader.ReadBytes(AlignmentBoundary - remainderLength);
                    }
                    else
                        break;

                    ret.Add(new KeyValuePair<ushort, byte[]>(optionCode, value));
                    if (optionCode == EndOfOption)
                        break;
                }
            }
            catch  (Exception exc)
            {
                ActionOnException?.Invoke(exc);
            }
            return ret;
        }

        protected static byte[] ConvertOptionFieldToByte(ushort optionType, byte[] value, bool reverseByteOrder, Action<Exception>? ActionOnException)
        {
            switch (value.Length)
            {
                case < 1 when optionType != 0:
                    throw new ArgumentException("Only for optionType == 0 value.Lenght can be == 0");
                case > ushort.MaxValue:
                    throw new IndexOutOfRangeException("value.Length > ushort.MaxValue");
            }

            var ret = new List<byte>();

            try
            {
                var remainderLength = (AlignmentBoundary - value.Length % AlignmentBoundary) % AlignmentBoundary;
                ret.AddRange(BitConverter.GetBytes(optionType.ReverseByteOrder(reverseByteOrder)));
                ret.AddRange(BitConverter.GetBytes(((ushort)value.Length).ReverseByteOrder(reverseByteOrder)));
                if (value.Length > 0)
                {
                    ret.AddRange(value);
                    for (var i = 0; i < remainderLength; i++)
                    {
                        ret.Add(0);
                    }
                }
            }
            catch (Exception exc)
            {
                ActionOnException?.Invoke(exc);
            }

            return ret.ToArray();
        }
        public abstract byte[] ConvertToByte(bool reverseByteOrder, Action<Exception>? ActionOnException);
         
        #endregion
    }
}

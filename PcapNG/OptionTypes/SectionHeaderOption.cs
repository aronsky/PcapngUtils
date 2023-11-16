using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.IO;
using NUnit.Framework;

namespace PcapngUtils.PcapNG.OptionTypes
{
    [ToString]     
    public sealed class SectionHeaderOption : AbstractOption
    {
        #region nUnitTest
        [TestFixture]
        private static class SectionHeaderOption_Test
        {
            [TestCase(true)]
            [TestCase(false)]
            [ContractVerification(false)]
            public static void SectionHeaderOption_ConvertToByte_Test(bool reorder)
            {
                var preOption = new SectionHeaderOption();
                SectionHeaderOption postOption;
                preOption.Comment = "Test Comment";
                preOption.Hardware = "x86 Personal Computer";
                preOption.OperatingSystem = "Windows 7";
                preOption.UserApplication = "PcapngUtils";
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
                Assert.AreEqual(preOption.Hardware, postOption.Hardware);
                Assert.AreEqual(preOption.OperatingSystem, postOption.OperatingSystem);
                Assert.AreEqual(preOption.UserApplication, postOption.UserApplication);
            }
        }
        #endregion

        #region enum
        public enum SectionHeaderOptionCode : ushort
        {
            EndOfOptionsCode = 0,
            CommentCode = 1,
            HardwareCode = 2,
            OperatingSystemCode = 3,
            UserApplicationCode = 4
        };
        #endregion

        #region fields & properies
        /// <summary>
        /// A UTF-8 string containing a comment that is associated to the current block.
        /// </summary>
        public string? Comment
        {
            get;
            set;
        }

        /// <summary>
        /// An UTF-8 string containing the description of the hardware used to create this section.
        /// </summary>
        public string? Hardware
        {
            get;
            set;
        }

        /// <summary>
        /// An UTF-8 string containing the name of the operating system used to create this section
        /// </summary>
        public string? OperatingSystem
        {
            get;
            set;
        }

        /// <summary>
        /// An UTF-8 string containing the name of the application used to create this section.
        /// </summary>
        public string? UserApplication
        {
            get;
            set;
        }
        #endregion

        #region ctor
        public SectionHeaderOption(string? Comment = null, string? Hardware = null, string? OperatingSystem = null, string? UserApplication = null)
        {
            this.Comment = Comment;
            this.Hardware = Hardware;
            this.OperatingSystem = OperatingSystem;
            this.UserApplication = UserApplication;
        }
        #endregion

        #region method
        public static SectionHeaderOption Parse(BinaryReader binaryReader, bool reverseByteOrder, Action<Exception>? ActionOnException)
        {
            var option = new SectionHeaderOption();
            var optionsList = ExtractOptions(binaryReader, reverseByteOrder, ActionOnException);
            if (!optionsList.Any()) return option;
            foreach (var item in optionsList)
            {
                try
                {
                    switch (item.Key)
                    {
                        case (ushort)SectionHeaderOptionCode.CommentCode:
                            option.Comment = Encoding.UTF8.GetString(item.Value);
                            break;
                        case (ushort)SectionHeaderOptionCode.HardwareCode:
                            option.Hardware = Encoding.UTF8.GetString(item.Value);
                            break;
                        case (ushort)SectionHeaderOptionCode.OperatingSystemCode:
                            option.OperatingSystem = Encoding.UTF8.GetString(item.Value);
                            break;
                        case (ushort)SectionHeaderOptionCode.UserApplicationCode:
                            option.UserApplication = Encoding.UTF8.GetString(item.Value);
                            break;
                    }
                }
                catch (Exception exc)
                {
                    if (ActionOnException != null)
                        ActionOnException(exc);
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
                    ret.AddRange(ConvertOptionFieldToByte((ushort)SectionHeaderOptionCode.CommentCode, comentValue, reverseByteOrder, ActionOnException));
            }

            if (Hardware != null)
            {
                var hardwareValue = Encoding.UTF8.GetBytes(Hardware);
                if (hardwareValue.Length <= ushort.MaxValue)
                    ret.AddRange(ConvertOptionFieldToByte((ushort)SectionHeaderOptionCode.HardwareCode, hardwareValue, reverseByteOrder, ActionOnException));
            }

            if (OperatingSystem != null)
            {
                var systemValue = Encoding.UTF8.GetBytes(OperatingSystem);
                if (systemValue.Length <= ushort.MaxValue)
                    ret.AddRange(ConvertOptionFieldToByte((ushort)SectionHeaderOptionCode.OperatingSystemCode, systemValue, reverseByteOrder, ActionOnException));
            }

            if (UserApplication != null)
            {
                var userAppValue = Encoding.UTF8.GetBytes(UserApplication);
                if (userAppValue.Length <= ushort.MaxValue)
                    ret.AddRange(ConvertOptionFieldToByte((ushort)SectionHeaderOptionCode.UserApplicationCode, userAppValue, reverseByteOrder, ActionOnException));
            }


            ret.AddRange(ConvertOptionFieldToByte((ushort)SectionHeaderOptionCode.EndOfOptionsCode, Array.Empty<byte>(), reverseByteOrder, ActionOnException));
            return ret.ToArray();
        }
        #endregion
    }
}

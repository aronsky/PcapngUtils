using PcapngUtils.Common;
using PcapngUtils.Pcap;
using PcapngUtils.PcapNG;
using PcapngUtils.PcapNG.BlockTypes;
using System;
using System.IO;

namespace PcapngUtils
{
    public sealed class IReaderFactory
    {
        #region fields && properties

        #endregion

        #region methods
        public static IReader GetReader(string path)
        {
            if (!File.Exists(path)) throw new ArgumentException("file must exist");
            
            uint mask;
            using (var stream = new FileStream(path, FileMode.Open))
            {
                using (var binaryReader = new BinaryReader(stream))
                {
                    if (binaryReader.BaseStream.Length < 12)
                        throw new ArgumentException($"[IReaderFactory.GetReader] file {path} is too short ");

                    mask = binaryReader.ReadUInt32();
                    if (mask == (uint)BaseBlock.Types.SectionHeader)
                    {
                        binaryReader.ReadUInt32();
                        mask = binaryReader.ReadUInt32();
                    }
                }
            }

            switch (mask)
            {       
                case (uint)SectionHeader.MagicNumbers.microsecondIdentical:
                case (uint)SectionHeader.MagicNumbers.microsecondSwapped:
                case (uint)SectionHeader.MagicNumbers.nanosecondSwapped:
                case (uint)SectionHeader.MagicNumbers.nanosecondIdentical:
                    {
                        IReader reader = new PcapReader(path);
                        return reader;
                    }
                case (uint)SectionHeaderBlock.MagicNumbers.Identical:
                    {
                        IReader reader = new PcapNGReader(path, false);
                        return reader;
                    }
                case (uint)SectionHeaderBlock.MagicNumbers.Swapped:
                    {
                        IReader reader = new PcapNGReader(path, true);
                        return reader;
                    }
                default:
                    throw new ArgumentException($"[IReaderFactory.GetReader] file {path} is not PCAP/PCAPNG file");
            }
        }
        
        public static IReader GetReader(Stream stream)
        {
            uint mask;
            
            using (var binaryReader = new BinaryReader(stream))
            {
                if (binaryReader.BaseStream.Length < 12)
                    throw new ArgumentException($"[IReaderFactory.GetReader] stream is too short ");

                mask = binaryReader.ReadUInt32();
                if (mask == (uint)BaseBlock.Types.SectionHeader)
                {
                    binaryReader.ReadUInt32();
                    mask = binaryReader.ReadUInt32();
                }
            }

            stream.Seek(0, SeekOrigin.Begin);

            switch (mask)
            {       
                case (uint)SectionHeader.MagicNumbers.microsecondIdentical:
                case (uint)SectionHeader.MagicNumbers.microsecondSwapped:
                case (uint)SectionHeader.MagicNumbers.nanosecondSwapped:
                case (uint)SectionHeader.MagicNumbers.nanosecondIdentical:
                {
                    IReader reader = new PcapReader(stream);
                    return reader;
                }
                case (uint)SectionHeaderBlock.MagicNumbers.Identical:
                {
                    IReader reader = new PcapNGReader(stream, false);
                    return reader;
                }
                case (uint)SectionHeaderBlock.MagicNumbers.Swapped:
                {
                    IReader reader = new PcapNGReader(stream, true);
                    return reader;
                }
                default:
                    throw new ArgumentException($"[IReaderFactory.GetReader] stream is not PCAP/PCAPNG file");
            }
        }
        #endregion
    }
}

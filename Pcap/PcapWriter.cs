using System;
using System.Collections.Generic;
using System.IO;
using PcapngUtils.Extensions;
using PcapngUtils.Common;
using System.Runtime.ExceptionServices;

namespace PcapngUtils.Pcap
{
    public sealed class PcapWriter : Disposable, IWriter
    {
        #region event & delegate

        public event CommonDelegates.ExceptionEventDelegate? OnExceptionEvent;

        private void OnException(Exception exception)
        {
            var handler = OnExceptionEvent;
            if (handler != null)
                handler(this, exception);
            else
                ExceptionDispatchInfo.Capture(exception).Throw();
        }

        #endregion

        #region fields & properties

        private Stream stream;
        private BinaryWriter binaryWriter;
        private SectionHeader header;
        private readonly object syncRoot = new();

        #endregion

        #region ctor

        public PcapWriter(string path, bool nanoseconds = false, bool reverseByteOrder = false) : this(path,
            SectionHeader.CreateEmptyHeader(nanoseconds, reverseByteOrder))
        {
        }

        public PcapWriter(string path, SectionHeader header) : this(new FileStream(path, FileMode.Create), header)
        {
        }

        public PcapWriter(Stream stream, bool nanoseconds = false, bool reverseByteOrder = false) : this(stream,
            SectionHeader.CreateEmptyHeader(nanoseconds, reverseByteOrder))
        {
        }

        public PcapWriter(Stream stream, SectionHeader header)
        {
            if (!stream.CanWrite) throw new ArgumentException("Cannot write to stream");
            this.header = header;
            this.stream = stream;
            binaryWriter = new BinaryWriter(stream);
            binaryWriter.Write(header.ConvertToByte());
        }

        #endregion

        /// <summary>
        /// Close stream, dispose members
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        public void WritePacket(IPacket packet)
        {
            try
            {
                var secs = (uint) packet.Seconds;
                var usecs = (uint) packet.Microseconds;
                if (header.NanoSecondResolution)
                    usecs = usecs * 1000;
                var caplen = (uint) packet.Data.Length;
                var len = (uint) packet.Data.Length;
                var data = packet.Data;

                var ret = new List<byte>();

                ret.AddRange(BitConverter.GetBytes(secs.ReverseByteOrder(header.ReverseByteOrder)));
                ret.AddRange(BitConverter.GetBytes(usecs.ReverseByteOrder(header.ReverseByteOrder)));
                ret.AddRange(BitConverter.GetBytes(caplen.ReverseByteOrder(header.ReverseByteOrder)));
                ret.AddRange(BitConverter.GetBytes(len.ReverseByteOrder(header.ReverseByteOrder)));
                ret.AddRange(data);
                if (ret.Count > header.MaximumCaptureLength)
                    throw new ArgumentOutOfRangeException(
                        $"[PcapWriter.WritePacket] packet length: {ret.Count} is greater than MaximumCaptureLength: {header.MaximumCaptureLength}");
                lock (syncRoot)
                {
                    binaryWriter.Write(ret.ToArray());
                }
            }
            catch (Exception exc)
            {
                OnException(exc);
            }
        }


        #region IDisposable Members

        /// <summary>
        /// Close stream, dispose members
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (binaryWriter != null)
                binaryWriter.Close();
            if (stream != null)
                stream.Close();
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using PcapngUtils.PcapNG.BlockTypes;
using System.Linq;
using PcapngUtils.Common;
using System.Runtime.ExceptionServices;

namespace PcapngUtils.PcapNG
{
    public sealed class PcapNGWriter : Disposable, IWriter
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

        private List<HeaderWithInterfacesDescriptions> headersWithInterface = new List<HeaderWithInterfacesDescriptions>();
        public IList<HeaderWithInterfacesDescriptions> HeadersWithInterfaces
        {
            get { return headersWithInterface.AsReadOnly(); }
        }

        private object syncRoot = new object();
        #endregion

        #region ctor
        public PcapNGWriter(string path) : this(path, 
            new List<HeaderWithInterfacesDescriptions> {HeaderWithInterfacesDescriptions.CreateEmptyHeaderWithInterfacesDescriptions(false)})
        { }
        
        public PcapNGWriter(string path, List<HeaderWithInterfacesDescriptions> headersWithInterface) : this(new FileStream(path, FileMode.Create), headersWithInterface)
        { }

        public PcapNGWriter(Stream stream) : this(stream,
            new List<HeaderWithInterfacesDescriptions> {HeaderWithInterfacesDescriptions.CreateEmptyHeaderWithInterfacesDescriptions(false)})
        { }

        public PcapNGWriter(Stream stream, List<HeaderWithInterfacesDescriptions> headersWithInterface)
        {
            if (headersWithInterface.Count < 1) throw new ArgumentException("headersWithInterface list is empty");
            this.stream = stream;
            binaryWriter = new BinaryWriter(stream);
            Initialize(headersWithInterface);
        }

        private void Initialize(List<HeaderWithInterfacesDescriptions> headersWithInterface)
        {
            if (!stream.CanWrite) throw new Exception("Cannot write to stream");

            if (headersWithInterface.Count < 1) throw new ArgumentException("headersWithInterface list is empty");

            this.headersWithInterface = headersWithInterface;
            binaryWriter = new BinaryWriter(stream);
            Action<Exception> reThrowException = (exc) => { ExceptionDispatchInfo.Capture(exc).Throw(); };
            foreach (var header in headersWithInterface)
            {
                binaryWriter.Write(header.ConvertToByte(header.Header.ReverseByteOrder, reThrowException));
            }
               
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
                var abstractBlock = packet as AbstractBlock ?? EnchantedPacketBlock.CreateEnchantedPacketFromIPacket(packet, OnException);

                var header = HeadersWithInterfaces.Last();
                var data = abstractBlock.ConvertToByte(header.Header.ReverseByteOrder, OnException);

                if (abstractBlock.AssociatedInterfaceID.HasValue)
                {
                    if (abstractBlock.AssociatedInterfaceID.Value >= header.InterfaceDescriptions.Count)
                    {
                        throw new ArgumentOutOfRangeException(
                            $"[PcapNGWriter.WritePacket] Packet interface ID: {abstractBlock.AssociatedInterfaceID.Value} is greater than InterfaceDescriptions count: {header.InterfaceDescriptions.Count}");
                    }
                    var maxLength = header.InterfaceDescriptions[abstractBlock.AssociatedInterfaceID.Value].SnapLength;
                    if (data.Length > maxLength)
                    {
                        throw new ArgumentOutOfRangeException(
                            $"[PcapNGWriter.WritePacket] block length: {data.Length} is greater than MaximumCaptureLength: {maxLength}");
                            
                    }
                }
                lock (syncRoot)
                {
                    binaryWriter.Write(data);
                }
            }
            catch (Exception exc)
            {
                OnException(exc);
            }
        }

        public void WriteHeaderWithInterfacesDescriptions(HeaderWithInterfacesDescriptions headersWithInterface)
        {
            var data = headersWithInterface.ConvertToByte(headersWithInterface.Header.ReverseByteOrder, OnException);
            try
            {
                lock (syncRoot)
                {
                    binaryWriter.Write(data);
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

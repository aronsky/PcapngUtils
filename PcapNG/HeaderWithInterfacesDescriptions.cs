using PcapngUtils.PcapNG.BlockTypes;
using System;
using System.Collections.Generic;

namespace PcapngUtils.PcapNG
{
    public class HeaderWithInterfacesDescriptions
    {
        #region fields && properties

        public SectionHeaderBlock Header { get; }

        private readonly List<InterfaceDescriptionBlock> interfaceDescriptions;
        public IList<InterfaceDescriptionBlock> InterfaceDescriptions => interfaceDescriptions.AsReadOnly();

        #endregion

        #region ctor
        public HeaderWithInterfacesDescriptions(SectionHeaderBlock header, List<InterfaceDescriptionBlock> interfaceDescriptions)
        {
            if (interfaceDescriptions.Count < 1) throw new ArgumentException("Interface description list is empty");
            
            Header = header;
            this.interfaceDescriptions = interfaceDescriptions;
        }
        #endregion


        #region method
        public byte[] ConvertToByte(bool reverseBytesOrder, Action<Exception>? ActionOnException )
        {
            var ret = new List<byte>();
            try
            {
                ret.AddRange(Header.ConvertToByte(reverseBytesOrder, ActionOnException));
                
                foreach (var item in interfaceDescriptions)
                {
                    ret.AddRange(item.ConvertToByte(reverseBytesOrder, ActionOnException));                   
                }
            }
            catch (Exception exc)
            {
                ActionOnException?.Invoke(exc);
            }
            return ret.ToArray();
        }
        #endregion

        public static HeaderWithInterfacesDescriptions CreateEmptyHeaderWithInterfacesDescriptions(bool reverseBytesOrder)
        {
            var header = SectionHeaderBlock.GetEmptyHeader(reverseBytesOrder);
            var emptyInterface = InterfaceDescriptionBlock.GetEmptyInterfaceDescription(reverseBytesOrder);
            return new HeaderWithInterfacesDescriptions(header, new List<InterfaceDescriptionBlock> { emptyInterface });
        }
    }
}

namespace PcapngUtils.Common
{
    public interface IPacket
    {
        ulong Seconds {get;}
        ulong Microseconds{get;}
        byte[] Data { get; }
    }
}

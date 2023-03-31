namespace ThingsEdge.Common.Internal;

public interface IPlatform
{
    int GetCurrentProcessId();

    byte[] GetDefaultDeviceId();
}

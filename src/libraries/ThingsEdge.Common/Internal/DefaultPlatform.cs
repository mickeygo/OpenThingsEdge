namespace ThingsEdge.Common.Internal;

internal sealed class DefaultPlatform : IPlatform
{
    private int _processId;

    public DefaultPlatform()
    {
        // We cannot use System.Diagnostics.Process.GetCurrentProcess here because System.Diagnostics.Process
        // is not part of .Net Standard 1.3. Since a time-seeded random number is already used in DefaultChannelId 
        // (the only consumer of this API), we'll use the first 4 bytes of a UUID for better entropy

        var processGuid = Guid.NewGuid();
        _processId = BitConverter.ToInt32(processGuid.ToByteArray(), 0);

        // DotNetty expects process id to be no greater than 0x400000, so clear this higher bits:
        _processId = _processId & 0x3FFFFF;
        Contract.Assert(_processId <= 0x400000);

        _processId = Process.GetCurrentProcess().Id;
    }

    int IPlatform.GetCurrentProcessId() => _processId;

    byte[] IPlatform.GetDefaultDeviceId() => MacAddressUtil.GetBestAvailableMac();
}

using System.Runtime.InteropServices;

namespace MonoKits.Helper;

public static partial class PlatformHelper
{
    public static string PlatformName { get; private set; }

    [LibraryImport("libc")]
    private static partial int uname(IntPtr buf);

    static PlatformHelper()
    {
        PlatformID pid = Environment.OSVersion.Platform;

        switch (pid)
        {
            case PlatformID.Win32NT:
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.WinCE:
                PlatformName = "Windows";
                break;
            case PlatformID.MacOSX:
                PlatformName = "MacOSX";
                break;
            case PlatformID.Unix:

                // Mac can return a value of Unix sometimes, We need to double check it.
                IntPtr buf = IntPtr.Zero;
                try
                {
                    buf = Marshal.AllocHGlobal(8192);

                    if (uname(buf) == 0)
                    {
                        if (Marshal.PtrToStringAnsi(buf) == "Darwin")
                        {
                            PlatformName = "MacOSX";
                            return;
                        }
                    }
                }
                catch
                {

                }
                finally
                {
                    if (buf != IntPtr.Zero)
                        Marshal.FreeHGlobal(buf);
                }

                PlatformName = "Linux";
                break;
            default:
                PlatformName = "Unknown";
                break;
        }
    }
}

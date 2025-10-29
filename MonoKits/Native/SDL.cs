using MonoKits.Helper;
using System.Runtime.InteropServices;

namespace MonoKits.Native;

public static class SDL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_starttextinput();
    public readonly static d_sdl_starttextinput StartTextInput = null!;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_stoptextinput();
    public readonly static d_sdl_stoptextinput StopTextInput = null!;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_settextinputrect(ref Rectangle rect);
    public readonly static d_sdl_settextinputrect SetTextInputRect = null!;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_setwindowminimumsize(IntPtr window, int min_w, int min_h);
    public readonly static d_sdl_setwindowminimumsize SetWindowMinimumSize = null!;

    static SDL()
    {
        string targetLibrary = PlatformHelper.PlatformName switch
        {
            "Windows" => "SDL2.dll",
            "Linux" => "libSDL2-2.0.so.0",
            "MacOSX" => "libSDL2-2.0.0.dylib",
            _ => "sdl2"
        };

        string? nativeLibraryPath = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, targetLibrary, SearchOption.AllDirectories)
            .FirstOrDefault();

        if (nativeLibraryPath == null) return;
        if (!NativeLibrary.TryLoad(nativeLibraryPath, out var handle)) return;

        StartTextInput = Marshal.GetDelegateForFunctionPointer<d_sdl_starttextinput>(NativeLibrary.GetExport(handle, "SDL_StartTextInput"));
        StopTextInput = Marshal.GetDelegateForFunctionPointer<d_sdl_stoptextinput>(NativeLibrary.GetExport(handle, "SDL_StopTextInput"));
        SetTextInputRect = Marshal.GetDelegateForFunctionPointer<d_sdl_settextinputrect>(NativeLibrary.GetExport(handle, "SDL_SetTextInputRect"));
        SetWindowMinimumSize = Marshal.GetDelegateForFunctionPointer<d_sdl_setwindowminimumsize>(NativeLibrary.GetExport(handle, "SDL_SetWindowMinimumSize"));
    }
}

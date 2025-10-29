using System.Runtime.InteropServices;

namespace MonoKits.Native;

public unsafe struct NativeArray<T> : IDisposable where T : unmanaged
{
    private uint _length; 
    private void* _ptr;

    public NativeArray(uint length)
    {
        _length = length;
        _ptr = NativeMemory.Alloc(_length, (uint)sizeof(T));
    }

    public readonly int Length => (int)_length;

    public readonly Span<T> AsSpan() => new(_ptr, (int)_length);

    public void Dispose()
    {
        if (_ptr != null)
        {
            NativeMemory.Free(_ptr);
            _ptr = null;
            _length = 0;
        }
    }
}

namespace Pollus.Emscripten;

using System.Runtime.InteropServices;
using Pollus.Mathematics;
using Pollus.Utils;
using Silk.NET.OpenAL;

static partial class EmscriptenOpenALNative
{
    #region Context
    [LibraryImport("OpenAL")]
    public unsafe static partial Device* alcOpenDevice(byte* name);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alcCloseDevice(Device* device);

    [LibraryImport("OpenAL")]
    public unsafe static partial Context* alcCreateContext(Device* device, int* attrlist);

    [LibraryImport("OpenAL")]
    public unsafe static partial Context* alcDestroyContext(Context* device);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alcMakeContextCurrent(Context* context);
    #endregion

    #region Buffers
    [LibraryImport("OpenAL")]
    public unsafe static partial void alBufferData(uint buffer, BufferFormat format, void* data, int size, int freq);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alDeleteBuffers(int n, uint* buffers);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alGenBuffers(int n, uint* buffers);
    #endregion

    #region Source
    [LibraryImport("OpenAL")]
    public unsafe static partial void alGenSources(int n, uint* sources);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alDeleteSources(int n, uint* sources);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourcePlay(uint source);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourcePause(uint source);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourceStop(uint source);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourceRewind(uint source);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alGetSourcei(uint source, GetSourceInteger param, int* value);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alGetSourcei(uint source, SourceBoolean param, [MarshalAs(UnmanagedType.I4)] ref bool value);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alGetSource3i(uint source, GetSourceInteger param, int* value1, int* value2, int* value3);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alGetSourceiv(uint source, GetSourceInteger param, int* values);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alGetSourcef(uint source, SourceFloat param, float* value);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alGetSource3f(uint source, SourceVector3 param, float* value1, float* value2, float* value3);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alGetSourcefv(uint source, SourceVector3 param, float* values);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourcei(uint source, SourceInteger param, int value);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourcei(uint source, SourceInteger param, uint value);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourcei(uint source, SourceBoolean param, [MarshalAs(UnmanagedType.I4)] bool value);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSource3i(uint source, SourceInteger param, int value1, int value2, int value3);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourceiv(uint source, SourceInteger param, int* value);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourcef(uint source, SourceFloat param, float value);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSource3f(uint source, SourceVector3 param, float value1, float value2, float value3);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourcefv(uint source, SourceVector3 param, float* value);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourcePlayv(int n, uint* sources);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourceStopv(int n, uint* sources);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourcePausev(int n, uint* sources);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourceRewind(int n, uint* sources);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourceQueueBuffers(uint source, int nb, uint* buffers);

    [LibraryImport("OpenAL")]
    public unsafe static partial void alSourceUnqueueBuffers(uint source, int nb, uint* buffers);
    #endregion
}

public class ALContext
{
    unsafe public Device* OpenDevice(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return EmscriptenOpenALNative.alcOpenDevice(null);
        }
        else
        {
            using var namePtr = TemporaryPin.PinString(name);
            return EmscriptenOpenALNative.alcOpenDevice((byte*)namePtr.Ptr);
        }
    }

    unsafe public void CloseDevice(Device* device)
    {
        EmscriptenOpenALNative.alcCloseDevice(device);
    }

    unsafe public Context* CreateContext(Device* device, int* attrlist)
    {
        return EmscriptenOpenALNative.alcCreateContext(device, attrlist);
    }

    unsafe public void DestroyContext(Context* context)
    {
        EmscriptenOpenALNative.alcDestroyContext(context);
    }

    unsafe public void MakeContextCurrent(Context* context)
    {
        EmscriptenOpenALNative.alcMakeContextCurrent(context);
    }
}

public class AL
{
    unsafe public void BufferData(uint buffer, BufferFormat format, void* data, int size, int freq)
    {
        EmscriptenOpenALNative.alBufferData(buffer, format, data, size, freq);
    }

    unsafe public uint GenBuffer()
    {
        uint result = 0u;
        EmscriptenOpenALNative.alGenBuffers(1, &result);
        return result;
    }

    unsafe public void GenBuffers(int n, uint* buffers)
    {
        EmscriptenOpenALNative.alGenBuffers(n, buffers);
    }

    unsafe public void DeleteBuffer(uint buffer)
    {
        EmscriptenOpenALNative.alDeleteBuffers(1, &buffer);
    }

    unsafe public void DeleteBuffers(int n, uint* buffers)
    {
        EmscriptenOpenALNative.alDeleteBuffers(n, buffers);
    }

    unsafe public uint GenSource()
    {
        uint result = 0u;
        EmscriptenOpenALNative.alGenSources(1, &result);
        return result;
    }

    unsafe public void DeleteSource(uint source)
    {
        EmscriptenOpenALNative.alDeleteSources(1, &source);
    }

    unsafe public void SourcePlay(uint source)
    {
        EmscriptenOpenALNative.alSourcePlay(source);
    }

    unsafe public void SourcePause(uint source)
    {
        EmscriptenOpenALNative.alSourcePause(source);
    }

    unsafe public void SourceStop(uint source)
    {
        EmscriptenOpenALNative.alSourceStop(source);
    }

    unsafe public void SourceRewind(uint source)
    {
        EmscriptenOpenALNative.alSourceRewind(source);
    }

    unsafe public void SourceQueueBuffers(uint source, int nb, uint* buffers)
    {
        EmscriptenOpenALNative.alSourceQueueBuffers(source, nb, buffers);
    }

    unsafe public void SourceUnqueueBuffers(uint source, int nb, uint* buffers)
    {
        EmscriptenOpenALNative.alSourceUnqueueBuffers(source, nb, buffers);
    }

    unsafe public void GetSourceProperty(uint source, GetSourceInteger param, out int value)
    {
        fixed (int* ptr = &value)
        {
            EmscriptenOpenALNative.alGetSourcei(source, param, ptr);
        }
    }

    unsafe public void GetSourceProperty(uint source, SourceBoolean param, out bool value)
    {
        value = false;
        EmscriptenOpenALNative.alGetSourcei(source, param, ref value);
    }

    unsafe public void GetSourceProperty(uint source, SourceFloat param, out float value)
    {
        fixed (float* ptr = &value)
        {
            EmscriptenOpenALNative.alGetSourcef(source, param, ptr);
        }
    }

    unsafe public void GetSourceProperty(uint source, SourceVector3 param, out Vec3<float> value)
    {
        fixed (float* x = &value.X, y = &value.Y, z = &value.Z)
        {
            EmscriptenOpenALNative.alGetSource3f(source, param, x, y, z);
        }
    }

    unsafe public void SetSourceProperty(uint source, SourceFloat param, float value)
    {
        EmscriptenOpenALNative.alSourcef(source, param, value);
    }

    unsafe public void SetSourceProperty(uint source, SourceVector3 param, Vec3<float> value)
    {
        EmscriptenOpenALNative.alSource3f(source, param, value.X, value.Y, value.Z);
    }

    unsafe public void SetSourceProperty(uint source, SourceBoolean param, bool value)
    {
        EmscriptenOpenALNative.alSourcei(source, param, value);
    }

    unsafe public void SetSourceProperty(uint source, SourceInteger param, int value)
    {
        EmscriptenOpenALNative.alSourcei(source, param, value);
    }

    unsafe public void SetSourceProperty(uint source, SourceInteger param, uint value)
    {
        EmscriptenOpenALNative.alSourcei(source, param, value);
    }
}
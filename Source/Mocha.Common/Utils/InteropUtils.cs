using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mocha.Common;

public static class MemoryLogger
{
	public static ConcurrentDictionary<string, (int Allocations, int Frees)> Entries = new();

	public static void AllocatedBytes( string name, int count )
	{
		if ( !Entries.ContainsKey( name ) )
			Entries[name] = (0, 0);

		Entries[name] = (Entries[name].Allocations + count, Entries[name].Frees);
	}

	public static void FreedBytes( string name, int count )
	{
		if ( !Entries.ContainsKey( name ) )
			Entries[name] = (0, 0);

		Entries[name] = (Entries[name].Allocations, Entries[name].Frees + count);
	}
}

/// <summary>
/// Use this for allocating memory, it will automatically free it
/// when IDisposable.Dispose is called.
/// </summary>
public class MemoryContext : IDisposable
{
	enum Type
	{
		CoTaskMem,
		HGlobal
	}

	private List<(Type Type, IntPtr Pointer)> Values { get; } = new();
	private string Name { get; }

	public MemoryContext( string name )
	{
		Name = name;
	}

	public IntPtr StringToCoTaskMemUTF8( string str )
	{
		var ptr = Marshal.StringToCoTaskMemUTF8( str );
		Values.Add( (Type.CoTaskMem, ptr) );

		MemoryLogger.AllocatedBytes( Name, IntPtr.Size );

		return ptr;
	}

	public IntPtr AllocHGlobal( int size )
	{
		var ptr = Marshal.AllocHGlobal( size );
		Values.Add( (Type.HGlobal, ptr) );

		MemoryLogger.AllocatedBytes( Name, IntPtr.Size );

		return ptr;
	}

	public void Dispose()
	{
		foreach ( var value in Values )
		{
			switch ( value.Type )
			{
				case Type.CoTaskMem:
					Marshal.FreeCoTaskMem( value.Pointer );
					break;
				case Type.HGlobal:
					Marshal.FreeHGlobal( value.Pointer );
					break;
			}
		}

		MemoryLogger.FreedBytes( Name, Values.Count * IntPtr.Size );
	}

	public IntPtr GetPtr( IInteropArray arr ) => GetPtr( arr.GetNative() );
	public IntPtr GetPtr( INativeGlue native ) => native.NativePtr;
	public IntPtr GetPtr( string str ) => StringToCoTaskMemUTF8( str );
	public IntPtr GetPtr( int i ) => i;
	public IntPtr GetPtr( uint u ) => (IntPtr)u;
	public IntPtr GetPtr( float f ) => (IntPtr)f;
	public IntPtr GetPtr( bool b ) => b ? new IntPtr( 1 ) : IntPtr.Zero;
	public IntPtr GetPtr( Enum enumValue )
	{
		// FIXME: This will not work for enums that do not fit this size
		return GetPtr( Convert.ToInt32( enumValue ) );
	}
	public IntPtr GetPtr<T>( T value ) where T : unmanaged
	{
		if ( value is Enum enumValue )
			return GetPtr( enumValue );

		var ptr = AllocHGlobal( Marshal.SizeOf<T>() );
		Marshal.StructureToPtr( value, ptr, false );
		return ptr;
	}

	internal string GetString( IntPtr strPtr )
	{
		return Marshal.PtrToStringUTF8( strPtr ) ?? "UNKNOWN";
	}
}

public interface IInteropArray
{
	public Glue.UtilArray GetNative();
}

public class InteropArray<T> : IInteropArray
{
	private Glue.UtilArray _nativeStruct;
	private InteropArray() { }

	public static InteropArray<T> FromArray( T[] array )
	{
		bool isNativeGlue = typeof( T ).GetInterfaces().Contains( typeof( INativeGlue ) );

		int stride, size;

		var interopArray = new InteropArray<T>();
		interopArray._nativeStruct = new();

		if ( isNativeGlue )
			stride = Marshal.SizeOf( typeof( IntPtr ) );
		else
			stride = Marshal.SizeOf( typeof( T ) );

		size = stride * array.Length;

		interopArray._nativeStruct.count = array.Length;
		interopArray._nativeStruct.size = size;

		unsafe
		{
			if ( isNativeGlue )
			{
				fixed ( void* data = array.Select( x => (x as INativeGlue).NativePtr ).ToArray() )
					interopArray._nativeStruct.data = (IntPtr)data;
			}
			else
			{
				fixed ( void* data = array )
					interopArray._nativeStruct.data = (IntPtr)data;
			}
		}

		return interopArray;
	}

	public static InteropArray<T> FromList( List<T> list )
	{
		return FromArray( list.ToArray() );
	}

	public Glue.UtilArray GetNative()
	{
		return _nativeStruct;
	}

	//
	// Implicit conversions to Glue.InteropStruct and from lists/arrays
	//
	public static implicit operator Glue.UtilArray( InteropArray<T> arr ) => arr.GetNative();
	public static implicit operator InteropArray<T>( List<T> list ) => FromList( list );
}

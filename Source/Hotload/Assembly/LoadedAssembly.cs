﻿using Mocha.Hotload;
using System.Reflection;

namespace Mocha;

public class LoadedAssemblyType<T>
{
	private T? managedClass;
	private FileSystemWatcher watcher;
	private LoadedAssemblyInfo assemblyInfo;
	private Assembly assembly;

	public T? Value => managedClass;
	public Assembly Assembly => assembly;

	public LoadedAssemblyType( LoadedAssemblyInfo assemblyInfo )
	{
		this.assemblyInfo = assemblyInfo;

		CompileIntoMemory();
		CreateFileSystemWatcher( assemblyInfo.SourceRoot );
	}

	private void UnloadAssembly()
	{
		managedClass = default;
		assembly = default;
	}

	private void CompileIntoMemory()
	{
		var compileResult = Compiler.Instance.Compile( assemblyInfo );

		if ( !compileResult.WasSuccessful )
		{
			var errorStr = string.Join( '\n', compileResult.Errors );

			foreach ( var error in compileResult.Errors )
			{
				Log.Error( error );
			}

			Common.Notify.AddNotification( $"Build failed", $"Failed to compile project '{assemblyInfo.AssemblyName}'\n{errorStr}" );
			return;
		}

		// Keep old assembly as reference. Should be destroyed once out of scope
		var oldAssembly = assembly;
		var oldGameInterface = managedClass;

		// Unload any loaded assemblies
		UnloadAssembly();

		var newAssembly = compileResult.CompiledAssembly;
		var newInterface = CreateInterfaceFromAssembly( newAssembly );

		// Invoke upgrader to move values from oldAssembly into assembly
		if ( oldAssembly != null && oldGameInterface != null )
		{
			Upgrader.UpgradeInstance( oldGameInterface, newInterface );
		}

		// Now that everything's been upgraded, swap the new interface
		// and assembly in
		assembly = newAssembly;
		managedClass = newInterface;

		Common.Notify.AddNotification( $"Build successful!", $"Compiled '{assemblyInfo.AssemblyName}'!" );
	}

	private T CreateInterfaceFromAssembly( Assembly assembly )
	{
		// Is T an interface?
		if ( typeof( T ).IsInterface )
		{
			// Find first type that derives from interface T
			foreach ( var type in assembly.GetTypes() )
			{
				if ( type.GetInterface( typeof( T ).FullName! ) != null )
				{
					return (T)Activator.CreateInstance( type )!;
				}
			}
		}

		if ( managedClass == null )
		{
			throw new Exception( $"Could not find implementation of '{typeof( T ).Name}'" );
		}

		return default;
	}

	private void CreateFileSystemWatcher( string sourcePath )
	{
		watcher = new FileSystemWatcher( sourcePath, "*.*" );
		watcher.Changed += OnFileChanged;
		watcher.EnableRaisingEvents = true;
	}

	private void OnFileChanged( object sender, FileSystemEventArgs e )
	{
		Log.Trace( $"File {e.FullPath} was changed" );
		CompileIntoMemory();
	}
}

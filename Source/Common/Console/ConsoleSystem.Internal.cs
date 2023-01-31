﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mocha.Common;

public static partial class ConsoleSystem
{
	public static class Internal
	{
		internal static List<string> s_items = new();
		internal static Dictionary<string, Action<List<string>>> s_commandCallbacks = new();

		internal static class ConVarCallbackStore<T>
		{
			public static Dictionary<string, Action<T, T>> Callbacks = new();
		}

		internal static void RegisterCommand( string name, CVarFlags flags, string description, Action<List<string>> callback )
		{
			Glue.ConsoleSystem.RegisterCommand( name, flags, description );
			s_commandCallbacks[name] = callback;
			s_items.Add( name );
		}

		internal static void RegisterStringConVar( string name, string value, CVarFlags flags, string description, Action<string, string> callback )
		{
			Glue.ConsoleSystem.RegisterString( name, value, flags, description );
			ConVarCallbackStore<string>.Callbacks[name] = callback;
			s_items.Add( name );
		}

		internal static void RegisterFloatConVar( string name, float value, CVarFlags flags, string description, Action<float, float> callback )
		{
			Glue.ConsoleSystem.RegisterFloat( name, value, flags, description );
			ConVarCallbackStore<float>.Callbacks[name] = callback;
			s_items.Add( name );
		}

		internal static void RegisterBoolConVar( string name, bool value, CVarFlags flags, string description, Action<bool, bool> callback )
		{
			Glue.ConsoleSystem.RegisterBool( name, value, flags, description );
			ConVarCallbackStore<bool>.Callbacks[name] = callback;
			s_items.Add( name );
		}

		/// <summary>
		/// Register all of the CVars in an assembly
		/// </summary>
		/// <param name="assembly">The assembly to grab from</param>
		/// <param name="extraFlags">Extra flags to force each CVar to have. Used mainly for hotloaded assemblies</param>
		public static void RegisterAssembly( Assembly assembly, CVarFlags extraFlags = CVarFlags.None )
		{
			if ( assembly is null )
				return;

			foreach ( Type type in assembly.GetTypes() )
			{
				foreach ( MethodInfo method in type.GetMethods( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic ) )
				{
					ConCmd.BaseAttribute? customAttribute = method.GetCustomAttribute<ConCmd.BaseAttribute>();
					if ( customAttribute is not null )
					{
						RegisterCommand( customAttribute.Name, customAttribute.Flags | extraFlags, customAttribute.Description,
							( List<string> arguments ) => method.Invoke( null, new object[] { arguments } ) );
					}
				}
			}
		}

		public static void ClearGameCVars()
		{
			foreach ( var name in s_items.ToList() )
			{
				bool exists = Glue.ConsoleSystem.Exists( name );
				CVarFlags flags = Glue.ConsoleSystem.GetFlags( name );

				if ( exists && (flags & CVarFlags.Game) != 0 )
				{
					s_items.Remove( name );
					s_commandCallbacks.Remove( name );
					ConVarCallbackStore<string>.Callbacks.Remove( name );
					ConVarCallbackStore<float>.Callbacks.Remove( name );
					ConVarCallbackStore<bool>.Callbacks.Remove( name );

					Glue.ConsoleSystem.Remove( name );
				}
			}
		}

		public static void DispatchCommand( string name, List<string> arguments )
		{
			if ( s_commandCallbacks.TryGetValue( name, out var callback ) )
			{
				callback( arguments );
			}
		}

		public static void DispatchConVarCallback<T>( string name, T oldValue, T newValue )
		{
			if ( ConVarCallbackStore<T>.Callbacks.TryGetValue( name, out var callback ) )
			{
				callback( oldValue, newValue );
			}
		}
	}
}

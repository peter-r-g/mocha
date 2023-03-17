﻿using System.CodeDom.Compiler;
using System.Collections.Immutable;

namespace MochaTool.InteropGen;

/// <summary>
/// Contains functionality for generating C# code.
/// </summary>
internal static class ManagedCodeGenerator
{
	/// <summary>
	/// The namespace that all generated code will be under.
	/// </summary>
	private const string Namespace = "Mocha.Glue";

	/// <summary>
	/// An array containing all using declarations for generated code.
	/// </summary>
	private static readonly string[] Usings = new[]
	{
		"System.Runtime.InteropServices",
		"System.Runtime.Serialization",
		"Mocha.Common"
	};

	/// <summary>
	/// The header to be used at the top of generated code.
	/// </summary>
	private static string Header => $"""
	//------------------------------------------------------------------------------ 
	// <auto-generated> 
	// This code was generated by a tool. 
	// InteropGen generated on {DateTime.Now}
	// 
	// Changes to this file may cause incorrect behavior and will be lost if 
	// the code is regenerated. 
	// </auto-generated> 
	//------------------------------------------------------------------------------
	""";

	/// <summary>
	/// Generates and returns C# code for a set of <see cref="IUnit"/>s.
	/// </summary>
	/// <param name="units">An enumerable list of <see cref="IUnit"/>s to generate code for.</param>
	/// <returns>C# code representing the set of <see cref="IUnit"/>s passed.</returns>
	internal static string GenerateCode( IEnumerable<IUnit> units )
	{
		var (baseTextWriter, writer) = Utils.CreateWriter();

		// Write header.
		writer.WriteLine( Header );
		writer.WriteLine();

		// Write using statements.
		foreach ( var usingStatement in Usings )
			writer.WriteLine( $"using {usingStatement};" );

		// Write namespace.
		writer.WriteLine();
		writer.WriteLine( $"namespace {Namespace};" );
		writer.WriteLine();

		// Write each unit.
		foreach ( var unit in units )
		{
			switch ( unit )
			{
				case Class c when c.IsNamespace:
					GenerateNamespaceCode( writer, c );
					break;
				case Class c:
					GenerateClassCode( writer, c );
					break;
				case Struct s:
					GenerateStructCode( writer, s );
					break;
				default:
					continue;
			}

			writer.WriteLine();
		}

		return baseTextWriter.ToString();
	}

	/// <summary>
	/// Generates C# code for a class.
	/// </summary>
	/// <param name="writer">The writer to append the code to.</param>
	/// <param name="c">The class to write code for.</param>
	private static void GenerateClassCode( IndentedTextWriter writer, Class c )
	{
		//
		// Gather everything we need into nice lists
		//
		List<string> decls = new();

		foreach ( var method in c.Methods )
		{
			var returnType = Utils.GetManagedType( method.ReturnType );
			var name = method.Name;

			var returnsPointer = Utils.IsPointer( method.ReturnType ) && !method.IsConstructor && !method.IsDestructor;

			if ( returnsPointer )
				returnType = "IntPtr";

			if ( returnType == "string" )
				returnType = "IntPtr"; // Strings are handled specially - they go from pointer to string using InteropUtils.GetString

			if ( method.IsConstructor || method.IsDestructor )
				returnType = "IntPtr"; // Ctor/dtor handled specially too

			var parameterTypes = method.Parameters.Select( x => "IntPtr" ); // Everything gets passed as a pointer
			var paramAndReturnTypes = parameterTypes.Append( returnType );

			if ( !method.IsStatic )
				paramAndReturnTypes = paramAndReturnTypes.Prepend( "IntPtr" ); // Pointer to this class's instance

			var delegateTypeArguments = string.Join( ", ", paramAndReturnTypes );

			var delegateSignature = $"delegate* unmanaged< {delegateTypeArguments} >";

			//
			// With each method, we pass in a pointer to the instance, along with
			// any parameters. The return type is the last type argument passed to
			// the delegate.
			//
			decls.Add( $"private static {delegateSignature} _{name} = ({delegateSignature})Mocha.Common.Global.UnmanagedArgs.__{c.Name}_{name}MethodPtr;" );
		}

		//
		// Write shit
		//
		writer.WriteLine( $"public unsafe class {c.Name} : INativeGlue" );
		writer.WriteLine( "{" );
		writer.Indent++;

		writer.WriteLine( "public IntPtr NativePtr { get; set; }" );

		// Decls
		writer.WriteLine();
		foreach ( var decl in decls )
			writer.WriteLine( decl );
		writer.WriteLine();

		// Ctor
		if ( c.Methods.Any( x => x.IsConstructor ) )
		{
			var ctor = c.Methods.First( x => x.IsConstructor );
			var managedCtorArgs = string.Join( ", ", ctor.Parameters.Select( x => $"{Utils.GetManagedType( x.Type )} {x.Name}" ) );

			writer.WriteLine( $"public {c.Name}( {managedCtorArgs} )" );
			writer.WriteLine( "{" );
			writer.Indent++;

			var ctorCallArgs = string.Join( ", ", ctor.Parameters.Select( x => x.Name ) );
			writer.WriteLine( $"this.NativePtr = this.Ctor( {ctorCallArgs} );" );

			writer.Indent--;
			writer.WriteLine( "}" );
		}

		// Methods
		foreach ( var method in c.Methods )
		{
			writer.WriteLine();

			//
			// Gather function signature
			//
			// Call parameters as comma-separated string
			var managedCallParams = string.Join( ", ", method.Parameters.Select( x => $"{Utils.GetManagedType( x.Type )} {x.Name}" ) );
			var name = method.Name;

			// We return a pointer to the created object if it's a ctor/dtor, but otherwise we'll do auto-conversions to our managed types
			var returnType = (method.IsConstructor || method.IsDestructor) ? "IntPtr" : Utils.GetManagedType( method.ReturnType );

			var returnsPointer = Utils.IsPointer( method.ReturnType ) && !method.IsConstructor && !method.IsDestructor;

			// If this is a ctor or dtor, we don't want to be able to call the method manually
			var accessLevel = (method.IsConstructor || method.IsDestructor) ? "private" : "public";

			if ( method.IsStatic )
				accessLevel += " static";

			// Write function signature
			writer.WriteLine( $"{accessLevel} {returnType} {name}( {managedCallParams} ) " );
			writer.WriteLine( "{" );
			writer.Indent++;

			// Spin up a MemoryContext instance
			writer.WriteLine( $"using var ctx = new MemoryContext( \"{c.Name}.{name}\" );" );

			//
			// Gather function body
			//
			var paramsAndInstance = method.Parameters;

			// We need to pass the instance in if this is not a static method
			if ( !method.IsStatic )
				paramsAndInstance = paramsAndInstance.Prepend( new Variable( "NativePtr", "IntPtr" ) ).ToImmutableArray();

			// Gather function call arguments. Make sure that we're passing in a pointer for everything
			var paramNames = paramsAndInstance.Select( x => "ctx.GetPtr( " + x.Name + " )" );

			// Function call arguments as comma-separated string
			var functionCallArgs = string.Join( ", ", paramNames );

			if ( returnsPointer )
			{
				// If we want to return a pointer:
				writer.WriteLine( $"var ptr = _{name}( {functionCallArgs} );" );
				writer.WriteLine( $"var obj = FormatterServices.GetUninitializedObject( typeof( {returnType} ) ) as {returnType};" );
				writer.WriteLine( $"obj.NativePtr = ptr;" );
				writer.WriteLine( $"return obj;" );
			}
			else
			{
				// If we want to return a value:
				if ( returnType != "void" )
					writer.Write( "return " );

				// This is a pretty dumb and HACKy way of handling strings
				if ( returnType == "string" )
					writer.Write( "ctx.GetString( " );

				// Call the function..
				writer.Write( $"_{name}( {functionCallArgs} )" );

				// Finish string
				if ( returnType == "string" )
					writer.Write( ")" );

				writer.WriteLine( ";" );
			}

			writer.Indent--;
			writer.WriteLine( "}" );
		}

		writer.Indent--;
		writer.WriteLine( "}" );
	}

	/// <summary>
	/// Generates C# code for a struct.
	/// </summary>
	/// <param name="writer">The writer to append the code to.</param>
	/// <param name="s">The struct to write code for.</param>
	private static void GenerateStructCode( IndentedTextWriter writer, Struct s )
	{
		writer.WriteLine( $"[StructLayout( LayoutKind.Sequential )]" );
		writer.WriteLine( $"public struct {s.Name}" );
		writer.WriteLine( "{" );
		writer.Indent++;

		foreach ( var field in s.Fields )
			writer.WriteLine( $"public {Utils.GetManagedType( field.Type )} {field.Name};" );

		writer.Indent--;
		writer.WriteLine( "}" );
	}

	/// <summary>
	/// Generates C# code for a namespace.
	/// </summary>
	/// <param name="writer">The writer to append the code to.</param>
	/// <param name="ns">The namespace to write code for.</param>
	private static void GenerateNamespaceCode( IndentedTextWriter writer, Class ns )
	{
		//
		// Gather everything we need into nice lists
		//
		List<string> decls = new();

		foreach ( var method in ns.Methods )
		{
			var returnType = Utils.GetManagedType( method.ReturnType );
			var name = method.Name;

			var returnsPointer = Utils.IsPointer( method.ReturnType ) && !method.IsConstructor && !method.IsDestructor;

			if ( returnType == "string" || returnsPointer )
				returnType = "IntPtr"; // Strings are handled specially - they go from pointer to string using InteropUtils.GetString

			var parameterTypes = method.Parameters.Select( x => "IntPtr" ); // Everything gets passed as a pointer
			var paramAndReturnTypes = parameterTypes.Append( returnType );

			var delegateTypeArguments = string.Join( ", ", paramAndReturnTypes );

			var delegateSignature = $"delegate* unmanaged< {delegateTypeArguments} >";

			//
			// With each method, we pass in a pointer to the instance, along with
			// any parameters. The return type is the last type argument passed to
			// the delegate.
			//
			decls.Add( $"private static {delegateSignature} _{name} = ({delegateSignature})Mocha.Common.Global.UnmanagedArgs.__{ns.Name}_{name}MethodPtr;" );
		}

		//
		// Write shit
		//
		writer.WriteLine( $"public static unsafe class {ns.Name}" );
		writer.WriteLine( "{" );
		writer.Indent++;

		writer.WriteLine();
		foreach ( var decl in decls )
			writer.WriteLine( decl );

		// Methods
		foreach ( var method in ns.Methods )
		{
			writer.WriteLine();

			var managedCallParams = string.Join( ", ", method.Parameters.Select( x => $"{Utils.GetManagedType( x.Type )} {x.Name}" ) );
			var name = method.Name;
			var returnType = Utils.GetManagedType( method.ReturnType );
			var accessLevel = (method.IsConstructor || method.IsDestructor) ? "private" : "public";
			var returnsPointer = Utils.IsPointer( method.ReturnType ) && !method.IsConstructor && !method.IsDestructor;

			writer.WriteLine( $"{accessLevel} static {returnType} {name}( {managedCallParams} ) " );
			writer.WriteLine( "{" );
			writer.Indent++;

			// Spin up a MemoryContext instance
			writer.WriteLine( $"using var ctx = new MemoryContext( \"{ns.Name}.{name}\" );" );

			var paramNames = method.Parameters.Select( x => "ctx.GetPtr( " + x.Name + " )" );
			var functionCallArgs = string.Join( ", ", paramNames );

			if ( returnsPointer )
			{
				// If we want to return a pointer:
				writer.WriteLine( $"var ptr = _{name}( {functionCallArgs} );" );
				writer.WriteLine( $"var obj = FormatterServices.GetUninitializedObject( typeof( {returnType} ) ) as {returnType};" );
				writer.WriteLine( $"obj.instance = ptr;" );
				writer.WriteLine( $"return obj;" );
			}
			else
			{
				// If we want to return a value:
				if ( returnType != "void" )
					writer.Write( "return " );

				// This is a pretty dumb and HACKy way of handling strings
				if ( returnType == "string" )
					writer.Write( "ctx.GetString( " );

				// Call the function..
				writer.Write( $"_{name}( {functionCallArgs} )" );

				// Finish string
				if ( returnType == "string" )
					writer.Write( ")" );

				writer.WriteLine( ";" );
			}

			writer.Indent--;
			writer.WriteLine( "}" );
		}

		writer.Indent--;
		writer.WriteLine( "}" );
	}
}

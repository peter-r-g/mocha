﻿using MochaTool.InteropGen.Parsing;
using System.CodeDom.Compiler;
using System.Collections.Immutable;

namespace MochaTool.InteropGen.CodeGen;

/// <summary>
/// Contains functionality for generating C++ code.
/// </summary>
internal static class NativeCodeGenerator
{
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
	/// Generates and returns C++ code for a set of <see cref="IUnit"/>s.
	/// </summary>
	/// <param name="headerPath">The path to the header file that contained the units.</param>
	/// <param name="units">An enumerable list of <see cref="IUnit"/>s to generate code for.</param>
	/// <returns>C++ code representing the set of <see cref="IUnit"/>s passed.</returns>
	internal static string GenerateCode( string headerPath, IEnumerable<IUnit> units )
	{
		var (baseTextWriter, writer) = Utils.CreateWriter();

		writer.WriteLine( Header );
		writer.WriteLine();

		writer.WriteLine( "#pragma once" );
		writer.WriteLine( $"#include \"{Path.Combine( "..", headerPath )}\"" );

		writer.WriteLine();

		foreach ( var unit in units )
		{
			if ( unit is Namespace n )
				GenerateNamespaceCode( writer, n );
			else if ( unit is Class c )
				GenerateClassCode( writer, c );
			else
				continue;

			writer.WriteLine();
		}

		return baseTextWriter.ToString();
	}

	/// <summary>
	/// Generates C++ code for a class.
	/// </summary>
	/// <param name="writer">The writer to append the code to.</param>
	/// <param name="c">The class to write code for.</param>
	private static void GenerateClassCode( IndentedTextWriter writer, Class c )
	{
		foreach ( var method in c.Methods )
		{
			var args = method.Parameters;

			if ( !method.IsStatic )
				args = args.Prepend( new Variable( "instance", $"{c.Name}*" ) ).ToImmutableArray();

			var argStr = string.Join( ", ", args.Select( x =>
			{
				if ( x.Type == "std::string" )
					return $"const char* {x.Name}";

				return $"{x.Type} {x.Name}";
			} ) );

			var signature = $"extern \"C\" inline {method.ReturnType} __{c.Name}_{method.Name}( {argStr} )";
			var body = "";
			var parameters = string.Join( ", ", method.Parameters.Select( x => x.Name ) );

			if ( method.IsConstructor )
				body += $"return new {c.Name}( {parameters} );";
			else if ( method.IsDestructor )
				body += $"instance->~{c.Name}( {parameters} );";
			else
			{
				var accessor = method.IsStatic ? $"{c.Name}::" : "instance->";

				if ( method.ReturnType == "void" )
					body += $"{accessor}{method.Name}( {parameters} );";
				else if ( method.ReturnType == "std::string" )
					body += $"std::string text = {accessor}{method.Name}( {parameters} );\r\nconst char* cstr = text.c_str();\r\nchar* dup = _strdup(cstr);\r\nreturn dup;";
				else
					body += $"return {accessor}{method.Name}( {parameters} );";
			}

			writer.WriteLine( signature );
			writer.WriteLine( "{" );
			writer.Indent++;

			writer.WriteLine( body );

			writer.Indent--;
			writer.WriteLine( "}" );
		}
	}

	/// <summary>
	/// Generates C++ code for a namespace.
	/// </summary>
	/// <param name="writer">The writer to append the code to.</param>
	/// <param name="ns">The namespace to write code for.</param>
	private static void GenerateNamespaceCode( IndentedTextWriter writer, Namespace ns )
	{
		foreach ( var method in ns.Methods )
		{
			var args = method.Parameters;

			var argStr = string.Join( ", ", args.Select( x =>
			{
				if ( x.Type == "std::string" )
					return $"const char* {x.Name}";

				return $"{x.Type} {x.Name}";
			} ) );

			var signature = $"extern \"C\" inline {method.ReturnType} __{ns.Name}_{method.Name}( {argStr} )";
			var body = "";
			var parameters = string.Join( ", ", method.Parameters.Select( x => x.Name ) );

			var accessor = $"{ns.Name}::";

			if ( method.ReturnType == "void" )
				body += $"{accessor}{method.Name}( {parameters} );";
			else if ( method.ReturnType == "std::string" )
				body += $"std::string text = {accessor}{method.Name}( {parameters} );\r\nconst char* cstr = text.c_str();\r\nchar* dup = _strdup(cstr);\r\nreturn dup;";
			else
				body += $"return {accessor}{method.Name}( {parameters} );";

			writer.WriteLine( signature );
			writer.WriteLine( "{" );
			writer.Indent++;

			writer.WriteLine( body );

			writer.Indent--;
			writer.WriteLine( "}" );
		}
	}
}

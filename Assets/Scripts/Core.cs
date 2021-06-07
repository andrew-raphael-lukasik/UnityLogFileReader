using System.Collections.Generic;

using IO = System.IO;
using StringBuilder = System.Text.StringBuilder;

public static class Core
{


	public static bool ReadFromCommandLineArgs ( out string[] rawLines , out string filePath )
	{
		// read log file:
		foreach( string argument in System.Environment.GetCommandLineArgs() )
		if( IO.Path.GetExtension(argument)==".log" && IO.File.Exists(argument) )
		{
			rawLines = WriteSafeReadAllLines( argument );
			filePath = argument;
			return true;
		}

		// fallback: fill array with debug messages:
		List<string> debugMessages = new List<string>();
		debugMessages.Add( "No log file path provided/recognised in execution arguments." );
		debugMessages.Add( "CommandLineArgs:" );
		foreach( string argument in System.Environment.GetCommandLineArgs() )
			debugMessages.Add( $"\t\"{argument}\"" );
		rawLines = debugMessages.ToArray();
		filePath = string.Empty;
		return false;
	}


	public static Entry[] ProcessRawLines ( string[] rawLines )
	{
		List<string> list = new List<string>();
		{
			var buffer = new StringBuilder();
			for( int i=0 ; i<rawLines.Length ; i++ )
			{
				string line = rawLines[i];
				if( !string.IsNullOrEmpty(line) )
				{
					if( !( line.StartsWith("(Filename:") || line.StartsWith("[C:") ) )
						buffer.AppendLine( line );
					else if( list.Count!=0 )
						list[list.Count-1] = $"{list[list.Count-1]}\r\n{line}";
				}
				else if( buffer.Length!=0 )
				{
					RemoveLastLineEnding( buffer );
					list.Add( buffer.ToString() );
					buffer.Clear();
				}
			}
			if( buffer.Length!=0 )
			{
				RemoveLastLineEnding( buffer );
				list.Add( buffer.ToString() );
			}
		}
		
		List<Entry> entriesList = new List<Entry>( capacity:list.Count );
		if( list.Count!=0 )
		{
			string current = null;
			int currentHash = -1;
			int count = -1;
			for( int i=0 ; i<list.Count ; i++ )
			{
				string next = list[i];
				int nextHash = next.GetHashCode();
				if( nextHash==currentHash )
					count++;
				else
				{
					if( !string.IsNullOrEmpty(current) )
						entriesList.Add( new Entry{ text=current , count=count } );
					current = list[i];
					currentHash = current.GetHashCode();
					count = 1;
					currentHash = nextHash;
				}
			}
			if( !string.IsNullOrEmpty(current) )
				entriesList.Add( new Entry{ text=current , count=count } );
		}

		return entriesList.ToArray();
	}


	public static string[] WriteSafeReadAllLines ( string path )
	{
		using( var csv = new IO.FileStream( path , IO.FileMode.Open , IO.FileAccess.Read , IO.FileShare.ReadWrite ) )
		using( var sr = new IO.StreamReader(csv) )
		{
			List<string> file = new List<string>();
			while( !sr.EndOfStream )
				file.Add( sr.ReadLine() );
			return file.ToArray();
		}
	}


	static void RemoveLastLineEnding ( StringBuilder sb )
	{
		if( sb[sb.Length-1]=='\n' ) sb.Remove( sb.Length-1 , 1 );
		if( sb[sb.Length-1]=='\r' ) sb.Remove( sb.Length-1 , 1 );
	}


}

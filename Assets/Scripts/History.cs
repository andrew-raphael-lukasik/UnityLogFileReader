using System.Collections.Generic;
using UnityEngine;

using IO = System.IO;

public class History
{

	public static string GetFilePath () => IO.Path.Combine( Application.persistentDataPath , "file_history.txt" );

	public static void Update ( string path )
	{
		List<string> history = new List<string>( Read() );
		int pos = history.IndexOf(path);
		if( pos!=0 )
		{
			if( pos!=-1 ) history.RemoveAt( pos );
			history.Insert( 0 , path );
			Write( history.ToArray() );
		}
	}

	public static string[] Read ()
	{
		string filePath = GetFilePath();
		return IO.File.Exists(filePath) ? IO.File.ReadAllLines(filePath) : new string[0];
	}

	public static void Write ( string[] array )
	{
		const int k_length_limit = 16;
		if( array.Length>k_length_limit )
			System.Array.Resize( ref array , k_length_limit );
		IO.File.WriteAllLines( GetFilePath() , array );
	}

}

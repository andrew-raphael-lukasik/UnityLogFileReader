using System.Collections.Generic;
using UnityEngine;

public class History
{


	public const string k_file_history_key = "file_history";


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
		string json = PlayerPrefs.GetString( k_file_history_key , JsonUtility.ToJson( new HistoryData{ Values=new string[0] } ) );
		return JsonUtility.FromJson<HistoryData>(json).Values;
	}

	public static void Write ( string[] array )
	{
		const int k_length_limit = 16;
		if( array.Length>k_length_limit )
			System.Array.Resize( ref array , k_length_limit );
		string json = JsonUtility.ToJson( new HistoryData{ Values=array } );
		PlayerPrefs.SetString( k_file_history_key , json );
	}


	[System.Serializable]
	public struct HistoryData
	{
		public string[] Values;
	}


}

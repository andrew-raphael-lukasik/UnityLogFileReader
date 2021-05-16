using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

using IO = System.IO;

[ExecuteAlways]
[RequireComponent( typeof(UIDocument) )]
public class MainPanelController : MonoBehaviour
{
	

	EntryListView _list = null;
	Label _filePath = null;


	void Awake ()
	{
		Bind();
		InvokeRepeating( nameof(Tick) , time:0 , repeatRate:Mathf.Sqrt(2) );
	}


	void Tick ()
	{
		bool readSuccess = ReadRawLines( out string[] rawLines , out string filePath );
		_list.itemsSource = ProcessRawLines( rawLines );
		_filePath.text = filePath;

		#if UNITY_EDITOR
		if( !readSuccess )
		{
			// read any example of a log file, to make build preview ui from:
			try
			{
				string logsDirectory = IO.Path.Combine( Application.dataPath.Replace("/Assets","") , "Logs/" );
				if( IO.Directory.Exists(logsDirectory) )
				{
					string[] files = IO.Directory.GetFiles(logsDirectory);
					string path = files[Random.Range(0,files.Length)];
					rawLines = WriteSafeReadAllLines( path );
					_list.itemsSource = ProcessRawLines( rawLines );
				}
			}
			catch( System.Exception ex ) { Debug.LogException(ex); }
		}
		#endif
	}


	void Bind ()
	{
		var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
		if( rootVisualElement!=null )
		{
			_list = rootVisualElement.Q<EntryListView>();
			rootVisualElement.Add( _list );
			_filePath = rootVisualElement.Q<Label>( "file_path" );
		}
	}


	bool ReadRawLines ( out string[] rawLines , out string filePath )
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
		debugMessages.Add(string.Empty);
		foreach( string argument in System.Environment.GetCommandLineArgs() )
		{
			debugMessages.Add( $"\t\"{argument}\"" );
			debugMessages.Add(string.Empty);
		}
		rawLines = debugMessages.ToArray();
		filePath = "N/A";
		return false;
	}


	Entry[] ProcessRawLines ( string[] rawLines )
	{
		List<string> list = new List<string>();
		var sb = new System.Text.StringBuilder();
		foreach( string line in rawLines )
		{
			if( !string.IsNullOrEmpty(line) )
			{
				if( line[0]!='[' )
					sb.AppendLine( line );
				else if( list.Count!=0 )
					list[list.Count-1] += line;
			}
			else if( sb.Length!=0 )
			{
				list.Add( sb.ToString() );
				sb.Clear();
			}
		}
		if( sb.Length!=0 )
		{
			list.Add( sb.ToString() );
			sb.Clear();
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


	string[] WriteSafeReadAllLines ( string path )
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


}

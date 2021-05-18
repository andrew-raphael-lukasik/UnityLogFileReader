using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using IO = System.IO;

[ExecuteAlways]
[RequireComponent( typeof(UIDocument) )]
public class MainPanelController : MonoBehaviour
{
	
	EntryListView _listView = null;
	HistoryListView _historyView = null;
	Button _historyClear = null;
	
	TextField _filePathField = null;
	IO.FileSystemWatcher _filePathWatcher = null;


	void Awake ()
	{
		Bind();
		
		bool readSuccess = ReadFromCommandLineArgs( out string[] rawLines , out string path );
		UpdateListView( rawLines );
		if( readSuccess )
			WatchFile( path );

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
					string editorLogPath = files[Random.Range(0,files.Length)];
					WatchFile( editorLogPath );
					UpdateListView( editorLogPath );
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
			// entry list view:
			_listView = rootVisualElement.Q<EntryListView>();

			// file path label:
			_filePathField = rootVisualElement.Q<TextField>( "file_path" );

			// history list view:
			_historyView = rootVisualElement.Q<HistoryListView>();
			if( _historyView!=null )
			{
				_historyView.onClicked += (path) =>
				{
					WatchFile( path );
					UpdateListView( path );
				};
			}

			// history clear button:
			_historyClear = rootVisualElement.Q<Button>( "history_clear" );
			_historyClear.clicked += () => {
				var empty = new string[0];
				History.Write( empty );
				_historyView.itemsSource = empty;
			};
		}
	}


	public void UpdateListView ( string[] rawLines ) => _listView.itemsSource = ProcessRawLines( rawLines );
	public void UpdateListView ( string path ) => UpdateListView( WriteSafeReadAllLines( path ) );


	void WatchFile ( string path )
	{
		if( _filePathWatcher!=null )
			_filePathWatcher.Dispose();
		
		_filePathWatcher = new IO.FileSystemWatcher();
		_filePathWatcher.Path = IO.Path.GetDirectoryName(path);
		_filePathWatcher.Filter = IO.Path.GetFileName(path);
		_filePathWatcher.NotifyFilter = IO.NotifyFilters.LastWrite;
		_filePathWatcher.EnableRaisingEvents = true;
		_filePathWatcher.Changed += (sender,e) => {
			UpdateListView( path );
		};

		_filePathField.SetValueWithoutNotify( path );

		History.Update( path );
		_historyView.itemsSource = History.Read();
	}


	static bool ReadFromCommandLineArgs ( out string[] rawLines , out string filePath )
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


	static Entry[] ProcessRawLines ( string[] rawLines )
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


	static string[] WriteSafeReadAllLines ( string path )
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

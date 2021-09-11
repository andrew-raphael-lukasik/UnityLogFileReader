using UnityEngine;
using UnityEngine.UIElements;

using IO = System.IO;

[ExecuteAlways]
[RequireComponent( typeof(UIDocument) )]
public class MainPanelController : MonoBehaviour
{
	
	EntryListView _entriesView = null;
	HistoryListView _historyView = null;
	Button _historyClear = null;
	
	TextField _filePathField = null;
	IO.FileSystemWatcher _filePathWatcher = null;


	void Awake ()
	{
		Bind();
		
		bool readSuccess = Core.ReadFromCommandLineArgs( out string[] rawLines , out string path );
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
			_entriesView = rootVisualElement.Q<EntryListView>();

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
					Foldout foldout = _historyView.parent as Foldout;
					if( foldout!=null ) foldout.value = false;
					else Debug.LogWarning("parent Foldout not found");
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


	public void UpdateListView ( string[] rawLines ) => _entriesView.itemsSource = Core.ProcessRawLines( rawLines );
	public void UpdateListView ( string path ) => UpdateListView( Core.WriteSafeReadAllLines( path ) );


	void WatchFile ( string path )
	{
		// this block started throwing
		// "PlatformNotSupportedException: Operation is not supported on this platform."
		// on standalone builds
		try
		{
			if( _filePathWatcher!=null ) _filePathWatcher.Dispose();
			_filePathWatcher = new IO.FileSystemWatcher();
			_filePathWatcher.Path = IO.Path.GetDirectoryName(path);
			_filePathWatcher.Filter = IO.Path.GetFileName(path);
			_filePathWatcher.NotifyFilter = IO.NotifyFilters.LastWrite;
			_filePathWatcher.EnableRaisingEvents = true;
			_filePathWatcher.Changed += (sender,e) => UpdateListView(path);
		}
		catch( System.Exception ex )
		{
			Debug.LogException(ex);
		}

		History.Update( path );
		_historyView.itemsSource = History.Read();

		_filePathField.SetValueWithoutNotify( path );
	}


}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

using IO = System.IO;

[ExecuteAlways]
[RequireComponent( typeof(UIDocument) )]
public class MainPanelController : MonoBehaviour
{
	

	ListView _list = null;


	void Awake () => InvokeRepeating( nameof(Tick) , time:0 , repeatRate:Mathf.Sqrt(2) );
	
	
	#if UNITY_EDITOR
	void OnEnable () => Tick();
	#endif


	void Tick ()
	{
		if( ListViewExists() )
		{
			bool readSuccess = ReadRawLines( out string[] rawLines );
			_list.itemsSource = ProcessRawLines( rawLines );

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
	}


	bool ListViewExists ()
	{
		if( _list!=null )
			return true;
		else
		{
			var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
			if( rootVisualElement!=null )
			{
				rootVisualElement.Clear();
				_list = CreateMainListView();
				rootVisualElement.Add( _list );
				return true;
			}
			else
				return false;
		}
	}


	bool ReadRawLines ( out string[] rawLines )
	{
		// read log file:
		foreach( string argument in System.Environment.GetCommandLineArgs() )
		if( IO.Path.GetExtension(argument)==".log" && IO.File.Exists(argument) )
		{
			rawLines = WriteSafeReadAllLines( argument );
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
		return false;
	}


	ListView CreateMainListView ()
	{
		var LISTVIEW = new ListView();
		{
			var style = LISTVIEW.style;
			style.minHeight = 300;
			style.flexGrow = 1;
		}
		{
			LISTVIEW.itemHeight = 120;
			LISTVIEW.makeItem = () => {
				VisualElement root = new VisualElement();
					root.style.flexDirection = FlexDirection.RowReverse;
				
				var scrollView = new ScrollView();
					scrollView.style.width = new Length( 95 , LengthUnit.Percent );
				var mainLabel = new Label();
					mainLabel.enableRichText = true;
				scrollView.Add( mainLabel );
				root.Add( scrollView );

				var repeatsLabel = new Label();
					repeatsLabel.style.width = new Length( 5 , LengthUnit.Percent );
					repeatsLabel.tooltip = "Number of repeats.";
					repeatsLabel.displayTooltipWhenElided = true;
					repeatsLabel.focusable = true;
				root.Add( repeatsLabel );
				
				return root;
			};
			LISTVIEW.bindItem = (root,i) =>
			{
				Entry entry = (Entry) LISTVIEW.itemsSource[i];
				ScrollView scrollView = (ScrollView) root[0];
				Label mainLabel = (Label) scrollView[0];
				{
					var style = mainLabel.style;
					style.textOverflow = TextOverflow.Ellipsis;
					Color.RGBToHSV( TextToColor(entry.text) , out float h  , out float s , out float v );
					style.backgroundColor = Color.HSVToRGB( h , 0.5f , 0.8f );
				}
				mainLabel.text = entry.text;
				Label repeatsLabel = (Label) root[1];
				if( entry.count!=1 )
				{
					repeatsLabel.text = $"{entry.count}x";
					repeatsLabel.visible = true;
				}
				else
				{
					repeatsLabel.visible = false;
				}
			};
			LISTVIEW.onSelectionChange += (obj)=> GUIUtility.systemCopyBuffer = ((Entry) obj.FirstOrDefault()).text;
		}
		return LISTVIEW;
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


	Color TextToColor ( string text )
	{
		var md5 = System.Security.Cryptography.MD5.Create();
		var bytes = md5.ComputeHash( System.Text.Encoding.UTF8.GetBytes(text) );
		md5.Dispose();
		var color = new Color32( bytes[0] , bytes[1] , bytes[2] , 255 );
		return color;
	}


	struct Entry
	{
		public string text;
		public int count;
	}


}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

using IO = System.IO;
using Regex = System.Text.RegularExpressions.Regex;

[ExecuteAlways]
[RequireComponent( typeof(UIDocument) )]
public class MainPanelController : MonoBehaviour
{

	
	void OnEnable () => BindUI();


	void BindUI ()
	{
		var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
		if( rootVisualElement!=null )
		{
			rootVisualElement.Clear();
			CreateUI( rootVisualElement );
		}
	}


	void CreateUI ( VisualElement rootVisualElement )
	{
		string[] rawLines = null;

		#if UNITY_EDITOR
		// read any example of a log file, to make build preview ui from:
		try
		{
			string logsDirectory = IO.Path.Combine( Application.dataPath.Replace("/Assets","") , "Logs/" );
			if( IO.Directory.Exists(logsDirectory) )
			{
				string[] files = IO.Directory.GetFiles(logsDirectory);
				string path = files[Random.Range(0,files.Length)];
				rawLines = WriteSafeReadAllLines( path );
			}
		}
		catch( System.Exception ex ) { Debug.LogException(ex); }
		#endif

		// read log file:
		foreach( string argument in System.Environment.GetCommandLineArgs() )
		{
			if( IO.Path.GetExtension(argument)==".log" && IO.File.Exists(argument) )
			{
				rawLines = WriteSafeReadAllLines( argument );
				break;
			}
		}
		if( rawLines==null )
		{
			rootVisualElement.Add( new Label("No log file path provided/recognised in execution arguments.") );
			var ARGUMENTS = new ListView();
			{
				var style = ARGUMENTS.style;
				style.minHeight = 300;
				style.flexGrow = 1;
			}
			{
				ARGUMENTS.itemsSource = rawLines;
				ARGUMENTS.itemHeight = 20;
				ARGUMENTS.makeItem = () => new Label();
				ARGUMENTS.bindItem = (ve,i) => ((Label)ve).text = (string) ARGUMENTS.itemsSource[i];
			}
			rootVisualElement.Add( ARGUMENTS );
			return;
		}

		(string text,int count)[] entries = null;
		if( rawLines!=null )
		{
			List<string> list = new List<string>();
			var sb = new System.Text.StringBuilder();
			foreach( string line in rawLines )
			{
				if( !string.IsNullOrEmpty(line) )
				{
					if( line[0]!='[' )
						sb.AppendLine( line );
					else
					{
						if( list.Count!=0 )
							list[list.Count-1] += line;
					}
				}
				else if( sb.Length!=0 )
				{
					list.Add( sb.ToString() );
					sb.Clear();
				}
			}
			
			List<(string text,int count)> shorterList = new List<(string,int)>( capacity:list.Count );
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
					{
						count++;
					}
					else
					{
						if( !string.IsNullOrEmpty(current) )
							shorterList.Add( ( current , count ) );
						current = list[i];
						currentHash = current.GetHashCode();
						count = 1;
						currentHash = nextHash;
					}
				}
			}

			entries = shorterList.ToArray();
		}

		// create log view ui:
		var LOG_LINES = new ListView();
		{
			var style = LOG_LINES.style;
			style.minHeight = 300;
			style.flexGrow = 1;
		}
		{
			// 	string[] text_lines = Regex.Split( text , "\r\n|\r|\n" );
			LOG_LINES.itemsSource = entries;
			LOG_LINES.itemHeight = 120;
			LOG_LINES.makeItem = () => {
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
			LOG_LINES.bindItem = (root,i) =>
			{
				(string text,int count) entry = ((string,int)) LOG_LINES.itemsSource[i];
				ScrollView scrollView = (ScrollView) root[0];
				Label mainLabel = (Label) scrollView[0];
				{
					var style = mainLabel.style;
					style.textOverflow = TextOverflow.Ellipsis;
					style.backgroundColor = Color.HSVToRGB( (float)new System.Random(entry.text.GetHashCode()).NextDouble() , 0.5f , 0.8f );
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
			LOG_LINES.onSelectionChange += (obj)=> GUIUtility.systemCopyBuffer = (((string text,int count)) obj.FirstOrDefault()).text;
		}
		rootVisualElement.Add( LOG_LINES );
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

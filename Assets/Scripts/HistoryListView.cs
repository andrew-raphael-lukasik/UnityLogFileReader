using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

using IO = System.IO;

[UnityEngine.Scripting.Preserve]
public class HistoryListView : VisualElement
{

	ListView _listView;

	public string[] itemsSource {
		get => _listView.itemsSource as string[];
		set
		{
			_listView.itemsSource = value;
			_listView.Rebuild();
		}
	}


	public event System.Action<string> onClicked = (s) => {};

	public new class UxmlFactory : UxmlFactory<HistoryListView,UxmlTraits> {}
	public new class UxmlTraits : VisualElement.UxmlTraits
	{
		public override void Init ( VisualElement ve , IUxmlAttributes bag , CreationContext cc )
		{
			base.Init( ve , bag , cc );
			HistoryListView ROOT = ve as HistoryListView;

			string[] items = History.Read();
			#if UNITY_EDITOR
			if( items.Length==0 ) items = new string[]{ "lorem ipsum" , "lorem ipsum" , "lorem ipsum" , "lorem ipsum" };
			#endif

			var LISTVIEW = new ListView();
			LISTVIEW.style.flexGrow = 1;
			{
				LISTVIEW.fixedItemHeight = 20;
				LISTVIEW.itemsSource = items;
				LISTVIEW.makeItem = () => new Label();
				LISTVIEW.bindItem = (item,i) =>
				{
					string path = (string) LISTVIEW.itemsSource[i];
					Label label = (Label) item;
					label.text = path;
					label.SetEnabled( IO.File.Exists(path) );
				};

				// on selection change:
				LISTVIEW.onSelectionChange += (obj)=>
				{
					string path = (string) obj.FirstOrDefault();
					ROOT.onClicked( path );
				};
			}
			
			ROOT.Add( LISTVIEW );
			ROOT._listView = LISTVIEW;
			{
				var style = ROOT.style;
				style.minHeight = LISTVIEW.fixedItemHeight * 4.5f;
			}
		}

	}
	

}

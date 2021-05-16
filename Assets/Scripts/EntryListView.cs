using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EntryListView : VisualElement
{

	ListView _listView;

	public Entry[] itemsSource {
		get => _listView.itemsSource as Entry[];
		set => _listView.itemsSource = value;
	}


	public new class UxmlFactory : UxmlFactory<EntryListView,UxmlTraits> {}
	public new class UxmlTraits : VisualElement.UxmlTraits
	{

		public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription { get { yield break; } }

		public override void Init ( VisualElement ve , IUxmlAttributes bag , CreationContext cc )
		{
			base.Init( ve , bag , cc );
			EntryListView obj = ve as EntryListView;
			// obj.Clear();
			
			var LISTVIEW = new ListView();
			LISTVIEW.style.flexGrow = 1;
			{
				#if UNITY_EDITOR
				LISTVIEW.itemsSource = _loremIpsum;
				#endif
				
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
						repeatsLabel.tooltip = "Number of repetitions.";
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
			obj.Add( LISTVIEW );
			obj._listView = LISTVIEW;
		}

		Color TextToColor ( string text )
		{
			var md5 = System.Security.Cryptography.MD5.Create();
			var bytes = md5.ComputeHash( System.Text.Encoding.UTF8.GetBytes(text) );
			md5.Dispose();
			var color = new Color32( bytes[0] , bytes[1] , bytes[2] , 255 );
			return color;
		}

		#if UNITY_EDITOR
		readonly Entry[] _loremIpsum = new Entry[]{
			new Entry{ text="Lorem ipsum dolor sit amet, consectetur adipiscing elit." , count=1 } ,
			new Entry{ text="Vestibulum efficitur rutrum condimentum. Vestibulum a tortor in tellus finibus consectetur." , count=1 } ,
			new Entry{ text="Fusce sagittis diam et ante volutpat vulputate." , count=1 } ,
			new Entry{ text="Nam pretium rhoncus odio, vel ultricies ex sagittis in. Vivamus imperdiet quis eros sit amet pellentesque." , count=1 } ,
			new Entry{ text="Quisque velit arcu, ornare vel blandit at, accumsan nec lacus, ullamcorper auctor tortor ligula ut sem." , count=1 } ,
			new Entry{ text="Ut tincidunt sapien ut urna volutpat, at laoreet felis rhoncus." , count=1 } ,
			new Entry{ text="Nunc consequat, magna sed hendrerit hendrerit, ex purus viverra dolor, id tristique sapien dolor id felis." , count=1 } ,
			new Entry{ text="In interdum nisl ut ipsum eleifend cursus." , count=1 } ,
			new Entry{ text="Suspendisse tincidunt, erat vel interdum facilisis, arcu arcu maximus metus, ullamcorper auctor tortor ligula ut sem. In hac habitasse platea dictumst." , count=1 } ,
			new Entry{ text="Pellentesque nunc lorem, dictum non sodales eu, fringilla non eros. " , count=1 } ,
			new Entry{ text="Integer a neque semper, varius nunc sed, congue sapien." , count=1 } ,
			new Entry{ text="Sed vel vulputate ligula, sit amet porta velit." , count=1 } ,
			new Entry{ text="Etiam pharetra magna et tristique iaculis." , count=1 } ,
			new Entry{ text="Morbi eu ante nisi." , count=1 } ,
			new Entry{ text="Nullam in tellus mi." , count=1 } ,
			new Entry{ text="Ut malesuada dolor eget lectus placerat hendrerit." , count=1 } ,
			new Entry{ text="Nulla ultrices felis id ipsum imperdiet varius." , count=1 } ,
		};
		#endif

	}
	

}

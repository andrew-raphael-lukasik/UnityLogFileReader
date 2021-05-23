using UnityEngine;
using NUnit.Framework;

using IO = System.IO;

namespace Tests
{


	public class FileProcessing
	{
		[Test] public void TestFiles ()
		{
			string dir = IO.Path.Combine( Application.dataPath , "Editor/.unit_tests_data/" );
			if( !IO.Directory.Exists(dir) ) throw new System.Exception($"directory doesn't exists: {dir}");
			Debug.Log($"running {nameof(TestFiles)}, directory: {dir}");

			var logFiles = IO.Directory.GetFiles( dir , "*.log" );
			foreach( string logFilePath in logFiles )
			{
				string jsonFilePath = IO.Path.ChangeExtension( logFilePath , ".json" );
				if( !IO.File.Exists(jsonFilePath) )
				{
					Debug.LogError($"no json file found: {jsonFilePath}");
					continue;
				}
				Debug.Log($"\ttesting file: \"{IO.Path.GetFileName(jsonFilePath)}\"");
				
				string expectedJson = IO.File.ReadAllText( jsonFilePath );
				string[] actualLines = Core.WriteSafeReadAllLines( logFilePath );
				Entry[] actual = Core.ProcessRawLines( actualLines );
				
				// SerializableEntry[] serializableEntries = new SerializableEntry[ entries.Length ];
				// for( int i=0 ; i<entries.Length ; i++ )
				// 	serializableEntries[i] = (SerializableEntry) entries[i];
				// SerializableEntries results = new SerializableEntries{ entries=serializableEntries };
				// IO.File.WriteAllText( IO.Path.ChangeExtension(logFilePath,".txt") , actual );

				SerializableEntry[] expected = JsonUtility.FromJson<SerializableEntries>( expectedJson ).entries;
				Assert.AreEqual( actual.Length , expected.Length , $"expected.entries: {expected.Length}, actual.Length: {actual.Length}" );
				for( int i=0 ; i<actual.Length ; i++ )
				{
					Assert.AreEqual( expected:expected[i].count , actual:actual[i].count );
					Assert.AreEqual( expected:expected[i].text , actual:actual[i].text );
				}
			}
		}
	}


	[System.Serializable]
	public class SerializableEntries
	{
		public SerializableEntry[] entries;
	}


	[System.Serializable]
	public class SerializableEntry
	{

		public string text;
		public int count;

		public static implicit operator SerializableEntry ( Entry entry )
			=> new SerializableEntry{ text=entry.text , count=entry.count };
		
	}


}

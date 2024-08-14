namespace DialogueSystem
{
	using System;
	using System.IO;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using CSVUtility;
	using ExtensionMethods;
	
	[Serializable]
	public class Languages : IList<string>
	{
		public Languages ()
		{
			filePath = GetFullPath (Application.persistentDataPath);

			if (!CSVWriter.FileExists (filePath))
				CSVWriter.CreateFile (filePath);

			string projectFolder = UnityEditor.ProjectWindowUtil.GetContainingFolder (Application.dataPath);
			string backupFolder = Path.Combine (projectFolder, "ProjectData/DialogueSystem/");

			backupFilePath = GetFullPath (backupFolder);
			
			if (!CSVWriter.FileExists (backupFilePath))
				CSVWriter.CreateFile (backupFilePath);

			this.uniqueIDs = new ();
			this.languages = new ();

			string[][] content = CSVReader.Read (filePath);

			if (content.IsNullOrEmpty ())
				content = CSVReader.Read (backupFilePath);

			if (content.IsNullOrEmpty ())
				return;

			int columns = content.ColumnCount ();

			if (columns != 2)
				throw new Exception ("Unexpected number of columns in the CSV document.");

			string[] uniqueIDs = content.GetColumn (0);
			string[] languages = content.GetColumn (1);
			
			for (int i = 0; i < uniqueIDs.Length; i++)
				this.uniqueIDs.Add (uniqueIDs[i]);

			for (int i = 0; i < languages.Length; i++)
				this.languages.Add (languages[i]);
		}

		private static readonly string fileName = "data_languages";
		private static string filePath, backupFilePath;

		[SerializeField]
		private List<string> uniqueIDs;

		[SerializeField]
		private List<string> languages;

		private static string GetFullPath (string folderPath)
		{
			string fullFileName = string.Concat (fileName, ".csv");
			return Path.Combine (folderPath, fullFileName);
		}

		private void SaveData ()
		{
			const int datasets = 2;

			string[][] data = new string[datasets][];

			data[0] = uniqueIDs.ToArray ();
			data[1] = languages.ToArray ();

			CSVWriter.OverwriteFile (filePath, data.GetTranspose ());
			CSVWriter.OverwriteFile (backupFilePath, data.GetTranspose ());
		}

		public string this[int index]
		{
			get
			{
				if (index < 0 || index >= languages.Count)
				{
					string message = string.Concat ("Index out of range (index: ", index.ToString (), ", length: ", languages.Count, ").");
					throw new IndexOutOfRangeException (message);
				}

				return languages[index];
			}

			set
			{
				if (index < 0 || index >= languages.Count)
				{
					string message = string.Concat ("Index out of range (index: ", index.ToString (), ", length: ", languages.Count, ").");
					throw new IndexOutOfRangeException (message);
				}

				languages[index] = value;

				SaveData ();
			}
		}

		public string this[string id]
		{
			get
			{
				for (int i = 0; i < uniqueIDs.Count; i++)
					if (id == uniqueIDs[i])
						return languages[i];

				return string.Empty;
			}

			set
			{
				for (int i = 0; i < uniqueIDs.Count; i++)
				{
					if (id != uniqueIDs[i])
						continue;

					languages[i] = value;

					SaveData ();
				}
			}
		}

		public int Count => languages.Count;

		public bool IsReadOnly => true;

		public string GetID (int index)
		{
			if (index < 0 || index >= languages.Count)
			{
				string message = string.Concat ("Index out of range (index: ", index.ToString (), ", length: ", languages.Count, ").");
				throw new IndexOutOfRangeException (message);
			}

			return uniqueIDs[index];
		}

		public string GetID (string language)
		{
			for (int i = 0; i < languages.Count; i++)
				if (language == languages[i])
					return uniqueIDs[i];

			throw new Exception ("The specified language wasn't found.");
		}

		public void Clear ()
		{
			uniqueIDs.Clear ();
			languages.Clear ();

			SaveData ();
		}

		public int IndexOf (string language) => languages.IndexOf (language);

		public bool Contains (string language) => languages.Contains (language);

		public void CopyTo (string[] array, int arrayIndex) => languages.CopyTo (array, arrayIndex);

		public void Add (string language)
		{
			string uniqueID = GenerateUniqueID ();

			uniqueIDs.Add (uniqueID);
			languages.Add (language);

			SaveData ();
		}

		public void Insert (int index, string language)
		{
			languages.Insert (index, language);
			SaveData ();
		}

		public void Move (int indexFrom, int indexTo)
		{
			uniqueIDs.Move (indexFrom, indexTo);
			languages.Move (indexFrom, indexTo);

			SaveData ();
		}

		public bool Remove (string language)
		{
			int index = IndexOf (language);
			uniqueIDs.RemoveAt (index);
			
			bool removed = languages.Remove (language);

			SaveData ();

			return removed;
		}

		public void RemoveAt (int index)
		{
			uniqueIDs.RemoveAt (index);
			languages.RemoveAt (index);

			SaveData ();
		}

		public IEnumerator<string> GetEnumerator () => languages.GetEnumerator ();

		IEnumerator IEnumerable.GetEnumerator () => languages.GetEnumerator ();

		private string GenerateUniqueID ()
		{
			int randomID = UnityEngine.Random.Range (1000000, 10000000);
			
			for (int i = 0; i < uniqueIDs.Count; i++)
			{
				if (int.TryParse (uniqueIDs[i], out int id))
					if (randomID != id)
						continue;

				return GenerateUniqueID ();
			}

			return randomID.ToString ();
		}
	}
}

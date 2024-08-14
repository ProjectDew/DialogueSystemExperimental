namespace DialogueSystem
{
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;
	using EditorSections;
	using AssetManagement;
	using CSVUtility;
	using ExtensionMethods;

	public class ExporterSection : ISection
	{
		public ExporterSection ()
		{
			EditorApplication.delayCall += LoadPreferences;
		}

		private const string prefsFilePath = "DialogueSystem.DialoguesExporterSection.filePath";
		private const string prefsCurrentSearchOption = "DialogueSystem.DialoguesExporterSection.currentSearchOption";

		private readonly string[] searchOptionNames = new string[]
		{
			"in the project",
			"in the current folder",
			"in the current folder and subfolders",
			"in the active selection"
		};

		private SearchOption searchOption;
		private int currentSearchOption;
		
		private string filePath;

		private Languages languages;
		
		private List<string[]> content;
		private bool overwriteFile;

		private void LoadPreferences ()
		{
			filePath = EditorPrefs.GetString (prefsFilePath);
			currentSearchOption = EditorPrefs.GetInt (prefsCurrentSearchOption);
		}

		public void Draw ()
		{
			Layout.CenteredContent (DrawFileName);

			if (overwriteFile)
				Layout.CenteredContent (() => EditorGUILayout.HelpBox ("The file already exists. Click again to overwrite it.", MessageType.Warning, true), 10);
			else
				Layout.CenteredContent (() => EditorGUILayout.HelpBox ("If the document exists, it will be overwritten with the new data.", MessageType.Info, true), 10);

			Layout.GreyLine (10);

			Layout.CenteredContent (DrawSearchOptions, 10);

			if (EditorApplication.isCompiling || EditorApplication.isPlaying)
				overwriteFile = false;
		}

		private void DrawFileName ()
		{
			GUILayout.Label ("File path", Styles.BoldRightAlignedText ());

			EditorGUI.BeginChangeCheck ();

			filePath = EditorGUILayout.TextField (filePath, GUILayout.Width (Screen.width * 0.8f));

			if (filePath.IsNullOrEmpty ())
				filePath = string.Concat (Assets.GetCurrentFolder (), "Data_dialogues.csv");

			if (EditorGUI.EndChangeCheck ())
				EditorPrefs.SetString (prefsFilePath, filePath);
		}

		private void DrawSearchOptions ()
		{
			Layout.Button ("Export", ExportData, Styles.BoldCenteredText (GUI.skin.button));

			GUILayout.Label ("all dialogues");

			EditorGUI.BeginChangeCheck ();

			currentSearchOption = EditorGUILayout.Popup (currentSearchOption, searchOptionNames, GUILayout.MaxWidth (Screen.width * 0.6f));
			searchOption = (SearchOption)currentSearchOption;

			if (EditorGUI.EndChangeCheck ())
				EditorPrefs.SetInt (prefsCurrentSearchOption, currentSearchOption);
		}

		private void ExportData ()
		{
			if (filePath.IsNullOrEmpty ())
			{
				Debug.LogError ("The file path cannot be empty.");
				return;
			}
			
			if (!filePath.EndsWith (".csv"))
			{
				Debug.LogError ("The extension of the file must be \".csv\".");
				return;
			}

			languages = new ();
			content = new ();
			
			if (CSVWriter.FileExists (filePath))
			{
				if (!overwriteFile)
				{
					overwriteFile = true;
					return;
				}
			}
				
			CreateNewDocument ();

			AddDialogueNodes ();

			AssetDatabase.Refresh ();

			Debug.Log ("Dialogues exported to " + filePath);

			overwriteFile = false;
		}

		private void CreateNewDocument ()
		{
			List<string> firstLine = ComposeLine ("ID", "Is node", "Is branch", "Parents", "Children");
			List<string> secondLine = ComposeLine ("", "", "", "", "");

			for (int i = 0; i < languages.Count; i++)
			{
				firstLine.Add (languages[i]);
				secondLine.Add (languages.GetID (i));
			}

			content.Add (firstLine.ToArray ());
			content.Add (secondLine.ToArray ());

			CSVWriter.CreateFile (filePath, content.ToArray ());

			content.Clear ();
		}

		private List<string> ComposeLine (string id, params string[] additionalData)
		{
			List<string> newLine = new ();

			newLine.Add (id);

			for (int i = 0; i < additionalData.Length; i++)
				newLine.Add (additionalData[i]);

			return newLine;
		}

		private void AddDialogueNodes ()
		{
			DialogueNode[] dialogueNodes = Assets.FindAll<DialogueNode> (searchOption);

			string[][] documentContent = CSVReader.Read (filePath);

			List<string[]> currentData = new ();
			string[][] currentDataArray;

			int y = 0;

			for (int i = 0; i < dialogueNodes.Length; i++)
			{
				AddNode (dialogueNodes[i]);

				for (int x = 0; x < documentContent.Length; x++)
				{
					if (documentContent[x][y] != dialogueNodes[i].ID)
						continue;

					currentData.Add (documentContent[x]);
				}

				currentDataArray = currentData.ToArray ();

				if (CSVWriter.FileContains (filePath, currentDataArray))
					CSVWriter.ReplaceData (filePath, currentDataArray, content.ToArray ());
				else
					CSVWriter.SaveData (filePath, content.ToArray ());

				currentData.Clear ();
				content.Clear ();
			}
		}

		private void AddNode (DialogueNode dialogueNode)
		{
			DialogueNodeProperties dialogueNodeProperties = new (dialogueNode);
			
			List<string> newLine = new ();

			for (int i = 0; i < dialogueNodeProperties.Dialogues.Count; i++)
			{
				Dialogue dialogue = (Dialogue)dialogueNodeProperties.Dialogues.ObjectReference (i);

				bool isNode = dialogue.Equals (dialogueNode);
				bool isBranch = isNode && dialogueNode.IsBranch;

				newLine.Add (dialogueNode.ID);
				newLine.Add (isNode.ToString ());
				newLine.Add (isBranch.ToString ());

				if (isNode)
				{
					newLine.Add (GetConcatenatedDialogues (dialogueNode.GetParents ()));
					newLine.Add (GetConcatenatedDialogues (dialogueNode.GetChildren ()));
				}
				else
				{
					newLine.Add ("");
					newLine.Add ("");
				}

				for (int l = 0; l < languages.Count; l++)
				{
					string descriptor = dialogue.GetDescriptor (languages[l]);
					string content = dialogue.GetContent (languages[l]);

					newLine.Add (GetFullContent (descriptor, content));
				}

				content.Add (newLine.Copy ());

				newLine.Clear ();
			}
		}

		private string GetConcatenatedDialogues (DialogueNode[] dialogues)
		{
			if (dialogues.IsNullOrEmpty ())
				return "";
			
			string separator = ";";
			string concatenatedString = "";

			for (int i = 0; i < dialogues.Length; i++)
			{
				if (i < dialogues.Length - 1)
					concatenatedString = string.Concat (concatenatedString, dialogues[i].ID, separator);
				else
					concatenatedString = string.Concat (concatenatedString, dialogues[i].ID);
			}

			return concatenatedString;
		}

		private string GetFullContent (string descriptor, string content) => string.Concat (descriptor, "{<--DESCRIPTOR};", content);
	}
}

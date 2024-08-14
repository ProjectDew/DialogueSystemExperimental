namespace DialogueSystem
{
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;
	using AssetManagement;

	[System.Serializable]
	public sealed class LanguageEditor : EditorWindow
	{
		private const string prefsTotalColumns = "DialogueSystem.LanguagesWindow.totalColumns";

		private readonly string[] searchOptionNames = new string[]
		{
			"in the project",
			"in the current folder",
			"in the current folder and subfolders",
			"in the active selection"
		};

		private SearchOption searchOption;
		private int selectedOption;

		private Languages languages;
		private string[] indices;

		[SerializeField]
		private int totalColumns;

		[MenuItem ("Assets/Dialogue System/Language Editor", priority = 1125)]
		public static void OpenLanguageEditor ()
		{
			LanguageEditor window = GetWindow<LanguageEditor> ();
			
			window.Initialize ();
			window.Show ();
		}

		private void OnEnable ()
		{
			Initialize ();
		}

		private void Initialize ()
		{
			titleContent = new GUIContent ("Language Editor");
			minSize = new Vector2 (440, 137);
			
			totalColumns = EditorPrefs.GetInt (prefsTotalColumns);
			
			languages = new ();
		}

		private void OnGUI ()
		{
			if (indices == null || indices.Length != languages.Count)
			{
				indices = new string[languages.Count];

				for (int i = 0; i < indices.Length; i++)
					indices[i] = (i + 1).ToString ();
			}

			Layout.CenteredContent (() => GUILayout.Label ("LANGUAGES", Styles.BoldCenteredText ()), 10);

			Layout.GreyLine (10);

			if (languages.Count == 0)
				Layout.CenteredContent (() => GUILayout.Label ("No languages found.", Styles.ItalicCenteredText ()), 10);
			else
				Layout.CenteredContent (DrawColumnSelector, 10);

			for (int i = 0; i < languages.Count; i++)
				DrawLanguage (i);

			Layout.CenteredContent (() => Layout.Button ("Add language", () => languages.Add ("New language")), 10);

			Layout.GreyLine (10);

			Layout.CenteredContent (DrawApplyChanges, 10);

			GUILayout.Space (10);
		}

		private void DrawColumnSelector ()
		{
			GUILayout.Label ("Columns");

			int columns = totalColumns;

			columns = EditorGUILayout.IntField (columns, GUILayout.Width (30));
			columns = Mathf.Clamp (columns, 1, int.MaxValue);

			if (columns == totalColumns)
				return;

			totalColumns = columns;

			EditorPrefs.SetInt (prefsTotalColumns, totalColumns);
		}

		private void DrawLanguage (int index)
		{
			if (index == 0)
				GUILayout.Space (5);

			if (index % totalColumns == 0)
			{
				GUILayout.Space (5);

				GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
			}

			float width = (totalColumns == 1) ? Screen.width * 0.5f : (Screen.width - 60 * totalColumns) / totalColumns;

			int newIndex = index;
			newIndex = EditorGUILayout.Popup (newIndex, indices, GUILayout.Width (20));

			if (newIndex != index)
				languages.Move (index, newIndex);

			languages[index] = EditorGUILayout.TextField (languages[index], GUILayout.Width (width));

			Layout.Button ("X", () => RemoveLanguage (index), 20);

			if (index == languages.Count - 1 || index % totalColumns == totalColumns - 1)
			{
				GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal ();
			}
		}

		private void RemoveLanguage (int index)
		{
			ConfirmLanguageRemoval confirmLanguageRemoval = GetWindow<ConfirmLanguageRemoval> ();

			confirmLanguageRemoval.Open (languages[index], () => languages.RemoveAt (index));

			GUIUtility.ExitGUI ();
		}

		private void DrawApplyChanges ()
		{
			Layout.Button ("Apply changes", ApplyChanges);

			GUILayout.Label ("to all dialogues");

			selectedOption = EditorGUILayout.Popup (selectedOption, searchOptionNames, GUILayout.MinWidth (Screen.width * 0.5f));
			searchOption = (SearchOption)selectedOption;
		}

		private void ApplyChanges ()
		{
			DialogueNode[] dialogueNodes = Assets.FindAll<DialogueNode> (searchOption);

			if (dialogueNodes.Length == 0)
			{
				Debug.LogWarning ("No dialogues found.");
				return;
			}
			
			int successfulDialogues = 0;

			int successfulContents = 0;
			int totalContents = 0;

			for (int i = 0; i < dialogueNodes.Length; i++)
			{
				int successfulItems = UpdateNode (dialogueNodes[i]);
				
				if (dialogueNodes[i].TotalDialogues > 0)
					totalContents += dialogueNodes[i].TotalDialogues - 1;

				if (successfulItems == 0)
					continue;
				
				successfulDialogues++;
				successfulContents += successfulItems - 1;
			}

			Selection.objects = null;

			if (successfulDialogues > 0)
				Debug.Log (string.Concat ("Success in ", successfulDialogues, "/", dialogueNodes.Length, " assets and ", successfulContents, "/", totalContents, " subassets."));
			else
				Debug.LogWarning ("There are no changes to apply.");
		}

		private int UpdateNode (DialogueNode dialogueNode)
		{
			SerializedObject serializedObject = new (dialogueNode);
			DialogueNodeProperties properties = new (serializedObject);
			
			int successfulItems = 0;

			serializedObject.Update ();

			for (int i = 0; i < properties.Dialogues.Count; i++)
			{
				Dialogue dialogue = (Dialogue)properties.Dialogues.ObjectReference (i);

				if (UpdateDialogue (dialogue))
					successfulItems++;
			}

			serializedObject.ApplyModifiedProperties ();

			return successfulItems;
		}

		private bool UpdateDialogue (Dialogue dialogue)
		{
			SerializedObject serializedObject = new (dialogue);
			DialogueProperties properties = new (serializedObject);

			serializedObject.Update ();

			Dictionary<string, int> pairLanguagesAndContentIndices = new ();

			for (int i = 0; i < properties.LanguageIDs.Count; i++)
				pairLanguagesAndContentIndices.Add (properties.LanguageIDs.String (i), i);

			string[] content = properties.Content.GetArray<string> ();

			properties.LanguageIDs.Count = languages.Count;
			properties.LanguageNames.Count = languages.Count;

			properties.Descriptor.Count = languages.Count;
			properties.Content.Count = languages.Count;

			for (int i = 0; i < languages.Count; i++)
			{
				string languageID = languages.GetID (i);

				properties.LanguageIDs.Set (i, languageID);
				properties.LanguageNames.Set (i, languages[i]);

				if (!pairLanguagesAndContentIndices.ContainsKey (languageID))
				{
					properties.Descriptor.Set (i, "");
					properties.Content.Set (i, "");

					continue;
				}

				int contentIndex = pairLanguagesAndContentIndices[languageID];

				if (contentIndex < content.Length && contentIndex < properties.Descriptor.Count)
					properties.Descriptor.Set (i, content[contentIndex]);

				if (contentIndex < content.Length && contentIndex < properties.Content.Count)
					properties.Content.Set (i, content[contentIndex]);
			}
			
			return serializedObject.ApplyModifiedProperties ();
		}
	}
}

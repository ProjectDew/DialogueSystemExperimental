namespace DialogueSystem
{
	using System;
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;
	using EditorSections;
	using ExtensionMethods;

	using Object = UnityEngine.Object;

	public class DialogueNodeSection : IInspectorSection
	{
		private readonly string undoAddObjectToAsset = "Add object to asset";
		private readonly string undoRemoveObjectFromAsset = "Remove object from asset";
		
		private DialogueNodeProperties properties;
		private string assetPath;

		private DialogueSection dialogueSection;
		private SerializedObject serializedContent;
		private Action[] contentControllers;

		private int currentLanguage;
		private int currentContent;

		public void Initialize (SerializedObject serializedObject)
		{
			properties = new (serializedObject);
			assetPath = AssetDatabase.GetAssetPath (properties.TargetObject);
			
			serializedContent = new (properties.TargetObject);

			dialogueSection = new ();
			dialogueSection.Initialize (serializedContent);
		}

		public void Draw ()
		{
			if (properties == null)
				return;

			if (properties.ID.stringValue.IsNullOrEmpty () && !properties.SerializedObject.isEditingMultipleObjects)
				properties.ID.stringValue = properties.TargetObject.name;
			
			if (!Layout.FoldoutButton ("CONTENT", properties.Dialogues.BaseProperty))
				return;

			if (properties.Dialogues.Count == 0)
				Layout.CenteredContent (() => GUILayout.Label ("No contents to display.", Styles.ItalicCenteredText ()), 10);
			else
				DrawSection ();

			if (properties.SerializedObject.isEditingMultipleObjects)
				return;
			
			if (properties.Dialogues.Count > 1)
				DrawContentControllers ();

			Layout.CenteredContent (DrawInsertContent, 10);

			Layout.GreyLine (10);

			Layout.CenteredContent (DrawBranchToggle, 10);
		}

		private void DrawBranchToggle ()
		{
			GUILayout.Label ("Mark as a branch dialogue ");
			properties.IsBranch.boolValue = GUILayout.Toggle (properties.IsBranch.boolValue, "");
		}

		private void DrawSection ()
		{
			if (dialogueSection == null)
				return;

			if (currentContent >= properties.Dialogues.Count)
			{
				currentContent = 0;
				UpdateContent ();
			}
			
			if (properties.Dialogues.ObjectReference (currentContent) == null)
			{
				Layout.CenteredContent (() => Layout.Button ("Remove null element", RemoveContent, Screen.width * 0.94f), 10);
				return;
			}

			GUILayout.Space (10);

			if (serializedContent.targetObject != null)
				serializedContent.Update ();

			dialogueSection.Draw ();

			if (serializedContent.targetObject != null)
				serializedContent.ApplyModifiedProperties ();
		}

		private void DrawContentControllers ()
		{
			if (contentControllers.IsNullOrEmpty ())
				contentControllers = new Action[] { DrawRemoveContent, DrawIndexControllers };

			Layout.InlinedContent (contentControllers, 10);
		}

		private void DrawIndexControllers ()
		{
			if (currentContent > 0)
				Layout.Button ("<", SetPrevious, 20);

			EditorGUI.BeginChangeCheck ();

			int value = currentContent;
			value = EditorGUILayout.IntField (value, GUILayout.Width (20));
			
			if (EditorGUI.EndChangeCheck ())
				SetCurrent (value);
			
			if (currentContent == properties.Dialogues.Count - 1)
			{
				int lastContent = properties.Dialogues.Count - 1;
				string labelText = (lastContent < 10) ? string.Concat ("/ ", lastContent) : "–";
				
				GUILayout.Label (labelText, Styles.Style (EditorStyles.label, FontStyle.Normal, TextAnchor.UpperCenter, size: 13), GUILayout.Width (20));

				return;
			}
			
			Layout.Button (">", SetNext, 20);
		}

		private void SetPrevious ()
		{
			if (currentContent <= 0)
				return;

			currentContent--;

			UpdateContent ();
		}

		private void SetCurrent (int value)
		{
			value = Mathf.Clamp (value, 0, properties.Dialogues.Count - 1);

			currentContent = value;

			UpdateContent ();
		}

		private void SetNext ()
		{
			int lastContent = properties.Dialogues.Count - 1;

			if (currentContent >= lastContent)
				return;

			currentContent++;

			UpdateContent ();
		}

		private void UpdateContent ()
		{
			Object content = properties.Dialogues.ObjectReference (currentContent);

			if (content == null)
				return;

			currentLanguage = dialogueSection.CurrentLanguage;

			serializedContent = new (content);

			dialogueSection = new (currentLanguage);
			dialogueSection.Initialize (serializedContent);
		}

		private void DrawInsertContent ()
		{
			Layout.Button ("Insert new", InsertContent, 140);
		}

		private void DrawRemoveContent ()
		{
			if (currentContent > 0)
				Layout.Button ("X", RemoveContent, 20);

			Object content = properties.Dialogues.ObjectReference (currentContent);

			if (content != null)
			{
				GUILayout.Label ("Asset name:", Styles.ItalicText ());
				GUILayout.Label (content.name, Styles.BoldItalicText ());

				return;
			}
			
			GUILayout.Label ("null", Styles.BoldItalicText ());
		}

		private void InsertContent ()
		{
			if (properties.Dialogues.Count == 0)
			{
				properties.Dialogues.Add (properties.TargetObject);
				return;
			}

			Dialogue content = ScriptableObject.CreateInstance<Dialogue> ();

			Languages languages = new ();

			Undo.SetCurrentGroupName (undoAddObjectToAsset);
			int currentGroup = Undo.GetCurrentGroup ();

			DialogueProperties contentProperties = new (content);

			contentProperties.SerializedObject.Update ();

			for (int i = 0; i < languages.Count; i++)
			{
				contentProperties.LanguageNames.Add (languages[i]);
				contentProperties.LanguageIDs.Add (languages.GetID (i));

				contentProperties.Descriptor.Add ("");
				contentProperties.Content.Add ("");
			}

			contentProperties.SerializedObject.ApplyModifiedProperties ();

			AssetDatabase.AddObjectToAsset (content, properties.TargetObject);
			Undo.RegisterCreatedObjectUndo (content, undoAddObjectToAsset);

			int index = currentContent + 1;

			properties.Dialogues.Insert (index, content);

			for (int i = 1; i < properties.Dialogues.Count; i++)
				properties.Dialogues.ObjectReference (i).name = GetSequencialName (i);

			AssetDatabase.SaveAssets ();

			SetCurrent (index);

			Undo.CollapseUndoOperations (currentGroup);

			Undo.undoRedoEvent += OnRecreateAsset;
		}

		private void RemoveContent ()
		{
			if (currentContent <= 0 || properties.Dialogues.Count == 0 || currentContent >= properties.Dialogues.Count)
				return;

			Dialogue content = (Dialogue)properties.Dialogues.ObjectReference (currentContent);

			Undo.SetCurrentGroupName (undoRemoveObjectFromAsset);
			int currentGroup = Undo.GetCurrentGroup ();

			properties.Dialogues.RemoveAt (currentContent);

			for (int i = 1; i < properties.Dialogues.Count; i++)
				properties.Dialogues.ObjectReference (i).name = GetSequencialName (i);

			if (currentContent < properties.Dialogues.Count)
				UpdateContent ();
			else
				SetCurrent (properties.Dialogues.Count - 1);

			if (content == null)
				return;

			Undo.RecordObject (content, undoRemoveObjectFromAsset);
			Undo.DestroyObjectImmediate (content);

			AssetDatabase.SaveAssets ();

			Undo.CollapseUndoOperations (currentGroup);

			Undo.undoRedoEvent += OnRecreateAsset;
		}

		private void OnRecreateAsset (in UndoRedoInfo undoRedoInfo)
		{
			if (undoRedoInfo.undoName != undoAddObjectToAsset && undoRedoInfo.undoName != undoRemoveObjectFromAsset)
				return;

			Object[] contents = AssetDatabase.LoadAllAssetRepresentationsAtPath (assetPath);

			int deletedIndex = int.MaxValue;

			for (int i = 0; i < contents.Length; i++)
			{
				if (contents[i] == null)
				{
					deletedIndex = i;
					continue;
				}

				int index = (i > deletedIndex) ? i : i + 1;

				contents[i].name = GetSequencialName (index);
			}

			AssetDatabase.SaveAssets ();
		}

		private string GetSequencialName (int index) => string.Concat (AssetDatabase.LoadMainAssetAtPath (assetPath).name, "_", index.ToString ());
	}
}

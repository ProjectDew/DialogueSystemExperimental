namespace DialogueSystem
{
	using System;
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;

	public class ConfirmLanguageRemoval : EditorWindow
	{
		private const string warningMessage = "If you REMOVE A LANGUAGE, all the descriptors and contents associated to that language will be PERMANENTLY LOST " +
			"in all the specified dialogues when you apply changes.";

		private const string exportLabel = "You can make a backup of your dialogs before removing them.";

		private Action removeLanguage;
		private string language;

		public void Open (string language, Action removeLanguage)
		{
			const float width = 400, height = 165;

			this.removeLanguage = removeLanguage;
			this.language = language;

			titleContent = new GUIContent ("Remove language");
			
			Vector2 screenCenterPosition = EditorGUIUtility.GetMainWindowPosition ().center;
			position = new Rect (screenCenterPosition.x * 0.5f, screenCenterPosition.y * 0.5f, 400, 165);

			minSize = new Vector2 (width, height);
			maxSize = new Vector2 (width, height);

			ShowModal ();
		}

		private void OnGUI ()
		{
			EditorStyles.label.wordWrap = true;

			Layout.CenteredContent (() => EditorGUILayout.HelpBox (warningMessage, MessageType.Warning), 10);

			Layout.CenteredContent (() => GUILayout.Label (exportLabel, Styles.ItalicCenteredText (EditorStyles.label), GUILayout.MaxWidth (Screen.width * 0.95f)), 10);

			Layout.CenteredContent (() => Layout.Button ("Open exporter", OpenExporter, Screen.width * 0.33f), 10);

			Layout.GreyLine (10);

			Layout.CenteredContent (() => Layout.Button (string.Concat ("Remove ", language), RemoveLanguage, GUILayout.MinWidth (Screen.width * 0.33f)), 10);

			if (EditorApplication.isCompiling || EditorApplication.isPlaying)
				Close ();
		}

		private void OpenExporter ()
		{
			DialoguesExporter dialoguesExporter = GetWindow<DialoguesExporter> ();
			dialoguesExporter.Show ();
			
			Close ();
		}

		private void RemoveLanguage ()
		{
			removeLanguage ();
			Close ();
		}
	}
}

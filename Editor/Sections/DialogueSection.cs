namespace DialogueSystem
{
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;
	using EditorSections;
	using ExtensionMethods;

	public class DialogueSection : IInspectorSection
	{
		public DialogueSection () { }

		public DialogueSection (int currentLanguage)
		{
			this.currentLanguage = currentLanguage;
		}

		private DialogueProperties properties;

		[SerializeField]
		private string[] languageNames;
		
		[SerializeField]
		private int currentLanguage;

		public int CurrentLanguage => currentLanguage;

		public void Initialize (SerializedObject serializedObject)
		{
			properties = new (serializedObject);
			
			if (properties.Content.Count > properties.LanguageNames.Count)
				DrawErrorMessage ();
		}

		public void Draw ()
		{
			EditorStyles.textArea.wordWrap = true;

			GUIStyle style = Styles.CenteredText (EditorStyles.popup);
			GUILayoutOption width = GUILayout.MaxWidth (Screen.width * 0.45f);

			if (properties == null)
				return;
			
			if (properties.LanguageNames.Count == 0)
			{
				Layout.CenteredContent (() => Layout.Button ("Add languages", () => LanguageEditor.OpenLanguageEditor (), Screen.width * 0.5f));
				return;
			}

			if (languageNames.IsNullOrEmpty ())
				languageNames = properties.LanguageNames.GetArray<string> ();

			if (currentLanguage < 0 || currentLanguage >= properties.LanguageNames.Count)
				currentLanguage = 0;

			Layout.CenteredContent (() => currentLanguage = EditorGUILayout.Popup (currentLanguage, languageNames, style, width));

			Layout.CenteredContent (() => GUILayout.Label ("Descriptor", Styles.ItalicCenteredText ()), 5);
			Layout.CenteredContent (DrawDescriptor, 5);
			
			Layout.CenteredContent (() => GUILayout.Label ("Content", Styles.ItalicCenteredText ()), 5);
			Layout.CenteredContent (DrawContent, 5);
		}

		private void DrawErrorMessage ()
		{
			string errorMessage = "There are more contents than languages.";
			string languagesLength = properties.LanguageNames.Count.ToString ();
			string contentsLength = properties.Content.Count.ToString ();

			errorMessage = string.Concat (errorMessage, "\nLanguages: ", languagesLength, ", contents: ", contentsLength);

			Debug.LogWarning (errorMessage);
		}

		private void DrawDescriptor ()
		{
			GUILayoutOption maxWidth = GUILayout.MaxWidth (Screen.width * 0.45f);

			EditorGUI.BeginChangeCheck ();

			string descriptor = properties.Descriptor.String (currentLanguage);
			descriptor = EditorGUILayout.TextField (descriptor, Styles.CenteredText (EditorStyles.textField), maxWidth);

			if (EditorGUI.EndChangeCheck ())
				properties.Descriptor.Set (currentLanguage, descriptor);
		}

		private void DrawContent ()
		{
			GUIStyle style = new (EditorStyles.textArea);
			GUILayoutOption maxWidth = GUILayout.MaxWidth (Screen.width * 0.92f);

			EditorGUI.BeginChangeCheck ();

			string content = properties.Content.String (currentLanguage);
			content = EditorGUILayout.TextArea (content, style, maxWidth);

			if (EditorGUI.EndChangeCheck ())
				properties.Content.Set (currentLanguage, content);
		}
	}
}

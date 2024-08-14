namespace DialogueSystem
{
	using UnityEngine;
	using UnityEditor;
	using TMPro;
	using CustomizedEditor;
	using EditorSections;
	using ExtensionMethods;

	public class ContentReaderSection : IInspectorSection
	{
		private const int totalOverrideColumns = 4;

		private ContentReaderProperties properties;
		
		private GUILayoutOption objectFieldWidth;

		public void Initialize (SerializedObject serializedObject)
		{
			properties = new (serializedObject);
		}

		public void Draw ()
		{
			objectFieldWidth = GUILayout.Width (Screen.width * 0.35f);

			Layout.CenteredContent (DrawDescriptorText);

			Layout.GreyLine (10);
			
			if (Layout.FoldoutButton ("READ DELAYED", properties.CharacterDelay, 10))
				DrawCharactersDelay ();
		}

		private void DrawDescriptorText ()
		{
			GUILayout.Label ("Descriptor text", Styles.BoldText ());
			GUILayout.Label ("(optional)", Styles.ItalicText ());

			GUILayout.FlexibleSpace ();

			EditorGUI.BeginChangeCheck ();

			Object descriptorText = properties.DescriptorText.objectReferenceValue;
			descriptorText = EditorGUILayout.ObjectField (descriptorText, typeof (TMP_Text), !EditorUtility.IsPersistent (descriptorText), objectFieldWidth) as TMP_Text;

			if (EditorGUI.EndChangeCheck ())
				properties.DescriptorText.objectReferenceValue = descriptorText;
		}

		private void DrawCharactersDelay ()
		{
			Layout.CenteredContent (() => GUILayout.Label ("Default delay time between characters", Styles.BoldText ()), 10);
			Layout.CenteredContent (DrawDelay, 5);

			Layout.CenteredContent (() => Layout.Button ("Add a different delay for a specific character", AddOverride), 10);

			GUILayout.Space (5);

			if (properties.Delays.Count == 0)
				Layout.CenteredContent (() => GUILayout.Label ("No overriding delays to show", Styles.ItalicCenteredText ()));
			else
			{
				Layout.CenteredContent (() => GUILayout.Label ("The following delays will override the default value", Styles.ItalicCenteredText ()));
				properties.Delays.DrawInColumns (totalOverrideColumns, DrawOverride, 5);
			}
		}

		private void DrawDelay ()
		{
			EditorGUI.BeginChangeCheck ();

			float delay = properties.CharacterDelay.floatValue;
			delay = EditorGUILayout.FloatField (delay, Styles.CenteredText (EditorStyles.numberField), GUILayout.Width (40));

			if (EditorGUI.EndChangeCheck ())
				properties.CharacterDelay.floatValue = delay;
		}

		private void AddOverride ()
		{
			properties.Characters.Add ('\0');
			properties.Delays.Add (properties.CharacterDelay.floatValue);
		}

		private void DrawOverride (int index)
		{
			DrawOverrideCharacter (index);
			DrawOverrideDelay (index);

			Layout.Button ("X", () => RemoveOverride (index), 20);
		}

		private void RemoveOverride (int index)
		{
			properties.Characters.RemoveAt (index);
			properties.Delays.RemoveAt (index);
		}

		private void DrawOverrideCharacter (int index)
		{
			EditorGUI.BeginChangeCheck ();
			
			char character = (char)properties.Characters.UInt (index);
			string text = character.ToString ();

			text = EditorGUILayout.TextField (text, GUILayout.Width (20));

			if (!EditorGUI.EndChangeCheck ())
				return;
			
			character = (text.Length > 0) ? text[^1] : '\0';
			properties.Characters.Set (index, character);
		}

		private void DrawOverrideDelay (int index)
		{
			EditorGUI.BeginChangeCheck ();

			float delay = properties.Delays.Float (index);
			delay = EditorGUILayout.FloatField (delay, GUILayout.Width (30));

			if (EditorGUI.EndChangeCheck ())
				properties.Delays.Set (index, delay);
		}
	}
}

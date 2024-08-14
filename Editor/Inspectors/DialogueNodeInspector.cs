namespace DialogueSystem
{
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;
	using AssetManagement;
	using EditorSections.Presets;

	[CustomEditor (typeof (DialogueNode)), CanEditMultipleObjects]
	public sealed class DialogueNodeInspector : Editor
	{
		private DialogueNodeProperties properties;

		private DialogueNodeSection mainSection;
		
		private LinkedObject linkedParents;
		private LinkedObject linkedChildren;

		private GUILayoutOption editorWindowButtonMinWidth;

		private void OnEnable ()
		{
			properties = new (serializedObject);

			string parents = properties.ParentsPath;
			string children = properties.ChildrenPath;

			mainSection = new DialogueNodeSection ();

			linkedParents = new (serializedObject, parents, children);
			linkedChildren = new (serializedObject, children, parents);

			mainSection.Initialize (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			editorWindowButtonMinWidth = GUILayout.MinWidth (Screen.width * 0.33f);

			serializedObject.Update ();

			Layout.InlinedContent (() => GUILayout.Label ("DIALOGUE ID", Styles.BoldText ()), DrawDialogueID, 10);

			Layout.GreyLine (10);
			GUILayout.Space (10);

			Layout.CenteredContent (DrawInlinedInvokers);
			
			GUILayout.Space (5);

			Layout.CenteredContent (() => Layout.Button ("Languages", OpenLanguageEditor, editorWindowButtonMinWidth));

			Layout.GreyLine (10);
			GUILayout.Space (10);

			mainSection.Draw ();

			Layout.GreyLine (10);

			if (Layout.FoldoutButton ("PREV. DIALOGUES", properties.Parents.BaseProperty, 10))
			{
				GUILayout.Space (10);
				linkedParents.Draw ();
			}

			Layout.GreyLine (10);
			
			if (Layout.FoldoutButton ("NEXT DIALOGUES", properties.Children.BaseProperty, 10))
			{
				GUILayout.Space (10);
				linkedChildren.Draw ();
			}
			
			Layout.GreyLine (10);
			GUILayout.Space (5);

			serializedObject.ApplyModifiedProperties ();
		}

		private void DrawDialogueID ()
		{
			properties.ID.stringValue = EditorGUILayout.TextField (properties.ID.stringValue, Styles.CenteredText (EditorStyles.textField));
		}

		private void DrawInlinedInvokers ()
		{
			Layout.Button ("Manager", OpenDialoguesManager, editorWindowButtonMinWidth);

			GUILayout.FlexibleSpace ();

			Layout.Button ("Asset info", OpenAssetInfo, editorWindowButtonMinWidth);
		}

		private void OpenDialoguesManager ()
		{
			DialoguesManager window = EditorWindow.GetWindow<DialoguesManager> ();
			window.Show ();
		}

		private void OpenAssetInfo ()
		{
			AssetInfo window = EditorWindow.GetWindow<AssetInfo> ();

			window.SetActiveObject (serializedObject.targetObject);
			window.Show ();
		}

		private void OpenLanguageEditor ()
		{
			LanguageEditor window = EditorWindow.GetWindow<LanguageEditor> ();
			window.Show ();
		}
	}
}
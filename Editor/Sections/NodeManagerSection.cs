namespace DialogueSystem
{
	using UnityEngine;
	using UnityEditor;
	using TMPro;
	using CustomizedEditor;
	using EditorSections;
	using ExtensionMethods;

	public class NodeManagerSection : IInspectorSection
	{
		private const int totalBranchesColumns = 2;
		private const int totalNodeColumns = 2;

		private NodeManagerProperties properties;
		
		private GUILayoutOption objectFieldWidth;

		public void Initialize (SerializedObject serializedObject)
		{
			properties = new (serializedObject);
		}

		public void Draw ()
		{
			objectFieldWidth = GUILayout.Width (Screen.width * 0.35f);

			if (Layout.FoldoutButton ("TEXTS", properties.MainText))
			{
				Layout.CenteredContent (DrawMainDialogue, 10);
				DrawArray (properties.BranchTexts, totalBranchesColumns, "Add branch", "No branchs to show");
			}

			Layout.GreyLine (10);

			if (Layout.FoldoutButton ("NODES", properties.DialogueNodes.BaseProperty, 10))
				DrawArray (properties.DialogueNodes, totalNodeColumns, "Add node", "No nodes to show");
		}

		private void DrawMainDialogue ()
		{
			Object mainDialogue = properties.MainText.objectReferenceValue;
			mainDialogue = EditorGUILayout.ObjectField (mainDialogue, typeof (TMP_Text), !EditorUtility.IsPersistent (mainDialogue), objectFieldWidth) as TMP_Text;

			if (properties.MainText.objectReferenceValue != mainDialogue)
				properties.MainText.objectReferenceValue = mainDialogue;
		}

		private void DrawArray (SerializedArray array, int totalColumns, string buttonMessage, string emptyMessage)
		{
			Layout.CenteredContent (() => Layout.Button (buttonMessage, () => array.Add (null)), 10);

			GUILayout.Space (5);
			
			if (array.Count == 0)
				Layout.CenteredContent (() => GUILayout.Label (emptyMessage, Styles.ItalicCenteredText ()));
			else
				array.DrawInColumns (totalColumns, 5);
		}
	}
}

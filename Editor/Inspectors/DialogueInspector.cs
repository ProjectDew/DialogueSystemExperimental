namespace DialogueSystem
{
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;
	using AssetManagement;

	[CustomEditor (typeof (Dialogue)), CanEditMultipleObjects]
	public sealed class DialogueInspector : Editor
	{
		private DialogueSection dialogueSection;

		private void OnEnable ()
		{
			dialogueSection = new ();
			dialogueSection.Initialize (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			serializedObject.Update ();

			Layout.GreyLine (10);

			Layout.CenteredContent (() => Layout.Button ("Open asset info", OpenAssetInfo, Screen.width * 0.33f), 10);

			Layout.GreyLine (10);
			GUILayout.Space (10);

			dialogueSection.Draw ();
			
			Layout.GreyLine (10);
			GUILayout.Space (5);

			serializedObject.ApplyModifiedProperties ();
		}

		private void OpenAssetInfo ()
		{
			AssetInfo assetInfo = EditorWindow.GetWindow<AssetInfo> ();

			assetInfo.SetActiveObject (serializedObject.targetObject);
			assetInfo.Show ();
		}
	}
}

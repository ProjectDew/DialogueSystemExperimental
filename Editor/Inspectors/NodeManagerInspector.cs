namespace DialogueSystem
{
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;

	[CustomEditor (typeof (NodeManager)), CanEditMultipleObjects]
	public class NodeManagerInspector : Editor
	{
		private NodeManagerSection mainSection;

		private void OnEnable ()
		{
			mainSection = new ();
			mainSection.Initialize (serializedObject);
		}

		public override void OnInspectorGUI ()
		{
			serializedObject.Update ();
			
			GUILayout.Space (10);

			mainSection.Draw ();

			Layout.GreyLine (10);
			GUILayout.Space (5);

			serializedObject.ApplyModifiedProperties ();
		}
	}
}

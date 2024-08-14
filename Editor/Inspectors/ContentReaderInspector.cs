namespace DialogueSystem
{
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;

	[CustomEditor (typeof (ContentReader)), CanEditMultipleObjects]
	public class ContentReaderInspector : Editor
	{
		private ContentReaderSection mainSection;

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

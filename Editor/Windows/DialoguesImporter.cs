namespace DialogueSystem
{
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;

	public class DialoguesImporter : EditorWindow
	{
		[SerializeReference]
		private ImporterSection mainSection;

		[MenuItem ("Assets/Dialogue System/Import Dialogues", priority = 1140)]
		public static void OpenDialoguesImporter ()
		{
			DialoguesImporter window = GetWindow<DialoguesImporter> ();
			window.Show ();
		}

		private void Awake ()
		{
			titleContent = new GUIContent ("Dialogues importer");
			minSize = new Vector2 (400, 500);

			mainSection = new ();
		}

		private void OnGUI ()
		{
			if (mainSection == null)
				return;
			
			GUILayout.Space (10);

			GUILayout.Label ("DIALOGUES IMPORTER", Styles.BoldCenteredText ());
			
			GUILayout.Space (5);

			mainSection.Draw ();

			GUILayout.Space (10);
		}
	}
}

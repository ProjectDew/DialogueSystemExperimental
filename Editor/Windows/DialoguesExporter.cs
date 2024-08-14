namespace DialogueSystem
{
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;

	public class DialoguesExporter : EditorWindow
	{
		[SerializeReference]
		private ExporterSection mainSection;

		[MenuItem ("Assets/Dialogue System/Export Dialogues", priority = 1145)]
		public static void OpenDialoguesExporter ()
		{
			DialoguesExporter window = GetWindow<DialoguesExporter> ();

			window.Initialize ();
			window.Show ();
		}

		private void Initialize ()
		{
			Vector2 size = new (400, 182.5f);

			titleContent = new GUIContent ("Dialogues exporter");

			minSize = size;
			maxSize = size;

			mainSection = new ();
		}

		private void OnGUI ()
		{
			if (mainSection == null)
				return;
			
			GUILayout.Space (12.5f);

			GUILayout.Label ("DIALOGUES EXPORTER", Styles.BoldCenteredText ());
			
			Layout.GreyLine (10);
			GUILayout.Space (10);

			mainSection.Draw ();

			GUILayout.Space (10);
		}
	}
}
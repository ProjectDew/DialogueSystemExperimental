namespace DialogueSystem
{
    using UnityEngine;
    using UnityEditor;
	using CustomizedEditor;

    public class DialoguesManager : EditorWindow
    {
		[SerializeReference]
		private DialoguesManagerSection mainSection;

		[MenuItem ("CONTEXT/Dialogue/Dialogues Manager")]
		[MenuItem ("Assets/Dialogue System/Dialogues Manager", priority = 1120)]
        public static void OpenTranslationManager ()
        {
			EditorWindow window = GetWindow<DialoguesManager> ();
			window.Show ();
		}

		private void Awake ()
		{
			titleContent = new GUIContent ("Dialogues manager");
			minSize = new Vector2 (640, 480);

			mainSection = new (this);
		}

		private void OnGUI ()
		{
			if (mainSection == null)
				return;
			
			GUILayout.Space (10);

			GUILayout.Label ("DIALOGUES MANAGER", Styles.BoldCenteredText ());
			
			Layout.GreyLine (10);
			GUILayout.Space (10);

			mainSection.Draw ();

			GUILayout.Space (10);
		}
    }
}

namespace DialogueSystem
{
	using UnityEditor;
	using CustomizedEditor;

	public class LanguageEditorProperties : Properties
	{
		public LanguageEditorProperties (SerializedObject serializedObject) : base (serializedObject)
		{
			FindProperties ();
		}
		
		public LanguageEditorProperties (LanguageEditor inspectedObject) : base (inspectedObject)
		{
			FindProperties ();
		}
		
		private SerializedArray languages;
		
		public SerializedArray Languages => languages;
		
		public string LanguagesPath => languages.PropertyPath;

		private void FindProperties ()
		{
			languages = new (SerializedObject, "languages");
		}
	}
}

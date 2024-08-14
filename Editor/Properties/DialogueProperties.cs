namespace DialogueSystem
{
	using UnityEditor;
	using CustomizedEditor;

	public class DialogueProperties : Properties
	{
		public DialogueProperties (SerializedObject serializedObject) : base (serializedObject)
		{
			FindProperties ();
		}
		
		public DialogueProperties (Dialogue inspectedObject) : base (inspectedObject)
		{
			FindProperties ();
		}
		
		private SerializedArray languageIDs;
		private SerializedArray languageNames;
		private SerializedArray descriptor;
		private SerializedArray content;
		
		public SerializedArray LanguageIDs => languageIDs;
		
		public SerializedArray LanguageNames => languageNames;

		public SerializedArray Descriptor => descriptor;

		public SerializedArray Content => content;
		
		public string LanguageIDsPath => languageIDs.PropertyPath;
		
		public string LanguageNamesPath => languageNames.PropertyPath;

		public string DescriptorPath => descriptor.PropertyPath;

		public string ContentPath => content.PropertyPath;

		private void FindProperties ()
		{
			languageIDs = new (SerializedObject, "languageIDs");
			languageNames = new (SerializedObject, "languageNames");
			descriptor = new (SerializedObject, "descriptor");
			content = new (SerializedObject, "content");
		}
	}
}

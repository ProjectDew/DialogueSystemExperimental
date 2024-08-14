namespace DialogueSystem
{
    using UnityEngine;
    using UnityEditor;
    using CustomizedEditor;

    public class ContentReaderProperties : Properties
    {
        public ContentReaderProperties (SerializedObject serializedObject) : base (serializedObject)
		{
			FindProperties ();
		}
        
        public ContentReaderProperties (Object targetObject) : base (targetObject)
		{
			FindProperties ();
		}
		
		private SerializedProperty descriptorText;
		private SerializedProperty characterDelay;
		private SerializedProperty overrideDelays;

		private SerializedArray characters;
		private SerializedArray delays;
		
		public SerializedProperty DescriptorText => descriptorText;
		
		public SerializedProperty CharacterDelay => characterDelay;
		
		public SerializedProperty OverrideDelays => overrideDelays;

		public SerializedArray Characters => characters;

		public SerializedArray Delays => delays;
		
		public string DescriptorTextPath => descriptorText.propertyPath;
		
		public string CharacterDelayPath => characterDelay.propertyPath;
		
		public string OverrideDelaysPath => overrideDelays.propertyPath;

		public string CharactersPath => characters.PropertyPath;

		public string DelaysPath => delays.PropertyPath;

		private void FindProperties ()
		{
			descriptorText = SerializedObject.FindProperty ("descriptorText");
			characterDelay = SerializedObject.FindProperty ("characterDelay");
			
			overrideDelays = SerializedObject.FindProperty ("overrideDelays");

			characters = new (SerializedObject, overrideDelays, "characters");
			delays = new (SerializedObject, overrideDelays, "delays");
		}
    }
}

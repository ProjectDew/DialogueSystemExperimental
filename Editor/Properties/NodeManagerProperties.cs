namespace DialogueSystem
{
    using UnityEngine;
    using UnityEditor;
    using CustomizedEditor;

    public class NodeManagerProperties : Properties
    {
        public NodeManagerProperties (SerializedObject serializedObject) : base (serializedObject)
		{
			FindProperties ();
		}
        
        public NodeManagerProperties (Object targetObject) : base (targetObject)
		{
			FindProperties ();
		}
		
		private SerializedProperty mainText;
		private SerializedArray branchTexts;
		private SerializedArray dialogueNodes;
		
		public SerializedProperty MainText => mainText;

		public SerializedArray BranchTexts => branchTexts;

		public SerializedArray DialogueNodes => dialogueNodes;
		
		public string MainTextPath => mainText.propertyPath;

		public string BranchTextsPath => branchTexts.PropertyPath;

		public string DialogueNodesPath => dialogueNodes.PropertyPath;

		private void FindProperties ()
		{
			mainText = SerializedObject.FindProperty ("mainText");
			branchTexts = new (SerializedObject, "branchTexts");
			dialogueNodes = new (SerializedObject, "dialogueNodes");
		}
    }
}

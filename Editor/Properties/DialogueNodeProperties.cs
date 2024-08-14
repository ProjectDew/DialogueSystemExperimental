namespace DialogueSystem
{
	using UnityEditor;
	using CustomizedEditor;

	public class DialogueNodeProperties : Properties
	{
		public DialogueNodeProperties (SerializedObject serializedObject) : base (serializedObject)
		{
			FindProperties ();
		}
		
		public DialogueNodeProperties (DialogueNode inspectedObject) : base (inspectedObject)
		{
			FindProperties ();
		}
		
		private SerializedProperty id;
		private SerializedProperty isBranch;
		private SerializedArray dialogues;
		private SerializedArray parents;
		private SerializedArray children;
		
		public SerializedProperty ID => id;

		public SerializedProperty IsBranch => isBranch;

		public SerializedArray Dialogues => dialogues;

		public SerializedArray Parents => parents;

		public SerializedArray Children => children;
		
		public string IDPath => id.propertyPath;

		public string IsBranchPath => isBranch.propertyPath;

		public string DialoguesPath => dialogues.PropertyPath;

		public string ParentsPath => parents.PropertyPath;

		public string ChildrenPath => children.PropertyPath;

		private void FindProperties ()
		{
			id = SerializedObject.FindProperty ("id");
			dialogues = new (SerializedObject, "dialogues");
			parents = new (SerializedObject, "parents");
			children = new (SerializedObject, "children");
			isBranch = SerializedObject.FindProperty ("isBranch");
		}
	}
}

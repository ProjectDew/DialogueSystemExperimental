namespace DialogueSystem
{
	using UnityEngine;
	using UnityEditor;
	using AssetManagement;

	public class CreateDialogue
	{
		[MenuItem ("Assets/Dialogue System/Create Dialogue %&d", priority = 1100)]
		public static void Create ()
		{
			DialogueNode dialogueNode = ScriptableObject.CreateInstance<DialogueNode> ();
			string assetPath = string.Concat (Assets.GetCurrentFolder (), "New dialogue.asset");

			SerializedObject serializedObject = new (dialogueNode);
			DialogueNodeProperties nodeProperties = new (serializedObject);

			serializedObject.Update ();

			nodeProperties.Dialogues.Add (dialogueNode);

			serializedObject.ApplyModifiedProperties ();

			Languages languages = new ();

			if (languages.Count > 0)
			{
				for (int i = 0; i < nodeProperties.Dialogues.Count; i++)
				{
					Dialogue dialogue = (Dialogue)nodeProperties.Dialogues.ObjectReference (i);
					AddExistingLanguages (dialogue, languages);
				}
			}

			ProjectWindowUtil.CreateAsset (dialogueNode, assetPath);

			Selection.activeObject = dialogueNode;
		}

		private static void AddExistingLanguages (Dialogue dialogue, Languages languages)
		{
			DialogueProperties dialogueProperties = new (dialogue);

			dialogueProperties.SerializedObject.Update ();

			for (int i = 0; i < languages.Count; i++)
			{
				dialogueProperties.LanguageIDs.Add (languages.GetID (i));
				dialogueProperties.LanguageNames.Add (languages[i]);

				dialogueProperties.Descriptor.Add ("");
				dialogueProperties.Content.Add ("");
			}

			dialogueProperties.SerializedObject.ApplyModifiedProperties ();
		}
	}
}

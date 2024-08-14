namespace DialogueSystem
{
	using System;
	using UnityEngine;
	using ExtensionMethods;

	public class DialogueNode : Dialogue
	{
		private readonly string indexOutOfRange = "Index out of range. Index: {0}, length: {1}.";

		[SerializeField]
		private string id;

		[SerializeField]
		private bool isBranch;
		
		[SerializeField]
		private Dialogue[] dialogues;

		[SerializeField]
		private DialogueNode[] parents;

		[SerializeField]
		private DialogueNode[] children;

		public string ID => id;

		public bool IsBranch => isBranch;

		public int TotalDialogues => dialogues.Length;

		public int TotalParents => parents.Length;

		public int TotalChildren => children.Length;

		public Dialogue GetDialogue (int dialogueIndex)
		{
			if (dialogueIndex < 0 || dialogueIndex >= dialogues.Length)
			{
				string message = string.Format (indexOutOfRange, dialogueIndex.ToString (), dialogues.Length);
				throw new IndexOutOfRangeException (message);
			}

			return dialogues[dialogueIndex];
		}

		public string GetDescriptor (string language, int dialogueIndex)
		{
			if (dialogueIndex < 0 || dialogueIndex >= dialogues.Length)
			{
				string message = string.Format (indexOutOfRange, dialogueIndex.ToString (), dialogues.Length);
				throw new IndexOutOfRangeException (message);
			}

			return dialogues[dialogueIndex].GetDescriptor (language);
		}

		public string GetContent (string language, int dialogueIndex)
		{
			if (dialogueIndex < 0 || dialogueIndex >= dialogues.Length)
			{
				string message = string.Format (indexOutOfRange, dialogueIndex.ToString (), dialogues.Length);
				throw new IndexOutOfRangeException (message);
			}

			return dialogues[dialogueIndex].GetContent (language);
		}

		public DialogueNode GetParent (string dialogueID)
		{
			for (int i = 0; i < parents.Length; i++)
				if (dialogueID == parents[i].ID)
					return parents[i];

			return null;
		}

		public DialogueNode GetParentAt (int index)
		{
			if (index < 0 || index >= parents.Length)
				throw new IndexOutOfRangeException ();

			return parents[index];
		}

		public DialogueNode[] GetParents () => parents.Copy ();

		public DialogueNode GetChild (string dialogueID)
		{
			for (int i = 0; i < children.Length; i++)
				if (dialogueID == children[i].ID)
					return children[i];

			return null;
		}

		public DialogueNode GetChildAt (int index)
		{
			if (index < 0 || index >= children.Length)
				throw new IndexOutOfRangeException ();

			return children[index];
		}

		public DialogueNode[] GetChildren () => children.Copy ();
	}
}

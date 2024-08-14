namespace DialogueSystem
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using TMPro;

	public class NodeManager : MonoBehaviour
	{
		private readonly string branchIndexOutOfRange = "Branch index out of range. Index: {0}. Length: {1}.";
		private readonly string contentIndexOutOfRange = "Content index out of range. Index: {0}. Length: {1}.";

		[SerializeField]
		private TMP_Text mainText;
		
		[SerializeField]
		private TMP_Text[] branchTexts;

		[SerializeField]
		private DialogueNode[] dialogueNodes;

		private class ProcessedDialogue
		{
			public ProcessedDialogue (string id, string content, string descriptor, bool isBranched)
			{
				ID = id;
				Content = content;
				Descriptor = descriptor;
				IsBranched = isBranched;

				IsConcatenated = false;
				SeparationString = "";
			}

			public string ID { get; private set; }

			public string Content { get; private set; }

			public string Descriptor { get; private set; }

			public bool IsBranched { get; private set; }

			public bool IsConcatenated { get; private set; }

			public string SeparationString { get; private set; }

			public void MarkAsConcatenated (string separationString)
			{
				SeparationString = separationString;
				IsConcatenated = true;
			}
		}

		public class NodeInfo
		{
			private NodeInfo () { }

			public class NodeInfoFactory : INodeInfoFactory
			{
				private readonly NodeInfo nodeInfo = new ();

				public NodeInfo CreateNodeInfo (DialogueNode dialogueNode, string language, int dialogueIndex)
				{
					nodeInfo.Node = dialogueNode;
					nodeInfo.Content = dialogueNode.GetContent (language, dialogueIndex);
					nodeInfo.Descriptor = dialogueNode.GetDescriptor (language, dialogueIndex);
					nodeInfo.IsOnFirstDialogue = dialogueIndex == 0;
					nodeInfo.IsOnLastDialogue = dialogueIndex == dialogueNode.TotalDialogues - 1;

					return nodeInfo;
				}
			}

			public DialogueNode Node { get; private set; }

			public string Content { get; private set; }

			public string Descriptor { get; private set; }

			public bool IsOnFirstDialogue { get; private set; }

			public bool IsOnLastDialogue { get; private set; }
		}

		private interface INodeInfoFactory
		{
			NodeInfo CreateNodeInfo (DialogueNode dialogueNode, string language, int dialogueIndex);
		}

		private INodeInfoFactory nodeInfoFactory;

		private DialogueNode[] branchedNodes;
		
		private ContentReader mainReader;
		private ContentReader[] branchReaders;
		
		private ContentReader currentReader;
		
		private ProcessedDialogue processedDialogue;
		private List<ProcessedDialogue> pastDialogues;
		
		private DialogueNode currentNode;
		private string currentLanguage;
		
		private int dialogueIndex;

		public TMP_Text MainText => mainText;

		public ContentReader MainReader => mainReader;

		public NodeInfo CurrentNodeInfo { get; private set; }

		public int TotalBranches => branchTexts.Length;

		public float TextSpeed
		{
			get => currentReader.TextSpeed;
			set
			{
				if (mainReader != null)
					mainReader.TextSpeed = value;

				if (branchReaders == null)
					return;

				for (int i = 0; i < branchReaders.Length; i++)
				{
					if (branchReaders[i] == null)
						continue;
					
					branchReaders[i].TextSpeed = value;
				}
			}
		}

		public bool HasFinishedReading => currentReader.HasFinishedReading;

		private void Awake ()
		{
			mainReader = mainText.GetComponent<ContentReader> ();
			
			branchedNodes = new DialogueNode[branchTexts.Length];
			branchReaders = new ContentReader[branchTexts.Length];

			for (int i = 0; i < branchReaders.Length; i++)
			{
				branchReaders[i] = branchTexts[i].GetComponent<ContentReader> ();
			}

			nodeInfoFactory = new NodeInfo.NodeInfoFactory ();

			pastDialogues = new ();
		}

		public TMP_Text GetBranchText (int branchIndex)
		{
			if (branchIndex < 0 || branchIndex >= branchReaders.Length)
				throw new IndexOutOfRangeException ();

			return branchTexts[branchIndex];
		}

		public ContentReader GetBranchReader (int branchIndex)
		{
			if (branchIndex < 0 || branchIndex >= branchReaders.Length)
				throw new IndexOutOfRangeException ();

			return branchReaders[branchIndex];
		}

		public void SetLanguage (string language)
		{
			currentLanguage = language;
		}

		public void ReadDialogue (string dialogueID, params ITextProcessor[] contentProcessors)
		{
			DialogueNode dialogue = GetNode (dialogueID);

			SetCurrentNode (dialogue, 0, contentProcessors);

			if (dialogue.IsBranch)
			{
				ReadDialogue (dialogue, 0, 0);
				return;
			}

			ReadContent (mainReader, mainText);
		}

		public void ReadDialogue (string dialogueID, int contentIndex, params ITextProcessor[] contentProcessors)
		{
			DialogueNode dialogue = GetNode (dialogueID);

			if (contentIndex < 0 || contentIndex >= dialogue.TotalDialogues)
				throw new IndexOutOfRangeException (string.Format (contentIndexOutOfRange, contentIndex, dialogue.TotalDialogues));

			SetCurrentNode (dialogue, contentIndex, contentProcessors);

			if (dialogue.IsBranch)
			{
				ReadDialogue (dialogue, contentIndex, 0);
				return;
			}

			ReadContent (mainReader, mainText);
		}

		public void ReadDialogue (string dialogueID, int contentIndex, int branchIndex, params ITextProcessor[] contentProcessors)
		{
			DialogueNode dialogue = GetNode (dialogueID);
			
			if (!dialogue.IsBranch)
				throw new IndexOutOfRangeException ("You've specified a branch index, but this dialogue is not marked as branch.");

			if (branchIndex < 0 || branchIndex >= branchTexts.Length)
				throw new IndexOutOfRangeException (string.Format (branchIndexOutOfRange, branchIndex, branchTexts.Length));
			
			if (contentIndex < 0 || contentIndex >= dialogue.TotalDialogues)
				throw new IndexOutOfRangeException (string.Format (contentIndexOutOfRange, contentIndex, dialogue.TotalDialogues));

			branchedNodes[branchIndex] = dialogue;
			
			SetCurrentNode (dialogue, contentIndex, contentProcessors);
			ReadContent (branchReaders[branchIndex], branchTexts[branchIndex]);
		}

		public void ReadDialogue (DialogueNode dialogue, params ITextProcessor[] contentProcessors)
		{
			SetCurrentNode (dialogue, 0, contentProcessors);

			if (dialogue.IsBranch)
			{
				ReadDialogue (dialogue, 0, 0);
				return;
			}

			ReadContent (mainReader, mainText);
		}

		public void ReadDialogue (DialogueNode dialogue, int contentIndex, params ITextProcessor[] contentProcessors)
		{
			if (contentIndex < 0 || contentIndex >= dialogue.TotalDialogues)
				throw new IndexOutOfRangeException (string.Format (contentIndexOutOfRange, contentIndex, dialogue.TotalDialogues));

			SetCurrentNode (dialogue, contentIndex, contentProcessors);

			if (dialogue.IsBranch)
			{
				ReadDialogue (dialogue, contentIndex, 0);
				return;
			}

			ReadContent (mainReader, mainText);
		}

		public void ReadDialogue (DialogueNode dialogue, int contentIndex, int branchIndex, params ITextProcessor[] contentProcessors)
		{
			if (!dialogue.IsBranch)
				throw new IndexOutOfRangeException ("You've specified a branch index, but this dialogue is not marked as branch.");

			if (branchIndex < 0 || branchIndex >= branchTexts.Length)
				throw new IndexOutOfRangeException (string.Format (branchIndexOutOfRange, branchIndex, branchTexts.Length));
			
			if (contentIndex < 0 || contentIndex >= dialogue.TotalDialogues)
				throw new IndexOutOfRangeException (string.Format (contentIndexOutOfRange, contentIndex, dialogue.TotalDialogues));

			branchedNodes[branchIndex] = dialogue;
			
			SetCurrentNode (dialogue, contentIndex, contentProcessors);
			ReadContent (branchReaders[branchIndex], branchTexts[branchIndex]);
		}

		public void UnreadDialogue (string dialogueID, params ITextProcessor[] contentProcessors)
		{
			DialogueNode dialogue = GetNode (dialogueID);
			
			SetCurrentNode (GetNode (dialogueID), 0, contentProcessors);

			if (dialogue.IsBranch)
			{
				UnreadDialogue (dialogue, 0, 0);
				return;
			}

			UnreadContent (mainReader, mainText);
		}

		public void UnreadDialogue (string dialogueID, int contentIndex, params ITextProcessor[] contentProcessors)
		{
			DialogueNode dialogue = GetNode (dialogueID);
			
			if (contentIndex < 0 || contentIndex >= dialogue.TotalDialogues)
				throw new IndexOutOfRangeException (string.Format (contentIndexOutOfRange, contentIndex, dialogue.TotalDialogues));

			SetCurrentNode (GetNode (dialogueID), contentIndex, contentProcessors);

			if (dialogue.IsBranch)
			{
				UnreadDialogue (dialogue, contentIndex, 0);
				return;
			}

			UnreadContent (mainReader, mainText);
		}

		public void UnreadDialogue (string dialogueID, int contentIndex, int branchIndex, params ITextProcessor[] contentProcessors)
		{
			DialogueNode dialogue = GetNode (dialogueID);
			
			if (!dialogue.IsBranch)
				throw new IndexOutOfRangeException ("You've specified a branch index, but this dialogue is not marked as branch.");

			if (branchIndex < 0 || branchIndex >= branchTexts.Length)
				throw new IndexOutOfRangeException (string.Format (branchIndexOutOfRange, branchIndex, branchTexts.Length));
			
			if (contentIndex < 0 || contentIndex >= dialogue.TotalDialogues)
				throw new IndexOutOfRangeException (string.Format (contentIndexOutOfRange, contentIndex, dialogue.TotalDialogues));
			
			branchedNodes[branchIndex] = dialogue;
			
			SetCurrentNode (dialogue, contentIndex, contentProcessors);
			UnreadContent (branchReaders[branchIndex], branchTexts[branchIndex]);
		}

		public void UnreadDialogue (DialogueNode dialogue, params ITextProcessor[] contentProcessors)
		{
			SetCurrentNode (dialogue, 0, contentProcessors);

			if (dialogue.IsBranch)
			{
				UnreadDialogue (dialogue, 0, 0);
				return;
			}

			UnreadContent (mainReader, mainText);
		}

		public void UnreadDialogue (DialogueNode dialogue, int contentIndex, params ITextProcessor[] contentProcessors)
		{
			if (contentIndex < 0 || contentIndex >= dialogue.TotalDialogues)
				throw new IndexOutOfRangeException (string.Format (contentIndexOutOfRange, contentIndex, dialogue.TotalDialogues));

			SetCurrentNode (dialogue, contentIndex, contentProcessors);

			if (dialogue.IsBranch)
			{
				UnreadDialogue (dialogue, contentIndex, 0);
				return;
			}

			UnreadContent (mainReader, mainText);
		}

		public void UnreadDialogue (DialogueNode dialogue, int contentIndex, int branchIndex, params ITextProcessor[] contentProcessors)
		{
			if (!dialogue.IsBranch)
				throw new IndexOutOfRangeException ("You've specified a branch index, but this dialogue is not marked as branch.");

			if (branchIndex < 0 || branchIndex >= branchTexts.Length)
				throw new IndexOutOfRangeException (string.Format (branchIndexOutOfRange, branchIndex, branchTexts.Length));
			
			if (contentIndex < 0 || contentIndex >= dialogue.TotalDialogues)
				throw new IndexOutOfRangeException (string.Format (contentIndexOutOfRange, contentIndex, dialogue.TotalDialogues));
			
			branchedNodes[branchIndex] = dialogue;
			
			SetCurrentNode (dialogue, contentIndex, contentProcessors);
			UnreadContent (branchReaders[branchIndex], branchTexts[branchIndex]);
		}
		
		public bool ReadPrevious ()
		{
			ProcessedDialogue previousDialogue = pastDialogues[^1];

			string preexistingContent = GetConcatenatedContent ();
			int previousIndex = dialogueIndex - 1;

			pastDialogues.RemoveAt (pastDialogues.Count - 1);

			if (previousIndex >= 0)
			{
				SetCurrentNode (currentNode, previousIndex);
				ReadContent (mainReader, mainText, preexistingContent);
			}
			else
			{
				if (currentNode.TotalParents == 0)
					return false;

				int contentIndex = 0;

				if (currentNode.IsBranch)
				{
					DialogueNode dialogue = currentNode.GetParent (previousDialogue.ID).GetParent (pastDialogues[^1].ID);

					SetCurrentNode (dialogue, contentIndex);
					ReadChildren ();
				}
				else
				{
					DialogueNode dialogue = currentNode.GetParent (previousDialogue.ID);

					SetCurrentNode (dialogue, contentIndex);
					ReadContent (mainReader, mainText, preexistingContent);
				}
			}

			return true;
		}

		public bool UnreadPrevious ()
		{
			ProcessedDialogue previousDialogue = pastDialogues[^1];

			string preexistingContent = GetConcatenatedContent ();
			int previousIndex = dialogueIndex - 1;

			pastDialogues.RemoveAt (pastDialogues.Count - 1);

			if (previousIndex >= 0)
			{
				SetCurrentNode (currentNode, previousIndex);
				UnreadContent (mainReader, mainText, preexistingContent);
			}
			else
			{
				if (currentNode.TotalParents == 0)
					return false;

				int contentIndex = 0;

				if (currentNode.IsBranch)
				{
					DialogueNode dialogue = currentNode.GetParent (previousDialogue.ID).GetParent (pastDialogues[^1].ID);

					SetCurrentNode (dialogue, contentIndex);
					UnreadChildren ();
				}
				else
				{
					DialogueNode dialogue = currentNode.GetParent (previousDialogue.ID);

					SetCurrentNode (dialogue, contentIndex);
					UnreadContent (mainReader, mainText, preexistingContent);
				}
			}

			return true;
		}

		public bool ReadNext (params ITextProcessor[] contentProcessors)
		{
			int nextIndex = dialogueIndex + 1;

			if (nextIndex < currentNode.TotalDialogues)
			{
				SetCurrentNode (currentNode, nextIndex, contentProcessors);
				ReadContent (mainReader, mainText);
			}
			else
			{
				if (currentNode.TotalChildren == 0)
					return false;
				else if (currentNode.TotalChildren == 1)
					ReadChild (currentNode, 0, contentProcessors);
				else
					ReadChildren (contentProcessors);
			}

			return true;
		}

		public bool ConcatenateNext (string separationString, params ITextProcessor[] contentProcessors)
		{
			int nextIndex = dialogueIndex + 1;

			string preexistingContent = string.Concat (currentNode.GetContent (currentLanguage, dialogueIndex), separationString);

			if (nextIndex < currentNode.TotalDialogues)
			{
				SetCurrentNode (currentNode, nextIndex, contentProcessors);
				ReadContent (mainReader, mainText, preexistingContent);
			}
			else
			{
				if (currentNode.TotalChildren == 0)
					return false;
				else if (currentNode.TotalChildren == 1)
					ReadChild (currentNode, 0, preexistingContent, contentProcessors);
				else
					ReadChildren (contentProcessors);
			}

			processedDialogue.MarkAsConcatenated (separationString);

			return true;
		}

		public bool SelectBranch (int branchIndex, params ITextProcessor[] contentProcessors)
		{
			if (branchIndex < 0 || branchIndex >= branchedNodes.Length)
				return false;
			
			if (branchedNodes[branchIndex].TotalChildren == 0)
				return false;
			
			int contentIndex = 0;
			
			DialogueNode dialogue = branchedNodes[branchIndex].GetChildAt (contentIndex);

			SetCurrentNode (dialogue, contentIndex, contentProcessors);
			ReadContent (mainReader, mainText);

			return true;
		}

		private DialogueNode GetNode (string id)
		{
			for (int i = 0; i < dialogueNodes.Length; i++)
			{
				if (dialogueNodes[i] == null || id != dialogueNodes[i].ID)
					continue;

				return dialogueNodes[i];
			}

			return null;
		}

		private void SetCurrentNode (DialogueNode dialogue, int dialogueIndex, params ITextProcessor[] contentProcessors)
		{
			currentNode = dialogue;
			CurrentNodeInfo = nodeInfoFactory.CreateNodeInfo (currentNode, currentLanguage, dialogueIndex);

			if (processedDialogue != null)
				pastDialogues.Add (processedDialogue);

			this.dialogueIndex = dialogueIndex;
			
			string content = dialogue.GetContent (currentLanguage, dialogueIndex);
			string descriptor = dialogue.GetDescriptor (currentLanguage, dialogueIndex);
			
			content = ProcessText (content, contentProcessors);

			processedDialogue = new ProcessedDialogue (dialogue.ID, content, descriptor, dialogue.IsBranch);
		}

		private string ProcessText (string text, ITextProcessor[] textProcessors)
		{
			if (textProcessors == null)
				return text;

			for (int i = 0; i < textProcessors.Length; i++)
			{
				if (textProcessors[i] == null)
					continue;
				
				text = textProcessors[i].ProcessText (text);
			}

			return text;
		}

		private void ReadChildren (params ITextProcessor[] contentProcessors)
		{
			DialogueNode parentNode = currentNode;
			DialogueNode[] childrenNodes = parentNode.GetChildren ();
				
			for (int i = 0; i < childrenNodes.Length; i++)
			{
				if (childrenNodes[i] == null)
					continue;

				if (i >= branchReaders.Length)
					break;
				
				ReadChild (parentNode, i, contentProcessors);
			}
		}

		private void UnreadChildren (params ITextProcessor[] contentProcessors)
		{
			DialogueNode parentNode = currentNode;
			DialogueNode[] childrenNodes = parentNode.GetChildren ();
				
			for (int i = 0; i < childrenNodes.Length; i++)
			{
				if (childrenNodes[i] == null)
					continue;

				if (i >= branchReaders.Length)
					break;
				
				UnreadChild (parentNode, i, contentProcessors);
			}
		}

		private void ReadChild (DialogueNode parentDialogue, int childIndex, params ITextProcessor[] contentProcessors) => ReadChild (parentDialogue, childIndex, "", contentProcessors);

		private void ReadChild (DialogueNode parentDialogue, int childIndex, string preexistingContent, ITextProcessor[] contentProcessors)
		{
			branchedNodes[childIndex] = parentDialogue.GetChildAt (childIndex);
			
			ContentReader contentReader = branchedNodes[childIndex].IsBranch ? branchReaders[childIndex] : mainReader;
			TMP_Text text = branchedNodes[childIndex].IsBranch ? branchTexts[childIndex] : mainText;

			int contentIndex = 0;
			
			SetCurrentNode (branchedNodes[childIndex], contentIndex, contentProcessors);
			ReadContent (contentReader, text, preexistingContent);
		}

		private void UnreadChild (DialogueNode parentDialogue, int childIndex, ITextProcessor[] contentProcessors)
		{
			branchedNodes[childIndex] = parentDialogue.GetChildAt (childIndex);
			
			ContentReader contentReader = branchedNodes[childIndex].IsBranch ? branchReaders[childIndex] : mainReader;
			TMP_Text text = branchedNodes[childIndex].IsBranch ? branchTexts[childIndex] : mainText;
			
			int contentIndex = 0;
			
			SetCurrentNode (branchedNodes[childIndex], contentIndex, contentProcessors);
			ReadContent (contentReader, text, "");
		}

		private void ReadContent (ContentReader contentReader, TMP_Text text, string preexistingContent = "")
		{
			currentReader = contentReader;

			if (contentReader == null)
			{
				if (text == null)
					throw new NullReferenceException (string.Concat ("The TMP_Text ", text.name, " is null."));

				text.text = string.Concat (preexistingContent, processedDialogue.Content);

				return;
			}

			contentReader.Read (preexistingContent, processedDialogue.Content, processedDialogue.Descriptor);
		}

		private void UnreadContent (ContentReader contentReader, TMP_Text text, string preexistingContent = "")
		{
			currentReader = contentReader;

			if (contentReader == null)
			{
				if (text == null)
					throw new NullReferenceException (string.Concat ("The TMP_Text ", text.name, " is null."));

				text.text = string.Concat (preexistingContent, processedDialogue.Content);

				return;
			}

			contentReader.Unread (preexistingContent, processedDialogue.Content, processedDialogue.Descriptor);
		}

		private string GetConcatenatedContent ()
		{
			if (pastDialogues.Count == 0 || !pastDialogues[^1].IsConcatenated)
				return "";
			
			List<string> contentToConcatenate = new ();
			string concatenatedContent = "";

			for (int i = pastDialogues.Count; i > 0; --i)
			{
				string content = string.Concat (pastDialogues[i].SeparationString, pastDialogues[i].Content);
				contentToConcatenate.Insert (0, content);

				if (i < pastDialogues.Count && !pastDialogues[i].IsConcatenated)
					break;
			}

			for (int i = 0; i < contentToConcatenate.Count; i++)
				concatenatedContent = string.Concat (concatenatedContent, pastDialogues[i].Content);

			return concatenatedContent;
		}
	}
}

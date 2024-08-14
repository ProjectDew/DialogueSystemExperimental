namespace DialogueSystem
{
	using System;
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;
	using AssetManagement;
	using EditorSections;
	using ExtensionMethods;

	public class DialoguesManagerSection : ISection
	{
		public DialoguesManagerSection (DialoguesManager window)
		{
			this.window = window;
		}

		private enum DisplayOption
		{
			[InspectorName ("Show descriptors")]
			ShowDescriptors,
			[InspectorName ("Show contents")]
			ShowContents
		};

		private DisplayOption displayOption = DisplayOption.ShowContents;

		[SerializeField]
		private DialoguesManager window;

		private DialogueNode[] dialogueNodes;
		private DialogueNodeProperties[] dialogueNodeProperties;
		private DialogueProperties[] dialogueProperties;

		private int[] currentContent;

		private Languages languages;

		private string[] languageNames;
		private int languageA;
		private int languageB;

		private int focusedDialogue;

		private Action[] drawSubheader;

		private Rect scrollArea;
		private float scrollPosition;
		private float fieldHeight;

		private GUIStyle textAreaStyle;
		private GUILayoutOption idWidth;

		public void Draw ()
		{
			if (dialogueProperties.IsNullOrEmpty ())
				FindDialogues ();

			if (languages == null)
			{
				languages = new ();

				if (languages.Count > 1)
					languageB = 1;
			}

			if (dialogueProperties.IsNullOrEmpty () || languages == null)
				return;

			textAreaStyle = new (EditorStyles.textArea);
			textAreaStyle.wordWrap = true;

			idWidth = GUILayout.Width (Screen.width * 0.15f);

			drawSubheader = new Action[] { DrawEmptySpace, DrawDisplayOption, DrawUpdateDialogues };
			Layout.InlinedContent (drawSubheader);

			Layout.GreyLine (10);

			GUILayout.Space (10);

			DrawHeader ();

			GUILayout.Space (10);

			GUILayout.BeginVertical ();

			GUILayout.BeginHorizontal ();

			fieldHeight = 20;

			float scrollbarHeight = window.position.height - scrollArea.yMin;
			float marginBetweenFields = 2;

			int nodesDisplayed = (int)(scrollbarHeight / (fieldHeight + marginBetweenFields));

			if (nodesDisplayed > dialogueNodeProperties.Length)
				nodesDisplayed = dialogueNodeProperties.Length;

			Event e = Event.current;

			if (e.type == EventType.ScrollWheel)
			{
				scrollPosition += e.delta.y;
				window.Repaint ();
			}

			Rect scrollbarRect = new (window.position.width - 16, scrollArea.yMin, 16, scrollbarHeight);

			scrollPosition = GUI.VerticalScrollbar (scrollbarRect, scrollPosition, nodesDisplayed, 0, dialogueNodeProperties.Length);
			scrollPosition = Math.Clamp (scrollPosition, 0, dialogueNodeProperties.Length - nodesDisplayed);

			int firstNode = (int)scrollPosition;
			int lastNode = firstNode + nodesDisplayed;
			
			lastNode = Math.Clamp (lastNode, lastNode, dialogueNodeProperties.Length);
			
			GUILayout.BeginVertical (GUILayout.Width (Screen.width - 24));

			for (int i = firstNode; i < lastNode; i++)
				DrawDialogueNode (i);
			
			GUILayout.EndVertical ();

			GUILayout.EndHorizontal ();

			GUILayout.EndVertical ();

			if (Event.current.type == EventType.Repaint)
				scrollArea = GUILayoutUtility.GetLastRect ();
		}

		private void DrawUpdateDialogues ()
		{
			Layout.Button ("Update list", FindDialogues, 120);
		}

		private void DrawDisplayOption ()
		{
			displayOption = (DisplayOption)EditorGUILayout.EnumPopup (displayOption, Styles.CenteredText (EditorStyles.popup));
		}

		private void DrawEmptySpace ()
		{
			GUILayout.Label ("", GUILayout.Width (120));
		}

		private void FindDialogues ()
		{
			dialogueNodes = Assets.FindAll<DialogueNode> (SearchOption.All);

			dialogueNodeProperties = new DialogueNodeProperties[dialogueNodes.Length];
			dialogueProperties = new DialogueProperties[dialogueNodes.Length];

			if (currentContent == null || currentContent.Length != dialogueNodeProperties.Length)
				currentContent = new int[dialogueNodeProperties.Length];

			for (int i = 0; i < dialogueNodes.Length; i++)
			{
				if (dialogueNodes[i] == null)
				{
					Debug.LogError (dialogueNodes[i] + "is null");
					continue;
				}

				dialogueNodeProperties[i] = new (dialogueNodes[i]);

				UpdateDialogue (i);
			}
		}

		private void UpdateDialogue (int index)
		{
			if (currentContent[index] >= dialogueNodeProperties[index].Dialogues.Count)
			{
				dialogueProperties[index] = null;
				return;
			}

			Dialogue dialogue = (Dialogue)dialogueNodeProperties[index].Dialogues.ObjectReference (currentContent[index]);

			if (dialogue == null)
			{
				dialogueProperties[index] = null;
				return;
			}

			dialogueProperties[index] = new (dialogue);
		}

		private void DrawHeader ()
		{
			GUIStyle popupStyle = Styles.CenteredText (EditorStyles.popup);

			if (languageNames.IsNullOrEmpty ())
				languageNames = languages.Copy ();

			if (languageA < 0 || languageA >= languages.Count)
				languageA = 0;

			if (languageB < 0 || languageB >= languages.Count)
				languageB = 0;

			GUILayout.BeginHorizontal ();

			GUILayout.Label ("ID", Styles.BoldCenteredText (), idWidth);

			GUILayout.Label ("Index", Styles.BoldCenteredText (), GUILayout.Width (64));
			
			if (languageNames.Length > 0)
			{
				Layout.CenteredContent (() => languageA = EditorGUILayout.Popup (languageA, languageNames, popupStyle));

				if (languages.Count > 1)
					Layout.CenteredContent (() => languageB = EditorGUILayout.Popup (languageB, languageNames, popupStyle));
			}

			GUILayout.EndHorizontal ();
		}

		private void DrawDialogueNode (int index)
		{
			if (dialogueNodeProperties[index] == null)
			{
				GUILayout.Label ("Null node", Styles.ItalicCenteredText ());
				return;
			}

			GUILayout.BeginHorizontal ();

			EditorGUI.BeginChangeCheck ();

			string id = dialogueNodeProperties[index].ID.stringValue;
			id = EditorGUILayout.TextField (id, Styles.BoldText (EditorStyles.textField), idWidth);

			if (EditorGUI.EndChangeCheck ())
			{
				if (dialogueNodeProperties[index].SerializedObject == null || dialogueNodeProperties[index].TargetObject == null)
					return;

				dialogueNodeProperties[index].SerializedObject.Update ();
				dialogueNodeProperties[index].ID.stringValue = id;
				dialogueNodeProperties[index].SerializedObject.ApplyModifiedProperties ();
			}

			if (currentContent[index] > 0)
				Layout.Button ("<", () => SetPreviousContent (index), 22);
			else
				GUILayout.Label ("", GUILayout.Width (20));

			SetCurrentContent (index);
			
			if (currentContent[index] < dialogueNodeProperties[index].Dialogues.Count - 1)
				Layout.Button (">", () => SetNextContent (index), 22);
			else
				GUILayout.Label ("", GUILayout.Width (20));
			
			DrawDialogue (index);

			GUILayout.EndHorizontal ();
		}

		private void DrawDialogue (int index)
		{
			if (dialogueProperties[index] == null)
			{
				GUILayout.Label ("Null dialogue", Styles.ItalicCenteredText ());
				return;
			}

			SerializedArray array = (displayOption == DisplayOption.ShowDescriptors) ? dialogueProperties[index].Descriptor : dialogueProperties[index].Content;

			DrawDialogueContent (array, index, languageA);

			if (languages.Count < 2)
				return;
				
			DrawDialogueContent (array, index, languageB);
		}

		private void DrawDialogueContent (SerializedArray array, int dialogueIndex, int contentIndex)
		{
			if (contentIndex >= array.Count || languageNames.Length == 0)
			{
				GUILayout.Label ("No language available", Styles.ItalicCenteredText ());
				return;
			}

			EditorGUI.BeginChangeCheck ();

			string content = array.String (contentIndex);

			GUI.SetNextControlName (dialogueIndex.ToString ());

			if (GUI.GetNameOfFocusedControl () == dialogueIndex.ToString ())
				focusedDialogue = dialogueIndex;

			if (dialogueIndex == focusedDialogue)
				content = EditorGUILayout.TextArea (content, textAreaStyle);
			else
				content = EditorGUILayout.TextArea (content, textAreaStyle, GUILayout.Height (fieldHeight));

			if (EditorGUI.EndChangeCheck ())
			{
				if (dialogueProperties[dialogueIndex].SerializedObject == null || dialogueProperties[dialogueIndex].TargetObject == null)
					return;

				dialogueProperties[dialogueIndex].SerializedObject.Update ();
				array.Set (contentIndex, content);
				dialogueProperties[dialogueIndex].SerializedObject.ApplyModifiedProperties ();
			}
		}

		private void SetPreviousContent (int index)
		{
			if (currentContent[index] <= 0)
				return;

			currentContent[index]--;

			UpdateDialogue (index);
		}

		private void SetCurrentContent (int index)
		{
			EditorGUI.BeginChangeCheck ();

			int value = currentContent[index];

			value = EditorGUILayout.IntField (value, GUILayout.Width (20));
			value = Mathf.Clamp (value, 0, dialogueNodeProperties[index].Dialogues.Count - 1);

			if (!EditorGUI.EndChangeCheck ())
				return;

			currentContent[index] = value;

			UpdateDialogue (index);
		}

		private void SetNextContent (int index)
		{
			if (currentContent[index] >= dialogueNodeProperties[index].Dialogues.Count - 1)
				return;

			currentContent[index]++;

			UpdateDialogue (index);
		}
	}
}

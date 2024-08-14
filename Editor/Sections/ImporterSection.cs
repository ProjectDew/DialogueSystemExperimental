namespace DialogueSystem
{
	using System.IO;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;
	using AssetManagement;
	using EditorSections.Presets;
	using ExtensionMethods;

	public class ImporterSection : CSVImporter
	{
		private enum Column { ID = 0, IsNode = 1, IsBranch = 2, Parents = 3, Children = 4, FirstLanguage = 5 }

		private static readonly string isTrue = true.ToString ();
		private static readonly string descriptorMarker = "{<--DESCRIPTOR};";

		private Languages languages;

		private string[][] importedContent;
		private Dictionary<string, DialogueNode> existingNodes;
		
		private string isBranch;

		private string folderPath;

		protected override void ProcessImportedContent ()
		{
			importedContent = GetImportedContent ();

			if (importedContent.IsNullOrEmpty ())
			{
				Debug.LogError ("There is nothing to import.");
				return;
			}
			
			folderPath = Assets.GetCurrentFolder ();

			CreateFolder (folderPath);

			languages = new ();
			existingNodes = new ();

			ProcessContent (CreateNodes);
			ProcessContent (CreateSubassets);

			ProcessContent (ConnectNodes);

			AssetDatabase.Refresh ();
		}

		private void CreateFolder (string folderPath)
		{
			if (!Directory.Exists (folderPath))
				Directory.CreateDirectory (folderPath);
		}

		private void ProcessContent (System.Action<int, string, string> processContent)
		{
			int firstNonReservedRow = 2;

			string id;
			string isNode;

			for (int i = firstNonReservedRow; i < importedContent.Length; i++)
			{
				id = importedContent[i][(int)Column.ID];
				isNode = importedContent[i][(int)Column.IsNode];
				isBranch = importedContent[i][(int)Column.IsBranch];

				processContent (i, id, isNode);
			}
		}

		private void CreateNodes (int index, string id, string isNode)
		{
			if (id.IsNullOrEmpty ())
				return;
				
			if (isNode.IsNullOrEmpty () || isNode != isTrue)
				return;

			DialogueNode dialogueNode = ScriptableObject.CreateInstance<DialogueNode> ();

			existingNodes.Add (id, dialogueNode);

			SerializedObject serializedObject = new (dialogueNode);
			DialogueNodeProperties dialogueNodeProperties = new (serializedObject);
				
			serializedObject.Update ();
				
			SetDialogueProperties (serializedObject, index);
				
			dialogueNodeProperties.ID.stringValue = id;
			dialogueNodeProperties.IsBranch.boolValue = isBranch == isTrue;
			dialogueNodeProperties.Dialogues.Add (serializedObject.targetObject);

			serializedObject.ApplyModifiedProperties ();

			string assetPath = string.Concat (folderPath, id, ".asset");

			AssetDatabase.CreateAsset (dialogueNode, assetPath);
			AssetDatabase.Refresh ();
		}

		private void CreateSubassets (int index, string id, string isNode)
		{
			if (id.IsNullOrEmpty () || isNode == isTrue)
				return;

			Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue> ();
			dialogue.name = id;

			SerializedObject serializedObject = new (dialogue);
				
			serializedObject.Update ();
				
			SetDialogueProperties (serializedObject, index);

			serializedObject.ApplyModifiedProperties ();

			string assetPath = string.Concat (folderPath, id, ".asset");

			AssetDatabase.AddObjectToAsset (dialogue, assetPath);

			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();

			AddDialogueToNode (id, dialogue);
		}

		private void ConnectNodes (int index, string id, string isNode)
		{
			if (id.IsNullOrEmpty ())
				return;
				
			if (isNode.IsNullOrEmpty () || isNode != isTrue)
				return;
				
			SerializedObject serializedObject = new (existingNodes[id]);
			DialogueNodeProperties dialogueNodeProperties = new (serializedObject);
				
			serializedObject.Update ();

			string parents = importedContent[index][(int)Column.Parents];
			string children = importedContent[index][(int)Column.Children];

			SetNodeRelationships (dialogueNodeProperties.Parents, parents);
			SetNodeRelationships (dialogueNodeProperties.Children, children);

			serializedObject.ApplyModifiedProperties ();
		}

		private void SetDialogueProperties (SerializedObject serializedObject, int row)
		{
			DialogueProperties dialogueProperties = new (serializedObject);

			int firstLanguage = (int)Column.FirstLanguage;

			for (int i = firstLanguage; i < importedContent.ColumnCount (); i++)
			{
				string language = importedContent[0][i];

				if (!languages.Contains (language))
					languages.Add (language);

				int endOfDescriptor = importedContent[row][i].IndexOf (descriptorMarker);
				int startOfContent = (endOfDescriptor >= 0) ? endOfDescriptor + descriptorMarker.Length : 0;

				string descriptor = (endOfDescriptor >= 0) ? importedContent[row][i][..endOfDescriptor] : "";
				string content = importedContent[row][i][startOfContent..];

				dialogueProperties.LanguageNames.Add (language);
				dialogueProperties.LanguageIDs.Add (languages.GetID (language));
				dialogueProperties.Descriptor.Add (descriptor);
				dialogueProperties.Content.Add (content);
			}
		}

		private void AddDialogueToNode (string id, Dialogue dialogue)
		{
			DialogueNodeProperties dialogueNodeProperties = new (existingNodes[id]);

			dialogueNodeProperties.SerializedObject.Update ();
			dialogueNodeProperties.Dialogues.Add (dialogue);
			dialogueNodeProperties.SerializedObject.ApplyModifiedProperties ();
		}

		private void SetNodeRelationships (SerializedArray array, string data)
		{
			if (array.Count != 0 || data.IsNullOrEmpty ())
				return;

			if (!data.Contains (';'))
			{
				if (existingNodes.ContainsKey (data))
					array.Add (existingNodes[data]);

				return;
			}	
			
			string[] IDs = data.Split (';');

			for (int i = 0; i < IDs.Length; i++)
			{
				string key = IDs[i];

				if (key.IsNullOrEmpty () || !existingNodes.ContainsKey (key))
					continue;

				array.Add (existingNodes[key]);
			}
		}
	}
}

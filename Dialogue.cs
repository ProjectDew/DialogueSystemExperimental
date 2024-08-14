namespace DialogueSystem
{
	using UnityEngine;

	public class Dialogue : ScriptableObject
	{
		[SerializeField]
		private string[] languageIDs;
		
		[SerializeField]
		private string[] languageNames;

		[SerializeField]
		private string[] descriptor;

		[SerializeField]
		private string[] content;

		public virtual int TotalLanguages => languageNames.Length;

		public virtual string GetDescriptor (string language)
		{
			for (int i = 0; i < languageNames.Length; i++)
			{
				if (language != languageNames[i])
					continue;

				return descriptor[i];
			}

			return string.Empty;
		}

		public virtual string GetContent (string language)
		{
			for (int i = 0; i < languageNames.Length; i++)
			{
				if (language != languageNames[i])
					continue;
				
				return content[i];
			}

			return string.Empty;
		}
	}
}

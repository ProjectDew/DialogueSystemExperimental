namespace DialogueSystem
{
    public class VariableProcessor : ITextProcessor
    {
		public VariableProcessor (object variable, params object[] additionalVariables)
		{
			int offset = 1;

			variables = new object[additionalVariables.Length + offset];

			variables[0] = variable;

			for (int i = 0; i < additionalVariables.Length; i++)
			{
				int index = i + offset;
				variables[index] = additionalVariables[i];
			}
		}

		private readonly object[] variables;

		public string ProcessText (string text) => string.Format (text, variables);
	}
}

namespace DialogueSystem
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using TMPro;
	using ExtensionMethods;

	[RequireComponent (typeof (TMP_Text))]
	public class ContentReader : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text descriptorText;

		[SerializeField]
		private float characterDelay;

		[SerializeField]
		private OverrideDelays overrideDelays;

		[Serializable]
		private class OverrideDelays
		{
			[SerializeField]
			private char[] characters;

			[SerializeField]
			private float[] delays;

			public int Length => delays.Length;

			public char GetCharacterAt (int index)
			{
				if (index < 0 || index >= characters.Length)
					throw new IndexOutOfRangeException ("Index out of range.");

				return characters[index];
			}

			public float GetDelayAt (int index)
			{
				if (index < 0 || index >= delays.Length)
					throw new IndexOutOfRangeException ("Index out of range.");

				return delays[index];
			}
		}

		private enum State { Waiting, Starting, Reading, Finishing }

		private State currentState;
		
		private TMP_Text contentText;
		
		private Dictionary<char, float> delayTimes;
		private char[] characters;
		private char character;
		private float delayTime;

		private string preexistingContent;
		private string contentToRead;
		private string descriptor;

		private float delayCounter;
		private float textSpeed = 1;

		private int characterIndex;
		private int textDirection;
		private int previousDirection;

		private EventHandler startReading;
		private EventHandler finishReading;

		private Dictionary<int, EventHandler> interceptionList;
		private Dictionary<string, EventHandler> textInterceptionList;

		public float TextSpeed
		{
			get => textSpeed;
			set
			{
				float minSpeed = 0.0f;
				float maxSpeed = 1.0f;

				value = Mathf.Clamp (value, minSpeed, maxSpeed);

				textSpeed = value;
			}
		}

		private int TextDirection
		{
			get => textDirection;
			set
			{
				if (value > 0)
					value = 1;
				else if (value < 0)
					value = -1;

				if (textDirection == value)
					return;

				previousDirection = textDirection;
				textDirection = value;
			}
		}

		public bool HasFinishedReading { get; private set; }

		private void Awake ()
		{
			contentText = GetComponent<TMP_Text> ();

			interceptionList = new ();
			textInterceptionList = new ();

			InitializeDelayTimes ();

			currentState = State.Waiting;
		}

		private void Update ()
		{
			if (currentState == State.Starting)
				StartReading ();
			else if (currentState == State.Reading)
				Read ();
			else if (currentState == State.Finishing)
				FinishReading ();
		}

		public void Read (string contentToRead, string descriptor = "")
		{
			preexistingContent = "";

			this.contentToRead = contentToRead;
			this.descriptor = descriptor;
			
			TextDirection = 1;

			currentState = State.Starting;
		}

		public void Read (string preexistingContent, string contentToRead, string descriptor = "")
		{
			this.preexistingContent = preexistingContent;
			this.contentToRead = contentToRead;
			this.descriptor = descriptor;
			
			TextDirection = 1;
			
			currentState = State.Starting;
		}

		public void Unread (string contentToRead, string descriptor = "")
		{
			preexistingContent = "";
			this.contentToRead = contentToRead;
			this.descriptor = descriptor;
			
			TextDirection = -1;
			
			currentState = State.Starting;
		}

		public void Unread (string preexistingContent, string contentToRead, string descriptor = "")
		{
			this.preexistingContent = preexistingContent;
			this.contentToRead = contentToRead;
			this.descriptor = descriptor;
			
			TextDirection = -1;
			
			currentState = State.Starting;
		}

		public void Pause ()
		{
			TextDirection = 0;
		}

		public void Resume ()
		{
			TextDirection = previousDirection;
		}

		public void Rewind ()
		{
			if (HasFinishedReading)
			{
				Unread (preexistingContent, contentToRead, descriptor);
				return;
			}

			TextDirection = -1;
		}

		public void Forward ()
		{
			if (HasFinishedReading)
			{
				Read (preexistingContent, contentToRead, descriptor);
				return;
			}

			TextDirection = 1;
		}

		public void GoToStart ()
		{
			contentText.text = preexistingContent;

			if (descriptorText != null)
				descriptorText.text = "";

			currentState = State.Finishing;
		}

		public void GoToEnd ()
		{
			contentText.text = string.Concat (preexistingContent, contentToRead);

			if (descriptorText != null)
				descriptorText.text = descriptor;

			currentState = State.Finishing;
		}

		public void SubscribeToStartReading (EventHandler eventHandler)
		{
			startReading += eventHandler;
		}

		public void UnsubscribeFromStartReading (EventHandler eventHandler)
		{
			startReading -= eventHandler;
		}

		public void SubscribeToInterceptText (int indexToIntercept, EventHandler eventHandler)
		{
			if (interceptionList.ContainsKey (indexToIntercept))
				interceptionList[indexToIntercept] += eventHandler;
			else
				interceptionList.Add (indexToIntercept, eventHandler);
		}

		public void SubscribeToInterceptText (string textToIntercept, EventHandler eventHandler)
		{
			if (textInterceptionList.ContainsKey (textToIntercept))
				textInterceptionList[textToIntercept] += eventHandler;
			else
				textInterceptionList.Add (textToIntercept, eventHandler);
		}

		public void UnsubscribeFromInterceptText (int index, EventHandler eventHandler)
		{
			interceptionList[index] -= eventHandler;

			Delegate[] invocationList = interceptionList[index].GetInvocationList ();

			if (invocationList.Length == 0)
				interceptionList.Remove (index);
		}

		public void UnsubscribeFromInterceptText (string textToIntercept, EventHandler eventHandler)
		{
			foreach (KeyValuePair<int, EventHandler> item in interceptionList)
				if (item.Value.Equals (eventHandler))
					interceptionList[item.Key] -= eventHandler;

			textInterceptionList[textToIntercept] -= eventHandler;

			Delegate[] invocationList = textInterceptionList[textToIntercept].GetInvocationList ();

			if (invocationList.Length == 0)
				textInterceptionList.Remove (textToIntercept);
		}

		public void SubscribeToFinishReading (EventHandler eventHandler)
		{
			finishReading += eventHandler;
		}

		public void UnsubscribeFromFinishReading (EventHandler eventHandler)
		{
			finishReading -= eventHandler;
		}

		private void OnInterceptText (int indexToIntercept)
		{
			if (!interceptionList.ContainsKey (characterIndex))
				return;

			interceptionList[indexToIntercept]?.Invoke (this, EventArgs.Empty);
		}

		private void OnStartReading ()
		{
			startReading?.Invoke (this, EventArgs.Empty);
		}

		private void OnFinishReading ()
		{
			HasFinishedReading = true;
			finishReading?.Invoke (this, EventArgs.Empty);
		}

		private void InitializeDelayTimes ()
		{
			delayTimes = new ();

			for (int i = 0; i < overrideDelays.Length; i++)
			{
				char character = overrideDelays.GetCharacterAt (i);
				float delay = overrideDelays.GetDelayAt (i);

				delayTimes.Add (character, delay);
			}
		}

		private void AddTextInterceptionIndices (string content, string textToIntercept, EventHandler eventHandler)
		{
			if (textToIntercept.IsNullOrEmpty ())
				return;

			int startIndex = 0;

			while (startIndex >= 0)
			{
				int interceptionIndex = content.IndexOf (textToIntercept, startIndex);

				if (interceptionIndex >= 0)
					SubscribeToInterceptText (interceptionIndex, eventHandler);

				startIndex = interceptionIndex;
			}
		}

		private void StartReading ()
		{
			foreach (KeyValuePair<string, EventHandler> item in textInterceptionList)
				AddTextInterceptionIndices (contentToRead, item.Key, item.Value);

			HasFinishedReading = false;

			if (descriptorText != null && !descriptor.IsNullOrEmpty ())
				descriptorText.text = descriptor;

			if (contentText == null)
				return;

			OnStartReading ();

			characters = new char[contentToRead.Length];

			if (contentToRead.Length == 0)
			{
				contentText.text = preexistingContent;
				
				OnFinishReading ();

				return;
			}
			
			contentText.text = preexistingContent;

			if (characterDelay == 0 && overrideDelays.Length == 0)
			{
				if (TextDirection >= 0)
				{
					contentText.text += contentToRead;
					characterIndex = characters.Length - 1;
				}
				else
				{
					characterIndex = 0;
				}
				
				OnFinishReading ();

				return;
			}
			
			delayTime = 0;
			characterIndex = (TextDirection >= 0) ? 0 : characters.Length - 1;

			currentState = State.Reading;
		}

		private void Read ()
		{
			if (characterIndex < 0 || characterIndex >= characters.Length)
			{
				currentState = State.Finishing;
				return;
			}

			if (TextDirection == 0)
				return;

			delayCounter += Time.deltaTime;
				
			if (delayCounter < delayTime * TextSpeed)
				return;

			characters[characterIndex] = contentToRead[characterIndex];
			character = characters[characterIndex];
				
			OnInterceptText (characterIndex);

			if (TextDirection > 0)
				contentText.text += character.ToString ();
			else
				contentText.text.Remove (contentText.text.Length - 1);

			characterIndex += TextDirection;

			if (delayTimes.Count > 0 && delayTimes.ContainsKey (character))
				delayTime = delayTimes[character];
			else
				delayTime = characterDelay;

			delayCounter = 0;
		}

		private void FinishReading ()
		{
			OnFinishReading ();
			currentState = State.Waiting;
		}
	}
}

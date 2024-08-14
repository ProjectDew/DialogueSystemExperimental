# DialogueSystem

A dialogue system based on scriptable objects that I designed for personal use. It is still a work in progress, so some of its functionalities need more testing and it's subject to changes in general, but I've used it in a couple of minigames and I'm happy with it so far.

Dependencies: [EditorPack](https://github.com/ProjectDew/EditorPack), [CSVUtility](https://github.com/ProjectDew/CSVUtility), [ExtensionMethods](https://github.com/ProjectDew/ExtensionMethods).

## Runtime scripts

- ***Dialogue***: a ScriptableObject containing both the dialogue that will be displayed ingame and a descriptor that can (optionally) be displayed too, both of them available in as many languages as desired.

  - ***TotalLanguages***: returns the amount of languages that this dialogue is available in.
  - ***GetDescriptor***: returns the descriptor of the dialogue.
  - ***GetContent***: returns the content of the dialogue.

- ***DialogueNode***: the main element of the dialogue system. It's a ScriptableObject that inherits from the Dialogue class. It must contain at least one Dialogue (itself) and can hold an unlimited amount of them. It can also be linked to as many other nodes as required, either as a parent or as a child.

  - ***ID***: returns a string that serves to identify the node. It should be unique.
  - ***IsBranch***: returns true if this node is a branch that the player must select to continue and false otherwise.
  - ***TotalDialogues***: returns the amount of dialogues contained in this node.
  - ***TotalParents***: returns the amount of nodes that can lead to this one.
  - ***TotalChildren***: returns the amount of nodes that this one can lead into.
  - ***GetDialogue***: returns the desired Dialogue object.
  - ***GetDescriptor (overload)***: returns the descriptor of the desired dialogue.
  - ***GetContent (overload)***: returns the content of the desired dialogue.
  - ***GetParent***: returns the desired parent.
  - ***GetParentAt***: returns the parent at the given index.
  - ***GetParents***: returns an array containing all the parents of this node.
  - ***GetChild***: returns the desired child.
  - ***GetChildAt***: returns the child at the given index.
  - ***GetChildren***: return an array containing all the children of this node.

- ***ContentReader***: an optional class that requires a TMP_Text component. It allows to control how the dialogue is read and displayed.

  - ***TextSpeed***: returns and sets the speed at which the text will appear on screen. The value is clamped between 0 and 1.
  - ***HasFinishedReading***: returns true if the content has been fully read and displayed on screen, and false if not.
  - ***Read (2 overloads)***: fills the TMP_Text with the content provided. Optionally, it can set the descriptor text too.
  - ***Unread (2 overloads)***: empties the TMP_Text, storing the content provided. Optionally, it can delete the descriptor text too.
  - ***Pause***: interrupts the reading (or unreading) of the text.
  - ***Resume***: keeps reading or unreading the text from the point where it was interrupted.
  - ***Rewind***: starts emptying the text.
  - ***Forward***: starts filling the text.
  - ***GoToStart***: empties the text and stops reading.
  - ***GoToEnd***: fills the text and stops reading.
  - ***SubscribeToStartReading***: subscribes to an event that triggers right before reading the text.
  - ***UnsubscribeFromStartReading***: unsubscribes from an event that triggers right before reading the text.
  - ***SubscribeToInterceptText (2 overloads)***: subscribes to an event that triggers when a given index or string is read.
  - ***UnsubscribeFromInterceptText (2 overloads)***: unsubscribes from an event that triggers when a given index or string is read.
  - ***SubscribeToFinishReading***: subscribes to an event that triggers right after reading the text.
  - ***UnsubscribeFromFinishReading***: unsubscribes from an event that triggers right after reading the text.

- ***ITextProcessor***: interface that allows to process the content of a dialogue before it is displayed on screen.

  - ***ProcessText***: accepts a string as a parameter and returns the processed one.

- ***VariableProcessor***: implements ITextProcessor. It inserts the value of the object or objects provided in the constructor into the given string.

- ***NodeManager***: the main controller of this dialogue system. It has references to the desired TMP_Text components and the content readers associated to them (if any). It may or may not contain references to the first nodes that are going to be read in each narrative sequence. If it does, the nodes can be read using their IDs; if it doesn't, they must be provided at the time of reading.

  - ***MainText***: returns the TMP_Text that will display the main dialogue.
  - ***MainReader***: returns the ContentReader associated to the main TMP_Text.
  - ***CurrentNodeInfo***: returns a class that encapsulates relevant information about the last node that was read.
  - ***TotalBranches***: returns the amount of TMP_Text components used to display branching dialogues.
  - ***TextSpeed***: returns the speed of the current content reader. It's also a setter, which changes the speed of all the available readers (main and branches).
  - ***HasFinishedReading***: returns true if the current text is fully read and false if not.
  - ***GetBranchText***: returns the desired TMP_Text from the list of branches.
  - ***GetBranchReader***: returns the desired ContentReader from the list of branches.
  - ***SetLanguage***: sets the language of the dialogues.
  - ***ReadDialogue (6 overloads)***: displays the desired dialogue on the corresponding TMP_Text, using its ContentReader if it has one.
  - ***UnreadDialogue (6 overloads)***: empties the TMP_Text that is used to display the desired dialogue, using its ContentReader to rewind the text if it has one.
  - ***ReadPrevious***: displays the previous dialogue on the corresponding TMP_Text, using its ContentReader if it has one.
  - ***UnreadPrevious***: goes back to the previous dialogue and empties the TMP_Text that displayed it, using its ContentReader to rewind the text if it has one.
  - ***ReadNext***: finds the next dialogue and displays it on the corresponding TMP_Text, using its ContentReader if it has one.
  - ***ConcatenateNext***: finds the next dialogue and adds it at the end of the current one, using its ContentReader if it has one.
  - ***SelectBranch***: finds the next dialogue in the chosen branch and displays it on the corresponding TMP_Text, using its ContentReader if it has one.

## Editor scripts

- ***Languages***: it implements the IList interface (of type string) from the System.Collections.Generic namespace. It's essentially a list of languages with some extra logic for serialization and deserialization.

### Inspectors

- ***DialogueInspector***

  - ***Asset info***: a button that opens a window that contains information about the asset (name, GUID, ID, flags and parent node).
  - ***Language***: a dropdown menu that lets you choose the language whose contents you want to see or edit.
  - ***Descriptor***: a text field that you can use to add some extra information. The descriptor can (optionally) be displayed ingame as an independent dialogue.
  - ***Content***: a text area containing the main text that will be displayed ingame.

- ***DialogueNodeInspector***

  - ***Dialogue ID***: the identifier of the node. By default, it's the name of the asset.
  - ***Manager***: a button that opens the DialoguesManager window.
  - ***Asset info***: a button that opens a window with information about the asset (name, GUID, ID, flags) and all its subassets (name, GUID, ID, flags, parent node).
  - ***Languages***: a button that opens the LanguageEditor window.
  - ***Content***: a section where you can see and edit the dialogues contained in this node (one at a time). It has controls that allow you to choose what Dialogue class will be displayed and in which language, as well as inserting a new dialogue or deleting the current one (which is not possible if the current dialogue is the first).
  - ***Mark as a branch dialogue***: a toggle button that indicates if the node is a branch or not.
  - ***Prev. dialogues***: a list of all the nodes that lead to this one (if any).
  - ***Next dialogues***: a list of all the nodes that this one leads into (if any).

- ***ContentReaderInspector***

  - ***Descriptor text (optional)***: the TMP_Text that will display the descriptor of the dialogue.
  - ***Default delay between characters***: the time (in seconds) that will pass before the next character in the string is displayed.
  - ***Different delays for specific characters***: same as above, but the values specified here have priority.

- ***NodeManagerInspector***

  - ***Main text***: the TMP_Text that will display all the dialogues that are not marked as branches.
  - ***Branches***: the TMP_Texts that will display the dialogues marked as branches.
  - ***Nodes***: a list of the nodes handled by this NodeManager (you just need to assign one node for each sequence of dialogues, and that node will be the first read).

### Tools

- ***LanguageEditor***: it allows to add new languages to the project, as well as rename and remove the existing ones. Any changes made here must be explicitly applied using the corresponding button.
- ***ConfirmLanguageRemoval***: it asks for confirmation before removing a language and offers the option to export the dialogues before the language is removed.
- ***DialoguesManager***: it contains a list of all the dialogues in the project, optimized to handle very large amounts of them (hundreds, thousands or even more) at the same cost than handling just a few dozens. You can choose whether you want to see/edit their descriptors or their contents, and in which languages you want them to be displayed (up to two at the same time).
- ***DialoguesExporter***: it creates a CSV document that contains all the information about all the dialogues that you want to export.
- ***DialoguesImporter***: inherits from CSVImporter (a class inside my EditorSections.Presets namespace). It reads a CSV document and creates all the dialogues retrieved.

### Sections

The encapsulated sections of the inspectors and tools above.

### Properties

The encapsulated properties used in the inspectors above.

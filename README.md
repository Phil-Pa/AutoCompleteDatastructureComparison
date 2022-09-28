# What is this about?

The project tries to highlight the advantages and disadvantages of three different data structures for the task to recommend words for text autocompletion. The three different data structures are trie (search tree), hashmap (in C# Dictionary) and a usual list. It turned out, the list was the worst, while the hashmap was very quick to find out if a word has been added so the words that can be recommended, but quiet bad at recommending words for autocompletion. The trie turned out to be the best of the three for this task.

## How to run:

Before running, change the path of the word list in Program.cs in line 180, otherwise it will not find the file and throw an exception.
Then run ```dotnet run -c Release``` to run the benchmarks. It might take 5 to 10 minutes.

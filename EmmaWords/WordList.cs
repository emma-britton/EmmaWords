

using System.Collections;
using System.IO;

namespace EmmaWords
{
    public class WordList : IEnumerable<string>
    {
        private readonly SortedSet<string> Words = new();
        private readonly SortedSet<string> RemovedWords = new();
        private readonly Dictionary<string, int> BonusPoints = new();

        public string Name { get; }


        private WordList(string name)
        {
            Name = name;
        }


        public static WordList ReadFromFile(string filename)
        {
            Console.WriteLine("loading " + filename);

            var wordList = new WordList(Path.GetFileNameWithoutExtension(filename));
            
            foreach (string line in File.ReadAllLines(filename))
            {
                string word = line.ToUpper().Trim();

                if (word != "" && !word.StartsWith('#'))
                {
                    if (word.Contains(','))
                    {
                        string actualWord = word[..word.IndexOf(',')];
                        int bonus = int.Parse(word[(word.IndexOf(',') + 1)..]);

                        wordList.Words.Add(actualWord);
                        wordList.BonusPoints[actualWord] = bonus;
                    }
                    else
                    {
                        wordList.Words.Add(word);
                    }
                }
            }

            Console.WriteLine(wordList.Words.Count + " words loaded");

            string removedFile = Path.Combine(Path.GetDirectoryName(filename) ?? "",
                Path.GetFileNameWithoutExtension(filename) + "-removed.txt");

            if (File.Exists(removedFile))
            {
                foreach (string line in File.ReadAllLines(removedFile))
                {
                    string removedWord = line.ToUpper().Trim();

                    if (removedWord != "" && !removedWord.StartsWith('#'))
                    {
                        wordList.RemovedWords.Add(removedWord);
                        wordList.Words.Remove(removedWord);
                    }
                }
            }

            return wordList;
        }


        public static WordList Combine(IEnumerable<WordList> lists)
        {
            Console.WriteLine("generating combined word list");
            var combinedList = new WordList("All");

            foreach (var list in lists)
            {
                combinedList.Words.UnionWith(list.Words);
                combinedList.RemovedWords.UnionWith(list.RemovedWords);
            }

            Console.WriteLine(combinedList.Words.Count + " words in combined list");
            return combinedList;
        }


        public bool Contains(string word)
        {
            return Words.Contains(word);
        }


        public bool WasRemoved(string word)
        {
            return RemovedWords.Contains(word);
        }


        public int GetBonusPoints(string word)
        {
            if (BonusPoints.ContainsKey(word))
            {
                return BonusPoints[word];
            }

            return 0;
        }


        public override string ToString() => Name;
        
        public IEnumerator<string> GetEnumerator() => Words.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Words.GetEnumerator();
    }
}

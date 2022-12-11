using System.Collections;
using System.IO;

namespace EmmaWords
{
    public class WordListSet : IEnumerable<WordList>
    {
        private readonly Dictionary<string, WordList> WordLists = new();

        public WordList AllWords { get; }


        private WordListSet(Dictionary<string, WordList> wordLists)
        {
            WordLists = wordLists;
            AllWords = WordList.Combine(wordLists.Values);
        }


        public static WordListSet ReadFromFolder(string folder)
        {
            Console.WriteLine("loading word lists from " + folder);

            var wordLists = new Dictionary<string, WordList>();

            if (folder != null)
            {
                foreach (string file in Directory.GetFiles(folder))
                {
                    string name = Path.GetFileNameWithoutExtension(file);

                    if (!name.ToLower().Contains("removed"))
                    {
                        var wordList = WordList.ReadFromFile(file);
                        wordLists[name] = wordList;
                    }
                }
            }

            return new WordListSet(wordLists);
        }


        public WordList? GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            foreach (var item in WordLists.OrderByDescending(x => x.Key))
            {
                if (item.Key.ToLower().StartsWith(name.Trim().ToLower()))
                {
                    return item.Value;
                }
            }

            if (name.ToUpper() == "ALL")
            {
                return AllWords;
            }

            return null;
        }


        public IEnumerator<WordList> GetEnumerator() => WordLists.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => WordLists.Values.GetEnumerator();
    }
}

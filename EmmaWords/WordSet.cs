using System.Collections;
using System.Text;


namespace EmmaWords
{
    internal class WordSet : IEnumerable<Word>
    {
        public readonly Dictionary<string, Word> Words = new();
        public readonly decimal EmptyLeaveValue;
        public readonly Dictionary<string, HashSet<string>> Lookup = new();

        private readonly Dictionary<string, HashSet<string>> PartialLookup = new();
        private readonly RuleSet Rules;
        private readonly Random Random = new();


        internal WordSet(WordList wordList, RuleSet rules)
        {
            Console.WriteLine("calculating probabilities and leave values");
            Rules = rules;

            var offsets = new Dictionary<string, int>();
            var bag = new StringBuilder();

            foreach (var tile in rules.LetterDistribution)
            {
                offsets[tile.Key] = bag.Length;

                for (int i = 0; i < tile.Value; i++)
                {
                    bag.Append(tile.Key);
                }
            }

            foreach (var word in wordList)
            {   
                var wordObj = new Word(word);
                wordObj.EstimatedProbability = EstimateProbability(word);
                Words[word] = wordObj;

                var arr = word.ToCharArray();
                Array.Sort(arr);
                string sorted = new(arr);

                if (!Lookup.TryGetValue(sorted, out var hashset))
                {
                    Lookup[sorted] = hashset = new HashSet<string>();
                }

                hashset.Add(word);
                
                if (word.Length == 8)
                {
                    var arr8 = word.ToCharArray();
                    Array.Sort(arr8);

                    for (int i = 0; i < word.Length; i++)
                    {
                        var list = arr.ToList();
                        list.RemoveAt(i);
                        string sorted7 = new(list.ToArray());

                        if (!PartialLookup.TryGetValue(sorted, out var partialHashset))
                        {
                            PartialLookup[sorted] = partialHashset = new HashSet<string>();
                        }

                        partialHashset.Add(word);
                    }
                }
            }

            foreach (var group in Words.Values.GroupBy(w => w.Name.Length))
            {
                int count = group.Count();
                int i = 0;

                foreach (var word in group.OrderByDescending(w => w.EstimatedProbability))
                {
                    if (word.EstimatedProbability == 0)
                    {
                        word.RelativeProbability = 0;
                    }
                    else
                    {
                        word.RelativeProbability = (decimal)(count - i) / count;
                    }

                    i++;
                }
            }

            EmptyLeaveValue = EvaluateRack("");
        }

        public Word? GetWord(string word)
        {
            return Words.GetValueOrDefault(word);
        }

        public IEnumerator<Word> GetEnumerator()
        {
            return Words.Values.GetEnumerator();
        }

        public decimal RelativeProbability(string word)
        {
            if (Words.TryGetValue(word, out var wordObj))
            {
                return wordObj.RelativeProbability;
            }

            // Estimate what relative probability of non-dictionary word would be
            decimal estimate = EstimateProbability(word);

            foreach (var otherWord in Words.Values.Where(w => w.Name.Length == word.Length).OrderByDescending(w => w.EstimatedProbability))
            {
                if (otherWord.EstimatedProbability < estimate)
                {
                    return otherWord.RelativeProbability;
                }
            }

            return 0;
        }


        private decimal EstimateProbability(string word)
        {
            // This is only used to compare relative probabilities, so does 
            // not need to be accurate. The calculation is greatly over-simpliefied.
            decimal result = 1;

            var dist = new Dictionary<string, int>(Rules.LetterDistribution);
            int remaining = Rules.BagSize;
            int blanks = dist.GetValueOrDefault("?");

            foreach (char c in word)
            {
                if (!dist.TryGetValue(c.ToString(), out int count))
                {
                    return 0;
                }

                if (count == 0)
                {
                    if (blanks == 0) return 0;

                    result *= (decimal)blanks / remaining;
                    blanks--;
                }
                else
                {
                    result *= (decimal)count / remaining;
                    dist[c.ToString()] = count - 1;
                }

                remaining--;
            }

            return result;
        }


        public decimal EvaluateRack(string rack)
        {
            var bag = new List<string>();

            foreach (var item in Rules.LetterDistribution)
            {
                for (int i = 0; i < item.Value; i++)
                {
                    bag.Add(item.Key);
                }
            }

            int runs = 200000;
            int totalMatches = 0;

            for (int run = 1; run <= runs; run++)
            {
                var rackCopy = rack.ToList();
                var bagCopy = new List<string>(bag);
                var matches = new HashSet<string>();

                for (int tile = rackCopy.Count + 1; tile <= Rules.RackSize; tile++)
                {
                    int idx = Random.Next(0, bagCopy.Count - 1);
                    rackCopy.Add(bag[idx][0]);
                    bagCopy.RemoveAt(idx);
                }

                rackCopy.Sort();

                if (rackCopy.IndexOf('?') is int blank1 && blank1 >= 0)
                {
                    for (char c1 = 'A'; c1 <= 'Z'; c1++)
                    {
                        rackCopy[blank1] = c1;

                        if (rackCopy.LastIndexOf('?') is int blank2 && blank2 >= 0 && blank2 != blank1)
                        {
                            for (char c2 = 'A'; c2 <= 'Z'; c2++)
                            {
                                rackCopy[blank2] = c2;

                                var arr = rackCopy.ToArray();
                                Array.Sort(arr);
                                string rackString = new(arr);

                                if (Lookup.TryGetValue(rackString, out var hashset))
                                {
                                    foreach (var match in hashset)
                                    {
                                        matches.Add(match);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var arr = rackCopy.ToArray();
                            Array.Sort(arr);
                            string rackString = new(arr);

                            if (Lookup.TryGetValue(rackString, out var hashset))
                            {
                                foreach (var match in hashset)
                                {
                                    matches.Add(match);
                                }
                            }
                        }
                    }
                }
                else
                {
                    string rackString = new(rackCopy.ToArray());

                    if (Lookup.TryGetValue(rackString, out var hashset))
                    {
                        foreach (var match in hashset)
                        {
                            matches.Add(match);
                        }
                    }
                }

                totalMatches += matches.Count;
            }

            decimal averageScore = (decimal)totalMatches / runs;
            return averageScore;
        }


        public IEnumerable<string> Anagram(string alphagram)
        {
            return Lookup.GetValueOrDefault(alphagram, new());
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return Words.Values.GetEnumerator();
        }
    }
}

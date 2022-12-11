

namespace EmmaWords
{
    public class Word
    {
        public string Name;
        public decimal EstimatedProbability;
        public decimal RelativeProbability;


        public Word(string name)
        {
            Name = name;
        }


        public override string ToString() => Name;
    }
}

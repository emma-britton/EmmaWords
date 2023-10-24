using Emma.Lib;
using Emma.WordLearner.Properties;

namespace Emma.WordLearner;

public partial class WordLearnConfig : Form
{
    private readonly WordService m_WordService;
    private string? m_WordLearnFile;
    private string? m_AlphagramLearnFile;


    public WordLearnConfig(WordService wordService)
    {
        InitializeComponent();
        m_WordService = wordService;

        if (m_WordService.Lexicons.Count == 0)
        {
            MessageBox.Show("No lexicon data available", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        foreach (var lexicon in m_WordService.Lexicons)
        {
            LexiconList.Items.Add(lexicon);
        }

        if (m_WordService.GetLexicon(Settings.Default.Lexicon) is Lexicon lx)
        {
            LexiconList.SelectedItem = lx;
        }
        else if (LexiconList.Items.Count > 0)
        {
            LexiconList.SelectedIndex = 0;
        }

        ReviewPeriod.Value = Settings.Default.ReviewPeriod;
    }


    private void LexiconList_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateStats();
        Settings.Default.Lexicon = LexiconList.SelectedItem.ToString();
    }


    private void UpdateStats()
    {
        if (LexiconList.SelectedItem is not Lexicon lexicon) return;

        string learnFolder = Path.Combine(m_WordService.BaseFolder, "learn");
        Directory.CreateDirectory(learnFolder);

        m_WordLearnFile = Path.Combine(learnFolder, lexicon.Name + "-word-learn.txt");
        m_AlphagramLearnFile = Path.Combine(learnFolder, lexicon.Name + "-alphagram-learn.txt");

        if (File.Exists(m_WordLearnFile))
        {
            using var wordData = File.OpenRead(m_WordLearnFile);
            using var reader = new StreamReader(wordData);

            int learned = 0, progress = 0, missed = 0;

            while (!reader.EndOfStream && reader.ReadLine() is string line)
            {
                var fields = line.Trim().Split(',');

                if (fields.Length == 2)
                {
                    int score = int.Parse(fields[1]);

                    if (score >= 2)
                    {
                        learned++;
                    }
                    else if (score > 0)
                    {
                        progress++;
                    }
                    else
                    {
                        missed++;
                    }
                }
            }

            string stats = $"{learned}\u00a0words learned, {progress}\u00a0in progress, {missed}\u00a0missed";
            LexiconStats.Text = stats;
        }
        else
        {
            LexiconStats.Text = "Not yet started";
        }
    }


    private void Reset_Click(object sender, EventArgs e)
    {
        var prompt = MessageBox.Show("Really reset all progress?", "Emma Word Learning",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

        if (prompt == DialogResult.Yes)
        {
            if (File.Exists(m_WordLearnFile))
            {
                File.Delete(m_WordLearnFile);
            }

            if (File.Exists(m_AlphagramLearnFile))
            {
                File.Delete(m_AlphagramLearnFile);
            }

            UpdateStats();
        }
    }


    private void Start_Click(object sender, EventArgs e)
    {
        if (LexiconList.SelectedItem is Lexicon lexicon && m_WordLearnFile != null && m_AlphagramLearnFile != null)
        {
            if (!File.Exists(m_AlphagramLearnFile))
            {
                string utilityFolder = Path.Combine(m_WordService.BaseFolder, "utility");
                Directory.CreateDirectory(utilityFolder);
                string utilityFile = Path.Combine(utilityFolder, lexicon.Name + "-utility.txt");

                if (File.Exists(utilityFile))
                {
                    Start.Text = "Generating list, please wait...";
                    Application.DoEvents();

                    var learnOrder = new SortedList<double, string>();
                    int index = 1;
                    var random = new Random();

                    foreach (string line in File.ReadLines(utilityFile))
                    {
                        if (!line.StartsWith("/") && line.Trim().Length > 0)
                        {
                            string word = line.Trim();
                            double score = (double)1 / index;

                            // Add a small random component to the score so the order is not always the same.
                            score *= 1 + ((random.NextDouble() / 8) - (1 / 16));
                            learnOrder.Add(score, word);
                            index++;
                        }
                    }

                    using var writer = new StreamWriter(m_AlphagramLearnFile);

                    foreach (var word in learnOrder.OrderByDescending(x => x.Key))
                    {
                        writer.WriteLine(word.Value);
                    }
                }
                else
                {
                    MessageBox.Show("Missing utility file for lexicon " + lexicon.Name, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (!File.Exists(m_WordLearnFile))
            {
                File.WriteAllText(m_WordLearnFile, "");
            }

            Settings.Default.ReviewPeriod = ReviewPeriod.Value;
            Settings.Default.NetCorrect = (int)NetCorrect.Value;
            
            var wordLearning = new WordLearn(lexicon, m_WordLearnFile, m_AlphagramLearnFile, Settings.Default.ReviewPeriod);
            var ui = new WordLearnForm(wordLearning);

            Settings.Default.Save();
            ui.ShowDialog();
        }
    }
}
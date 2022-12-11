
namespace EmmaWords;

public class ScrabblePlay
{
    public bool Player { get; set; }
    public string Play { get; set; }
    public int Score { get; set; }

    public ScrabblePlay(bool player, string play, int score)
    {
        Player = player;
        Play = play;
        Score = score;
    }
}

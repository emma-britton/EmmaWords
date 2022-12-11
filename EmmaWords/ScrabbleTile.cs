
namespace EmmaWords;

public class ScrabbleTile
{
    public string Letter { get; set; }
    public string Display { get; set; }
    public string Designation { get; set; }
    public bool Uncommitted { get; set; }
    public int Player { get; set; }
    public int Points { get; set; }


    public ScrabbleTile(string letter, string display, string designation, int points)
    {
        Letter = letter;
        Display = display;
        Designation = designation;
        Uncommitted = false;
        Points = points;
    }

    public override string ToString()
    {
        return $"{Letter}/{Display}/{Designation}/{Uncommitted}";
    }
}

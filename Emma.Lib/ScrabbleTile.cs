
namespace Emma.Lib;

public class ScrabbleTile(string letter, string display, string designation, int points)
{
    public string Letter { get; set; } = letter;
    public string Display { get; set; } = display;
    public string Designation { get; set; } = designation;
    public bool Uncommitted { get; set; } = false;
    public int Player { get; set; }
    public int Points { get; set; } = points;


    public override string ToString()
    {
        return $"{Letter}/{Display}/{Designation}/{Uncommitted}";
    }
}

namespace CheckersGame.Models;

public class Move
{
    public int Row { get; set; }
    public int Column { get; set; }
    public CaptureInfo? Capture { get; set; }
}

public class CaptureInfo
{
    public int Row { get; set; }
    public int Column { get; set; }
}
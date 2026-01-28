using System.IO;
using UnityEngine;

public static class PLAYER_INFO
{
    private static string PATHNAME = "leaderboard.txt";
    public static string NAME;
    public static string SCORE;

    public static void SAVE_INFO()
    {
        StreamWriter sw = new StreamWriter(Path.Combine(Application.persistentDataPath, PATHNAME), true);
        using (sw)
        {
            sw.WriteLine($"{NAME},{SCORE}");
        }
    }
}

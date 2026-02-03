using System;
using UnityEngine;

public static class PLAYER_INFO
{
    public static string name;
    public static double score = 0;
    public static int minutes = 0;
    public static int seconds = 0;

    public static void add_time(float time)
    {
        PLAYER_INFO.minutes += (int)Math.Round(time / 60f);
        PLAYER_INFO.seconds += (int)Math.Round(time) % 60;
    }
}

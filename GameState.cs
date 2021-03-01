using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class GameState
{

    public static int baseline_egghead_goal = 10;
    public static int egghead_goal = baseline_egghead_goal;
    public static bool show_start_screen = true;

    public static void next_level()
    {
        int level = PlayerPrefs.GetInt("level");
        Debug.Log("advance to level " + (level + 1));
        PlayerPrefs.SetInt("level",level+1);
    }

    public static int get_high_score()
    {
        int level = PlayerPrefs.GetInt("level");
        Debug.Log("level is " + level + " high score is " + (baseline_egghead_goal + (level - 1) * 5));
        if (level == 1) return 0;
        else
        {
            return baseline_egghead_goal + (level - 2) * 5;
        }
    }

    public static void set_egghead_goal()
    {
        int level;
        if (PlayerPrefs.HasKey("level"))
        {
            level = PlayerPrefs.GetInt("level");
        }
        else // start first level
        {
            PlayerPrefs.SetInt("level", 1);
            level = 1;
        }
        egghead_goal = baseline_egghead_goal + (level - 1) * 5; // eg level 1 has a goal of 15, then 20, 25, 30 etc.
    }
}

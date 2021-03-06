using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class GameState
{

    public static int easy_egghead_goal = 5;
    public static int medium_egghead_goal = 9;
    public static int hard_egghead_goal = 13;
    public static int egghead_goal = easy_egghead_goal;
    public static string difficulty = "easy";
    public static bool show_start_screen = true;
    public static string[] level_list = new string[] { "Train Game", "Train Game Level 2", "Train Game Level 3", "Train Game Level 4", "Train Game Level 5", "Train Game Level 6", "Train Game Level 7", "Train Game Level 8", "Train Game Level 9", "Train Game Level 10", };

    public static string get_level_name()
    {
        int level_idx = PlayerPrefs.GetInt("level") - 1;
        string level_name = level_list[level_idx];
        return level_name;
    }

    public static void next_level()
    {
        int level = PlayerPrefs.GetInt("level");
        PlayerPrefs.SetInt("level",level+1);
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
        if (difficulty == "easy")
        {
            egghead_goal = easy_egghead_goal;
        }
        else if (difficulty == "medium")
        {
            egghead_goal = medium_egghead_goal;
        }
        else if (difficulty == "hard")
        {
            egghead_goal = hard_egghead_goal;
        }
    }
}

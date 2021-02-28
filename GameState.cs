using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameState
{

    public static int egghead_goal = 10;
    public static int level = 1;

    public static void next_level()
    {
        level += 1;
        egghead_goal += 5;
    }

    public static void reset_game()
    {
        level = 1;
        egghead_goal = 10;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class GameManager : MonoBehaviour
{
    // manage score, game state 

    const int cell_width = 1;
    const int cell_height = 1;
    public const int board_width = 14;
    public const int board_height = 8;
    Vector2Int home_base = new Vector2Int(0, 0);

    public Vector2Int get_home_base()
    {
        return home_base;
    }

    public Vector2Int get_board_dimension()
    {
        return new Vector2Int(board_width, board_height);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

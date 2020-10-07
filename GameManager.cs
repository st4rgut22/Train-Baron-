using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class GameManager : MonoBehaviour
{

    public Tilemap tilemap;
    const int cell_width = 1;
    const int cell_height = 1;
    const int board_width = 14;
    const int board_height = 8;
    GameObject[,] board;

    public GameObject train_in_cell(Vector3Int position)
    {
        if (board[position.x, position.y] != null)
        {
            GameObject game_object = board[position.x, position.y];
            string name = game_object.name;
            if (name == "train(Clone)")
            {
                return game_object;
            }
        }
        return null;
    }

    public void update_board_state(GameObject game_object, Vector3Int position, Vector3Int prev_position)
    {
        try
        {
            bool initial_vector = prev_position.Equals(new Vector3Int(-1, -1, -1));
            if (!initial_vector)
            {
                if (board[prev_position.y, prev_position.x] != game_object)
                    print("WARNING. Gameobject " + game_object.name + " not found in previous position " + prev_position);
                board[prev_position.y, prev_position.x] = null;
            }
            board[position.y, position.x] = game_object;
        } catch (IndexOutOfRangeException e)
        {
            print(e.Message + " Position: " + position);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        board = new GameObject[board_height, board_width];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

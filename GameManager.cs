using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class GameManager : MonoBehaviour
{
    public GameObject Train; // prefabs
    public GameObject Boxcar;

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

    public void create_boxcar(Vector3Int tilemap_position)
    {
        GameObject train = train_in_cell(tilemap_position);
        if (train != null)
        {
            GameObject boxcar = Instantiate(Boxcar); //change to accomodate different starting points
            boxcar.GetComponent<Boxcar>().attach_to_train(train);
            train.GetComponent<Train>().attach_boxcar(boxcar);
        }
    }

    public void create_train()
    {
        GameObject train = Instantiate(Train); //change to accomodate different starting points
        Vector3Int position = new Vector3Int(0, 0, 0);
        update_board_state(train, position, new Vector3Int(-1, -1, -1)); // vector3int is a placeholder because there is no prev 
        train.GetComponent<Train>().spawn_moving_object(position, MovingObject.Orientation.North);
    }

    public void update_board_state(GameObject game_object, Vector3Int position, Vector3Int prev_position)
    {
        try
        {
            bool initial_vector = prev_position.Equals(new Vector3Int(-1, -1, -1));
            if (!initial_vector)
            {
                if (board[prev_position.y, prev_position.x] == null)
                    print("WARNING. Gameobject " + game_object.name + " not found in previous position " + prev_position);
                else
                {
                    if (board[prev_position.y, prev_position.x] == game_object) // only remove gameobject references to itself
                        board[prev_position.y, prev_position.x] = null;
                }
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

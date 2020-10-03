using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train : MovingObject
{
    public Orientation orientation;
    public Orientation final_orientation;
    public Vector3Int tile_position;
    Vector2 target_position;
    const float z_pos = 0;
    bool reached_destination = false;

    // Start is called before the first frame update
    void Start()
    {
        tile_position = new Vector3Int(0, 0, 0);
        target_position = transform.position;
        orientation = Orientation.North;
    }

    // Update is called once per framech
    void Update()
    {
        if (Vector3.Distance(transform.position, target_position) < tolerance) // arrived at target 
        {
            orientation = final_orientation;
            print(orientation);
            if (orientation == Orientation.East)
            {
                tile_position = new Vector3Int(Mathf.RoundToInt(transform.position[0]), (int)(transform.position[1]), 0);
            }
            else if (orientation == Orientation.West)
            {
                tile_position = new Vector3Int(Mathf.RoundToInt(transform.position[0])-1, (int)(transform.position[1]), 0);
            }
            else if (orientation == Orientation.North)
            {
                tile_position = new Vector3Int((int)(transform.position[0]), Mathf.RoundToInt(transform.position[1]), 0);
            }
            else if (orientation == Orientation.South)
            {
                tile_position = new Vector3Int((int)(transform.position[0]), Mathf.RoundToInt(transform.position[1])-1, 0);
            }
            //if (orientation==Orientation.East || orientation==Orientation.West) tile_position = new Vector3Int(Mathf.RoundToInt(transform.position[0]), (int)(transform.position[1]), 0); 
            //else if (orientation==Orientation.North || orientation==Orientation.South) tile_position = new Vector3Int((int)(transform.position[0]), Mathf.RoundToInt(transform.position[1]), 0); 
            Vector2 train_dest_xy = BoardManager.get_train_destination(this);
            Vector3 train_destination = new Vector3(train_dest_xy[0], train_dest_xy[1], z_pos);
            target_position = train_destination;
            if (orientation != final_orientation) // curved track
                StartCoroutine(bezier_move(transform.position, orientation, final_orientation));
            else // straight track
            {
                StartCoroutine(straight_move(transform.position, target_position));
            }
        } else
        {
            //print("next position is " + next_position);
            transform.position = next_position;
        }

    }
}

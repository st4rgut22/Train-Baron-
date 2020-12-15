using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : BoardManager
{
    public Sprite left_door_bottom_left;
    public Sprite left_door_bottom_right;
    public Sprite left_door_top_left;
    public Sprite left_door_top_right;
    public Sprite right_door_bottom_left;
    public Sprite right_door_bottom_right;
    public Sprite right_door_top_left;
    public Sprite right_door_top_right;

    public class Door_Prop
    {
        public Sprite board_pivot_door;
        public Sprite unload_pivot_door;
        public float board_rotation;
        public float unload_rotation;
        public float rotation;
        public Door_Prop(Sprite board_pivot_door, Sprite unload_pivot_door, float board_rotation, float unload_rotation, float rotation)
        {
            this.board_pivot_door = board_pivot_door;
            this.unload_pivot_door = unload_pivot_door;
            this.board_rotation = board_rotation;
            this.unload_rotation = unload_rotation;
            this.rotation = rotation;
        }
    }

    public void display_structure(GameObject structure, bool is_display)
    {
        if (structure != null) structure.GetComponent<SpriteRenderer>().enabled = is_display;
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

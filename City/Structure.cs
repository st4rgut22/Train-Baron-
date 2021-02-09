using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : BoardManager
{
    public Sprite left_door_bottom_left;
    public Sprite right_door_top_right;
    public int start_reputation;

    public class Door_Prop
    {
        public Sprite pivot_door;
        public float door_rotation;
        public float rotation;
        public bool is_outer;
        public Door_Prop(Sprite pivot_door, float door_rotation, float rotation)
        {
            this.door_rotation = door_rotation;
            this.rotation = rotation;
            this.pivot_door = pivot_door;
        }
        public void set_door_sprite(Sprite door_sprite)
        {
            pivot_door = door_sprite;
        }
    }

    public void display_structure(GameObject structure, bool is_display)
    {
        if (structure != null) structure.GetComponent<SpriteRenderer>().enabled = is_display;
    }

    private void Awake()
    {

    }

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

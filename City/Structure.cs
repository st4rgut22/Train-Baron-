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

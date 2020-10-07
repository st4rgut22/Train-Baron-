using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class Train : MovingObject
{
    Boxcar[] boxcar_squad; // boxcars attached to this train

    public void change_motion()
    {
        in_motion = !in_motion;
    }

    // Start is called before the first frame update
    void Start()
    {
        tile_position = new Vector3Int(0, 0, 0);
        base.Start(); // train instantiated bottom left
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }
}

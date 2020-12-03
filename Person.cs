﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Person: Simple_Moving_Object
{
    public Sprite good_health_blob;
    public Sprite medium_health_blob;
    public Sprite poor_health_blob;
    public Sprite dead_blob;

    public int health;
    public int wealth;

    public GameObject boxcar_go;
    public bool is_board_boxcar=false;
    public bool is_exit_boxcar=false;
    public bool is_on_boxcar=false;
    public bool is_in_home=true;

    public void Start()
    {
        health = 100;
        wealth = 0;
        set_health_sprite();
        in_tile = true;
        final_dest_tile_pos = new Vector3Int(-1, -1, 0);
    }

    public void Update()
    {
        if (!is_in_home) // execute followo track after person has completed exit home sequence
            base.Update(); // follow track coroutine
        if (final_destination_reached)
        {
            if (is_board_boxcar)
            {
                is_board_boxcar = false;
                is_on_boxcar = true;
                final_destination_reached = false;
                if (boxcar_go == null) throw new System.Exception("boxcar should be assigned to person when person exited home");
                StartCoroutine(GameObject.Find("RouteManager").GetComponent<RouteManager>().step_on_boxcar(gameObject, boxcar_go));
            }
        }
    }

    public void set_orientation(RouteManager.Orientation orientation)
    {
        this.orientation = orientation;
        set_initial_rotation(orientation);
    }

    public void set_tile_pos(Vector2Int update_tile_pos)
    {
        tile_position = (Vector3Int)update_tile_pos;
    } 

    public void set_health_sprite()
    {
        Sprite sprite;
        if (health <= 0)
        {
            sprite = dead_blob;
        }
        else if (health <= 30)
        {
            sprite = poor_health_blob;
        }
        else if (health <= 70)
        {
            sprite = medium_health_blob;
        }
        else
        {
            sprite = good_health_blob;
        }
        gameObject.GetComponent<SpriteRenderer>().sprite = sprite; 
    }

    public bool change_wealth(int delta)
    {
        if (wealth + delta >= 0)
        {
            wealth += delta;
            return true;
        } else
        {
            return false;
        }
    }

    public void change_health(int delta)
    {
        if (health + delta > 100)
        {
            health = 100;
        }
        if (health + delta < 0)
        {
            health = 0;
        }
        set_health_sprite();

    }
}

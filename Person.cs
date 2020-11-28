using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Person: MonoBehaviour
{
    public Sprite good_health_blob;
    public Sprite medium_health_blob;
    public Sprite poor_health_blob;
    public Sprite dead_blob;

    public int health;
    public int wealth;
    public RouteManager.Orientation orientation;
    Vector2Int tile_pos;
    Vector2 next_position;

    private void Start()
    {
        health = 100;
        wealth = 0;
        set_health_sprite();
    }

    public void Update()
    {
        
    }

    public void set_tile_pos(Vector2Int update_tile_pos)
    {
        tile_pos = update_tile_pos;
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

    public IEnumerator straight_move(Vector2Int destination_tile)
    {
        float speed = 1f;
        Vector2 start_position = transform.position;
        Vector2 destination = RouteManager.track_tilemap.GetCellCenterWorld((Vector3Int)destination_tile);
        float og_distance = Vector2.Distance(start_position, destination); // distance to destination
        float distance = og_distance;
        while (distance > GameManager.tolerance)
        {
            float step = speed * Time.deltaTime;
            next_position = Vector2.MoveTowards(next_position, destination, step);
            transform.position = next_position;
            distance = Vector2.Distance(next_position, destination);
            yield return new WaitForEndOfFrame();
        }
        tile_pos = destination_tile;
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

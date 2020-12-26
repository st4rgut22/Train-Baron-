using UnityEngine;

public class Checkpoint
{
    public RouteManager.Orientation begin_orientation;
    public RouteManager.Orientation end_orientation;
    public Vector2 dest_pos;
    public Vector2Int tile_position;
    public float rotation;
    public float final_angle;
    public float cur_angle;
    public bool is_right_angle; // is this checkpoint a right angle turn/movement?
    public string animation_clip;

    public Checkpoint(Vector2 dest_pos, Vector2Int tile_position, RouteManager.Orientation begin_orientation, RouteManager.Orientation end_orientation, string action)
    {
        is_right_angle = true;
        this.begin_orientation = begin_orientation;
        this.end_orientation = end_orientation;
        this.dest_pos = dest_pos;
        this.tile_position = tile_position;
        animation_clip = PersonRouteManager.get_animation_from_orientation(end_orientation, action);
    }
}

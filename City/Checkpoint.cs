using UnityEngine;

public class Checkpoint
{
    public RouteManager.Orientation end_orientation;
    public Vector2 dest_pos;
    public Vector2Int tile_position;
    public float rotation;

    public Checkpoint(RouteManager.Orientation begin_orientation, RouteManager.Orientation end_orientation, Vector2 dest_pos, Vector2Int tile_position)
    {
        this.end_orientation = end_orientation;
        rotation = TrackManager.get_right_angle_rotation(begin_orientation, end_orientation);
        Debug.Log("CP with begin orientation " + begin_orientation + " and end orientation " + end_orientation + " has rotation " + rotation);
        this.dest_pos = dest_pos;
        this.tile_position = tile_position;
    }

}

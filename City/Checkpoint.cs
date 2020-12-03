using UnityEngine;

public class Checkpoint
{
    public RouteManager.Orientation end_orientation;
    public Vector2Int tile_position;
    public float rotation;

    public Checkpoint(RouteManager.Orientation begin_orientation, RouteManager.Orientation end_orientation, Vector2Int position)
    {
        this.end_orientation = end_orientation;
        rotation = TrackManager.get_right_angle_rotation(begin_orientation, end_orientation);
        tile_position = position;
    }

}

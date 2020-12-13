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

    public Checkpoint(Vector2 cur_position, float cur_angle, Vector2 dest_pos, Vector2Int tile_position, RouteManager.Orientation begin_orientation, RouteManager.Orientation end_orientation)
    {
        is_right_angle = true;
        this.begin_orientation = begin_orientation;
        this.end_orientation = end_orientation;
        this.dest_pos = dest_pos;
        this.tile_position = tile_position;
        this.cur_angle = cur_angle;
        float final_angle;
        if (cur_position.Equals(dest_pos))
        {
            final_angle = TrackManager.get_orientation_angle(end_orientation);
        }
        else
        {
            float delta_y = dest_pos.y - cur_position.y;
            float delta_x = dest_pos.x - cur_position.x;
            final_angle = (float)random_algos.radian_to_degree(Mathf.Atan2(delta_y, delta_x));
        }
        rotation = final_angle - cur_angle;
        this.final_angle = final_angle;
        if (final_angle == -1) throw new System.Exception("angle not set");
    }
}

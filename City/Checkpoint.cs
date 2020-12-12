using UnityEngine;

public class Checkpoint
{
    public RouteManager.Orientation begin_orientation;
    public RouteManager.Orientation end_orientation;
    public Vector2 dest_pos;
    public Vector2Int tile_position;
    public float rotation;
    public float final_angle;
    public bool is_right_angle; // is this checkpoint a right angle turn/movement?

    public Checkpoint(Vector2 cur_position, float cur_angle, Vector2 dest_pos, Vector2Int tile_position, RouteManager.Orientation begin_orientation, RouteManager.Orientation end_orientation)
    {
        is_right_angle = true;
        this.begin_orientation = begin_orientation;
        this.end_orientation = end_orientation;
        this.dest_pos = dest_pos;
        this.tile_position = tile_position;
        float final_angle = -1;
        if (cur_position.Equals(dest_pos))
        {
            switch (end_orientation)
            {
                case RouteManager.Orientation.North:
                    final_angle = 90;
                    break;
                case RouteManager.Orientation.East:
                    final_angle = 0;// random_algos.degree_to_radian(-90);
                    break;
                case RouteManager.Orientation.West:
                    final_angle = 180;// random_algos.degree_to_radian(90);
                    break;
                case RouteManager.Orientation.South:
                    final_angle = 270;// random_algos.degree_to_radian(180);
                    break;
            }
            rotation = TrackManager.get_rotation(begin_orientation, end_orientation);
        }
        else
        {
            float delta_y = dest_pos.y - cur_position.y;
            float delta_x = dest_pos.x - cur_position.x;
            final_angle = (float)random_algos.radian_to_degree(Mathf.Atan2(delta_y, delta_x));
            this.final_angle = final_angle; // FUCKYOUFUCKYOUFUCKYOUFUKCYOUFUCKMKDYUFIOSUDOAIFAUJS D;LIKFGH;OALQSIDHFL;KOIJASDH;LFIJKHAS;LOIDRHFYG;AWSOQ IDHYTRFAUDESWRGF; OSIDULKR;A DFHYLA;OKSUDI HF;OUIADS HYGADTF;OUI TRYHGKUL
            rotation = final_angle - cur_angle; //todo fuck you piec suiti
        }
        if (final_angle == -1) throw new System.Exception("angle not set");
    }

    //public Checkpoint(RouteManager.Orientation begin_orientation, RouteManager.Orientation end_orientation, Vector2 dest_pos, Vector2Int tile_position)
    //{
    //    is_right_angle = true;
    //    this.end_orientation = end_orientation;
    //    rotation = TrackManager.get_rotation(begin_orientation, end_orientation);
    //    Debug.Log("CP with begin orientation " + begin_orientation + " and end orientation " + end_orientation + " has rotation " + rotation);
    //    this.dest_pos = dest_pos;
    //    this.tile_position = tile_position;
    ////}

    //public Checkpoint(Vector2 position, float cur_angle, Vector2 dest_pos, Vector2Int tile_position) // free rotate overload constructor
    //{
    //    is_right_angle = false;
    //    this.tile_position = tile_position;
    //    this.dest_pos = dest_pos;
    //    this.end_orientation = RouteManager.Orientation.None; // explicitly set orientation in the next checkpoint, because this one has no valid orientation
    //    float delta_y = dest_pos.y - position.y;
    //    float delta_x = dest_pos.x - position.x;        
    //    float final_angle = (float)random_algos.radian_to_degree(Mathf.Atan2(delta_y, delta_x));
    //    if (final_angle < 0) final_angle = 360 + final_angle;
    //    rotation = TrackManager.get_shortest_rotation(final_angle, cur_angle);//final_angle - cur_angle;
    //    this.final_angle = final_angle;
    //    Debug.Log("Free Rotation " + rotation + " final angle " + final_angle + " start angle " + cur_angle + " position " + position + " final position " + dest_pos);
    //}

    //// todo: a third and final constructor that sets end orientation after doing a free transform
    //public Checkpoint(float cur_angle, Vector2 dest_pos, RouteManager.Orientation end_orientation, Vector2Int tile_position) // just turn not move
    //{
    //    this.tile_position = tile_position;
    //    final_angle = cur_angle;
    //    switch (end_orientation)
    //    {
    //        case RouteManager.Orientation.North:
    //            final_angle = 90;
    //            break;
    //        case RouteManager.Orientation.East:
    //            final_angle = 0;// random_algos.degree_to_radian(-90);
    //            break;
    //        case RouteManager.Orientation.West:
    //            final_angle = 180;// random_algos.degree_to_radian(90);
    //            break;
    //        case RouteManager.Orientation.South:
    //            final_angle = 270;// random_algos.degree_to_radian(180);
    //            break;
    //    }
    //    float angle_diff = TrackManager.get_shortest_rotation(final_angle, cur_angle);//final_angle - cur_angle;
    //    this.end_orientation = end_orientation;
    //    this.dest_pos = dest_pos;
    //    rotation = angle_diff;

    //}
}

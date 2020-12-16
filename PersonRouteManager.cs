using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class PersonRouteManager : RouteManager
{
    public bool is_curve_inner(Boxcar boxcar)
    {
        // the bezier curve multiplier is decided by whether the person travels along inside or outside of the track
        int is_inner = boxcar.station_track.inner;
        Orientation station_orientation = boxcar.station_track.station.orientation;
        if (is_inner == 0 && (station_orientation == Orientation.South || station_orientation == Orientation.North))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static Vector2 get_doorstep_offset(Orientation exit_home_orientation, Vector2 door_position, Vector2 dest_tile_center)
    {
        Vector2 offset = new Vector2(0, 0);
        if (exit_home_orientation == Orientation.North || exit_home_orientation == Orientation.South)
        {
            if (door_position.x < dest_tile_center.x)
                offset.x = -cell_width / 4;
            else { offset.x = cell_width / 4; }
        }
        else
        {
            if (door_position.y < dest_tile_center.y)
                offset.y = -cell_width / 4;
            else { offset.y = cell_width / 4; }
        }
        return offset;
    }

    public static Vector2 get_straight_walking_position(Vector2 middle_pos, Orientation orientation)
    {
        // todo: incorporate rotation here!
        switch (orientation)
        {
            case Orientation.North:
                middle_pos.y -= cell_width / 4;
                break;
            case Orientation.East:
                middle_pos.x -= cell_width / 4;
                break;
            case Orientation.West:
                middle_pos.x += cell_width / 4;
                break;
            case Orientation.South:
                middle_pos.y += cell_width / 4;
                break;
        }
        return middle_pos;
    }

    public static Checkpoint get_exit_home_checkpoint(Orientation orientation, Transform transform, Orientation exit_orientation, Boxcar boxcar, Room room, Vector2 door_position, Vector2Int doorstep_position)
    {
        // rotate to face door, then exit in straight path        
        Vector2 middle_pos = track_tilemap.GetCellCenterWorld((Vector3Int)doorstep_position);
        Vector2 offset = get_doorstep_offset(exit_orientation, door_position, middle_pos);
        middle_pos += offset;
        Vector2 exit_home_dest = get_straight_walking_position(middle_pos, exit_orientation);
        return new Checkpoint(transform.position, transform.eulerAngles.z + 90, exit_home_dest, doorstep_position, orientation, exit_orientation);
    }

    public static GameObject get_exit_door(Boxcar boxcar, Room room)
    {
        // get door that exits to the boxcar
        GameObject door_parent;
        int is_inner = boxcar.station_track.inner;
        if (room.outer_door != null && room.primary_door != null) // 2 doors to choose from
        { // choose the door that is on the right side of the track
            if (is_inner == 0) door_parent = room.outer_door_container;
            else { door_parent = room.primary_door_container; }
        }
        else
        {
            if (room.outer_door != null) door_parent = room.outer_door_container;
            else { door_parent = room.primary_door_container; }
        }
        return door_parent;
    }

    public IEnumerator board_train(Boxcar boxcar, Room room, Person occupant, Vector3Int destination)
    {
        GameObject door = get_exit_door(boxcar, room);
        room.unlocked_door = door;
        Door unlocked_door = room.unlocked_door.GetComponent<Door>();
        // 2 checkpoints. In first go to doorstep and rotate accordingly. In the next only rotate to face track direction
        List<Checkpoint> board_train_checkpoints = new List<Checkpoint>();
        Vector3Int train_start_location = boxcar.train.station_track.start_location; // id of track
        Orientation exit_orientation = City.station_track_boarding_map[train_start_location];
        Vector2Int doorstep_position = get_straight_next_tile_pos(exit_orientation, room.tile_position);
        string track_name = shipyard_track_tilemap.GetTile((Vector3Int)doorstep_position).name;
        occupant.final_dest_tile_pos = boxcar.tile_position;
        occupant.final_dest_pos = boxcar.transform.position;
        Orientation[] orientation_pair = City.station_track_curve_map[boxcar.station_track.start_location];
        Orientation start_orientation = orientation_pair[0];
        Orientation end_orientation = orientation_pair[1];
        occupant.is_tight_curve = true; // wide curve
        yield return StartCoroutine(unlocked_door.rotate());
        yield return StartCoroutine(occupant.bezier_move(occupant.transform, start_orientation, end_orientation));
        StartCoroutine(unlocked_door.rotate(3)); // wait 3 seconds before closing the door
        Vector3 offset_pos = occupant.transform.position + occupant.transform.up * .45f;
        Checkpoint offset_cp = new Checkpoint(occupant.transform.position, occupant.transform.eulerAngles.z + 90, offset_pos, (Vector2Int)doorstep_position, end_orientation, end_orientation);
        board_train_checkpoints.Add(offset_cp);
        if (!occupant.is_destination_reached(cell_width))
        {
            Orientation align_track_orientation = TrackManager.get_start_orientation(track_name, offset_cp.dest_pos, boxcar.transform.position, boxcar);
            Checkpoint align_track_cp = new Checkpoint(offset_cp.dest_pos, offset_cp.final_angle, offset_cp.dest_pos, offset_cp.tile_position, offset_cp.end_orientation, align_track_orientation); // dont move just rotate
            board_train_checkpoints.Add(align_track_cp); // no need to align with track if destination is same tile as exit
        }
        occupant.is_tight_curve = is_curve_inner(boxcar);
        yield return StartCoroutine(occupant.move_checkpoints(board_train_checkpoints));
        print("start follow track sequence");
        occupant.in_tile = false; // allow person to follow the track to the destination boxcar
                                  // if all goes well then boarding is all that's left
        occupant.exit_home();
        occupant.final_orientation = occupant.orientation;
    }

    public IEnumerator step_on_boxcar(GameObject person_go, GameObject boxcar_go)
    {
        Person person = person_go.GetComponent<Person>();
        Boxcar boxcar = boxcar_go.GetComponent<Boxcar>();
        Vector2 boxcar_position = boxcar_go.transform.position;
        Orientation final_orientation = TrackManager.enter_boxcar_orientation(boxcar_position, person_go.transform.position);
        print("enter boxcar cp with person orientation " + person.orientation + " final orientation " + final_orientation + " boxcar tile pos " + boxcar.tile_position);
        Checkpoint step_on_boxcar_cp = new Checkpoint(person.transform.position, person.transform.eulerAngles.z + 90, boxcar.transform.position, (Vector2Int)boxcar.tile_position, person.orientation, final_orientation);
        //(person.orientation, final_orientation, boxcar.transform.position, (Vector2Int)boxcar.tile_position);
        List<Checkpoint> board_train_checkpoints = new List<Checkpoint>() { step_on_boxcar_cp };
        yield return StartCoroutine(person.move_checkpoints(board_train_checkpoints));
        person.transform.parent = boxcar.transform; // make person a passenger of boxcar
    }

    public IEnumerator enter_home(GameObject person_go, Room room)
    {
        List<Checkpoint> enter_home_checkpoints = new List<Checkpoint>();
        Person person = person_go.GetComponent<Person>();
        Door unlocked_door = room.unlocked_door.GetComponent<Door>();
        Vector2 enter_door_location = room.unlocked_door.transform.position;
        Vector2 room_position = track_tilemap.GetCellCenterWorld((Vector3Int)room.tile_position);
        Checkpoint enter_door_cp = new Checkpoint(person.transform.position, person.transform.eulerAngles.z + 90, enter_door_location, room.tile_position, person.orientation, person.orientation); //new Checkpoint(enter_position, center_cp_angle, enter_door_location, room.tile_position);
        Checkpoint enter_home_cp = new Checkpoint(person.transform.position, enter_door_cp.final_angle, room_position, room.tile_position, person.orientation, person.orientation);//enter_door_cp.dest_pos, enter_door_cp.final_angle, room_position, room.tile_position);
        Orientation resting_orientation = City.initial_person_face_map[room.building.building_lot.id];
        Checkpoint resting_cp = new Checkpoint(enter_home_cp.dest_pos, enter_home_cp.final_angle, enter_home_cp.dest_pos, room.tile_position, enter_home_cp.end_orientation, resting_orientation); // todo change me building orientation to match rest orientation from dictionary
        enter_home_checkpoints.Add(enter_door_cp);
        enter_home_checkpoints.Add(enter_home_cp);
        enter_home_checkpoints.Add(resting_cp); // rotate person so facing same direction as building
        yield return StartCoroutine(unlocked_door.rotate());
        yield return StartCoroutine(person.move_checkpoints(enter_home_checkpoints));
        StartCoroutine(unlocked_door.rotate());
        room.occupied = true;
        room.person_go = person_go;
    }

    public IEnumerator unload_train(Boxcar boxcar, Room room, Orientation exit_orientation)
    {
        List<Checkpoint> go_home_checkpoints = new List<Checkpoint>();
        GameObject person_go = boxcar.passenger_go;
        boxcar.is_occupied = false;
        person_go.transform.parent = null;
        Person person = person_go.GetComponent<Person>();
        person.is_tight_curve = is_curve_inner(boxcar);
        room.unlocked_door = get_exit_door(boxcar, room);
        // the final dest tile is offset from the person's route, so find the offset using city map
        Orientation home_orientation = City.station_track_boarding_map[boxcar.station_track.start_location];
        person.room = room; // assign room to person
        person.final_dest_tile_pos = (Vector3Int)get_straight_next_tile_pos(home_orientation, room.tile_position);//(Vector3Int) room.tile_position;
        person.enter_home_orientation = TrackManager.flip_straight_orientation(home_orientation); // reverse direction of exiting the home
        Orientation flip_exit_orientation = TrackManager.flip_straight_orientation(exit_orientation); // need to flip because offset is in the opposite direction the person is traveling
        Vector2 exit_boxcar_dest = get_straight_walking_position(boxcar.transform.position, flip_exit_orientation);
        Checkpoint exit_boxcar_checkpoint = new Checkpoint(person_go.transform.position, person_go.transform.eulerAngles.z + 90, exit_boxcar_dest, (Vector2Int)boxcar.tile_position, person.orientation, exit_orientation);
        string track_name = shipyard_track_tilemap.GetTile(boxcar.tile_position).name;
        go_home_checkpoints.Add(exit_boxcar_checkpoint);
        Orientation exit_home_orientation = City.station_track_boarding_map[boxcar.station_track.start_location];
        Vector2Int doorstep_position = get_straight_next_tile_pos(exit_home_orientation, room.tile_position);
        Vector2 room_abs_position = track_tilemap.GetCellCenterWorld((Vector3Int)doorstep_position); //one tile offset from home in the center. Because boxcars always stop at tile edges, so center tells us which direction person should exit boxcar
        person.final_dest_pos = room.unlocked_door.GetComponent<Door>().door_sprite_go.transform.position;
        if (!person.is_destination_reached(cell_width))
        {
            Orientation align_track_orientation = TrackManager.get_start_orientation(track_name, boxcar.transform.position, room_abs_position, boxcar);
            Checkpoint align_track_cp = new Checkpoint(exit_boxcar_checkpoint.dest_pos, exit_boxcar_checkpoint.final_angle, exit_boxcar_checkpoint.dest_pos, exit_boxcar_checkpoint.tile_position, exit_boxcar_checkpoint.end_orientation, align_track_orientation);
            go_home_checkpoints.Add(align_track_cp);
        }
        yield return StartCoroutine(person.move_checkpoints(go_home_checkpoints));
        boxcar.passenger_go = null;
        person.boxcar_go = null;
        person.is_exit_boxcar = true; // set off follow track back home sequence
    }
}
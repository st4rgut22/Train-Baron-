using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    public float door_rotation;
    public float tile_rotation;
    public GameObject door_go;
    public GameObject door_sprite_go;
    public bool is_board;
    public Sprite door_sprite;
    public bool is_outer;
    public float original_angle;
    public Room room;
    public bool is_open;
    public bool is_opening;
    public bool is_closing;

    private void Awake()
    {
        is_open = false;
        is_opening = false;

        is_outer = false;
        is_board = true;
        door_sprite_go = transform.GetChild(0).gameObject;
        original_angle = door_sprite_go.transform.localEulerAngles.z;
    }

    public void set_sprite(Sprite sprite)
    {
        door_sprite = sprite;
        door_sprite_go.GetComponent<SpriteRenderer>().sprite = door_sprite;
    }

    public IEnumerator rotate(bool is_open) // when object is not active, it never finishes?
    {
        //if (delay > 0)
        //    yield return new WaitForSeconds(delay); // wait for some action to complete before rotating
        //float start_angle = original_angle; // remove offset incorporated into rotation calculations to align person in right direction
        float cur_angle = door_sprite_go.transform.localEulerAngles.z;
        float end_angle;
        if ((!is_open && !is_opening && !is_closing) || is_open)
        {
            if (is_open) // open the door (eg upon leaving house, etc)
            {
                end_angle = original_angle + door_rotation;
                is_opening = true;
            }
            else
            {
                is_closing = true;
                end_angle = original_angle;
            }
            print("end angle is " + end_angle);
            float t_param = 1;
            Debug.Log("In response from " + GetInstanceID());
            print("rotate from cur angle " + cur_angle + " to end angle " + end_angle);
            while (t_param > 0)
            {
                t_param -= Time.deltaTime / 2; // half speed give time to enter the door
                float interp = 1.0f - t_param;
                float angle = Mathf.LerpAngle(cur_angle, end_angle, interp); // interpolate from [0,1]
                door_sprite_go.transform.localEulerAngles = new Vector3(0, 0, angle);
                t_param -= Time.deltaTime * GameManager.speed / 4;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            print("IGNORE action is open is " + is_open + " is opening is " + is_opening + " is closing is " + is_closing);
        }
        if (is_open)
        {
            is_opening = false;
        }
        else
        {
            is_closing = false;
        }
    }
}

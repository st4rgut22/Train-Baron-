using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    public float door_rotation;
    public float tile_rotation;
    public GameObject door_go;
    public GameObject door_sprite_go;
    public bool is_board;
    public bool is_open;
    public Sprite door_sprite;
    public bool is_outer;
    public float original_angle;
    public int rotate_time;
    private void Awake()
    {
        is_outer = false;
        is_board = true;
        is_open = false;
        door_sprite_go = transform.GetChild(0).gameObject;
        original_angle = door_sprite_go.transform.localEulerAngles.z;
        rotate_time = 0;
    }

    public void set_sprite(Sprite sprite)
    {
        door_sprite = sprite;
        door_sprite_go.GetComponent<SpriteRenderer>().sprite = door_sprite;
    }

    public IEnumerator rotate(int delay = 0) // when object is not active, it never finishes?
    {        
        if (delay > 0)
            yield return new WaitForSeconds(delay); // wait for some action to complete before rotating
        //float start_angle = original_angle; // remove offset incorporated into rotation calculations to align person in right direction
        float cur_angle = door_sprite_go.transform.localEulerAngles.z;
        float end_angle;
        if (is_open)
            end_angle = original_angle;
        else
        {
            end_angle = original_angle + door_rotation;
        }
        print("end angle is " + end_angle);
        float t_param = 1;
        rotate_time += 1;
        int rotate_idx = rotate_time;
        print("rotate from cur angle " + cur_angle + " to end angle " + end_angle);
        while (t_param > 0)
        {
            if (rotate_time > 1 && rotate_idx == 1)
            {
                print("cancel first rotate for rotate idx " + rotate_idx + " total rotate is " + rotate_time);
                break;
            }
            t_param -= Time.deltaTime;
            float interp = 1.0f - t_param;
            float angle = Mathf.LerpAngle(cur_angle, end_angle, interp); // interpolate from [0,1]
            door_sprite_go.transform.localEulerAngles = new Vector3(0, 0, angle);
            t_param -= Time.deltaTime * GameManager.speed/4;
            yield return new WaitForEndOfFrame();
        }
        rotate_time -= 1;
        is_open = !is_open; // if was open not it is closed
    }
}

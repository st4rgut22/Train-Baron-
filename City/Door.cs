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

    private void Awake()
    {
        is_board = true;
        is_open = false;
        door_sprite_go = transform.GetChild(0).gameObject;
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
        float start_angle = door_sprite_go.transform.eulerAngles.z; // remove offset incorporated into rotation calculations to align person in right direction
        float end_angle;
        if (is_open) end_angle = start_angle - door_rotation;
        else { end_angle = start_angle + door_rotation; }        
        float t_param = 1;
        while (t_param > 0)
        {
            t_param -= Time.deltaTime;
            float interp = 1.0f - t_param;
            float angle = Mathf.LerpAngle(start_angle, end_angle, interp); // interpolate from [0,1]
            door_sprite_go.transform.eulerAngles = new Vector3(0, 0, angle);
            print("angle is " + door_sprite_go.transform.eulerAngles);
            t_param -= Time.deltaTime * GameManager.speed/4;
            yield return new WaitForEndOfFrame();
        }
        is_open = !is_open; // if was open not it is closed
    }
}

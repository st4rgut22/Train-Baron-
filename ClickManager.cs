using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ClickManager : MonoBehaviour
{
    public Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(camera.transform.position.z));
            Vector3 mouse_pos = camera.ScreenToWorldPoint(position);
            Vector2 mouse_pos_2d = new Vector2(mouse_pos.x, mouse_pos.y);

            RaycastHit2D hit = Physics2D.Raycast(mouse_pos_2d, Vector2.zero);
            if (hit.collider != null)
            {
                GameObject clicked_gameobject = hit.collider.gameObject;
                switch (clicked_gameobject.name)
                {
                    case "curve_ES(Clone)":
                        break;
                    case "curve_NE(Clone)":
                        break;
                    case "curve_WN(Clone)":
                        break;
                    case "curve_WS(Clone)":
                        break;
                    case "hor_track(Clone)":
                        break;
                    case "vert_track(Clone)":
                        break;
                    case "train(Clone)":
                        clicked_gameobject.GetComponent<Train>().change_motion();
                        break;
                    case "boxcar(Clone)":
                        break;
                    default:
                        print("You did not click a gameobject");
                        break;
                }
            }
        }
    }
}

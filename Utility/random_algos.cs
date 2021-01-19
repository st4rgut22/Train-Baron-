using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class random_algos
{

    public static bool list_contains_arr(List<int[]>list_of_arr, Vector3 arr)
    {
        foreach (int[] array in list_of_arr)
        {
            if (array[0] == arr.x && array[1] == arr.y) return true;
        }
        return false;
    }

    static string matching_string(string str, string[] str_list)
    {
        foreach (string s in str_list)
        {
            if (s == str) return str;
        }
        return "";
    }

    public static void dfs_find_child_objects(Transform transform, List<GameObject> btn_list, string[] item_to_find)
    {
        foreach (Transform child in transform)
        {
            if (child == null) return;
            string name = child.gameObject.name;
            if (name == matching_string(name, item_to_find)) //return true if string is in list
            {                
                btn_list.Add(child.gameObject);
            }
            else
            {
                dfs_find_child_objects(child.transform, btn_list, item_to_find);
            }
        }
        return;
    }

    public static float degree_to_radian(float degree)
    {
        return Mathf.PI / 180 * degree;
    }

    public static float radian_to_degree(float radian)
    {
        return 180 / Mathf.PI * radian;
    }

}

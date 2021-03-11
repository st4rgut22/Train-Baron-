using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyLot : MonoBehaviour
{
    static DontDestroyLot instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyBlockCanvas : MonoBehaviour
{
    static DontDestroyBlockCanvas instance;

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

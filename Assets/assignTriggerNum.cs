using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class assignTriggerNum : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int num = 0;
        foreach (Transform child in transform)
        {
            GameObject GO = child.gameObject;
            TriggerDoorController trigger = GO.GetComponent<TriggerDoorController>();
            trigger.idx = num;
            num += 1;
        }
        if (!global.firstDie)
        {
            global.len = num + 1;
            global.triggers = new bool[num + 1];
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

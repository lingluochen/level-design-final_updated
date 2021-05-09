using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.name == "MainTheme")
        {
            
            GameObject.Find("BGM").GetComponent<AudioSource>().mute = true;
            GameObject.Find("MainTheme").GetComponent<AudioSource>().Play();

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{

    // Start is called before the first frame update
    

    // Update is called once per frame


    void Step()
    {
        FindObjectOfType<AudioManager>().Play("step");
    }

    void Dash()
    {
        FindObjectOfType<AudioManager>().Play("dash");
    }

    void DashImpact()
    {
        FindObjectOfType<AudioManager>().Stop("dash");
        FindObjectOfType<AudioManager>().Play("dashImpact");
    }

    void Jump()
    {
        FindObjectOfType<AudioManager>().Play("jump");
    }

    void Land()
    {
        FindObjectOfType<AudioManager>().Play("land");
    }
}

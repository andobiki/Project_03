using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereController : MonoBehaviour
{
    [SerializeField] Material mat;
    [SerializeField] float testValue;

    // Start is called before the first frame update
    void Start()
    {
        
        mat = Instantiate(GetComponent<Renderer>().material);
        if (GetComponent<MeshRenderer>() != null) { GetComponent<MeshRenderer>().material = mat; }
        if (GetComponent<SkinnedMeshRenderer>() != null) { GetComponent<SkinnedMeshRenderer>().material = mat; }
        testValue = 0;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

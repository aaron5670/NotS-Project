using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceTextManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //transform.position = transform.position + new Vector3(10, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = transform.position + new Vector3(1.0f, 0, 0);
        transform.position = new Vector3(transform.position.x + 0.1f, transform.position.y, transform.position.z);
        this.transform.Rotate(0, 180f, 0);

    }
}

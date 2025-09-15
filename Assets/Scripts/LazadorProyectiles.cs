using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazadorProyectiles : MonoBehaviour
{
    public GameObject proyectil;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update () {
	
        if (Input.GetButtonDown("Fire1")) {
            GameObject go = GameObject.Instantiate (proyectil);
            go.transform.position = transform.position+transform.forward*2;
            go.GetComponent<Rigidbody> ().AddForce (transform.forward * 10,ForceMode.Impulse);
        }
    }
}

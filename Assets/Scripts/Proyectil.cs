using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Proyectil : MonoBehaviour
{
    private static readonly int BounceImpactTime = Shader.PropertyToID("_BounceImpactTime");
    private static readonly int BounceImpactPosition = Shader.PropertyToID("_BounceImpactPosition");
    private static readonly int BounceTime = Shader.PropertyToID("_BounceTime");

    public Material material;
    
    private float startImpactTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - startImpactTime < material.GetFloat(BounceTime))
        {
            material.SetFloat(BounceImpactTime, Time.time - startImpactTime);
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Terreno"))
        {
            material.SetVector(BounceImpactPosition, transform.position);
            material.SetFloat(BounceImpactTime, 0);
            startImpactTime = Time.time;
            Debug.Log("Diamante Cuadrado"); 
        }
    }
}

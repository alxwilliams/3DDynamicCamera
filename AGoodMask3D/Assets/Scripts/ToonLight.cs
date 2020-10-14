using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToonLight : MonoBehaviour
{

    [SerializeField] private Light light1;
    [SerializeField] private Light light2;
    [SerializeField] private Light light3;
    
    void Update () {
        if(light1 !=null)
        {
            Shader.SetGlobalVector("L1_dir", -light1.transform.forward);
            Shader.SetGlobalColor("L1_color", light1.color);
        }
        if(light2 !=null)
        {
            Shader.SetGlobalVector("L2_dir", -light2.transform.forward);
            Shader.SetGlobalColor("L2_color", light2.color);
        }
        if(light3 !=null)
        {
            Shader.SetGlobalVector("L3_dir", -light3.transform.forward);
            Shader.SetGlobalColor("L3_color", light3.color);
        }
    }
}

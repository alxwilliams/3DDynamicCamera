using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToonLight : MonoBehaviour
{

    [SerializeField] private Light mainLight;
    [SerializeField] private Light light2;
    [SerializeField] private Light light3;

    void Update () {
        if(mainLight !=null  && mainLight.gameObject.activeSelf)
        {
            Shader.SetGlobalVector("L1_dir", -mainLight.transform.forward);
            Shader.SetGlobalColor("L1_color", mainLight.color);
        }
        if(light2 !=null && light2.gameObject.activeSelf)
        {
            Shader.SetGlobalVector("L2_dir", -light2.transform.forward);
            Shader.SetGlobalColor("L2_color", light2.color);
        }
        else
        {
            Shader.SetGlobalVector("L2_dir", -mainLight.transform.forward);
            Shader.SetGlobalColor("L2_color", mainLight.color);
        }
        if(light3 !=null  && light3.gameObject.activeSelf)
        {
            Shader.SetGlobalVector("L3_dir", -light3.transform.forward);
            Shader.SetGlobalColor("L3_color", light3.color);
        }
        else
        {
            Shader.SetGlobalVector("L3_dir", -mainLight.transform.forward);
            Shader.SetGlobalColor("L3_color", mainLight.color);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToonLight : MonoBehaviour
{
    
    void Update () {
        Shader.SetGlobalVector("_ToonLightDirection", -this.transform.forward);
    }
}

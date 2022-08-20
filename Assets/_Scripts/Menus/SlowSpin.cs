using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowSpin : MonoBehaviour
{
    private float pitch;
    private float yaw;
    
    // Update is called once per frame
    void Update()
    {
        pitch = Mathf.Sin((Time.time*50) * 0.001f) * 10f;
        yaw = (Time.time*20) * 0.1f;
        transform.localRotation = Quaternion.Euler(-pitch, 0, yaw);
    }
}

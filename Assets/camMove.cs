using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camMove : MonoBehaviour
{
    float moveSpeed = 0.3f;
    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f && moveSpeed<1) // forward
        {
            moveSpeed = moveSpeed + .1f;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f && moveSpeed>0.1f) // backwards
        {
            moveSpeed = moveSpeed-.1f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + moveSpeed, this.transform.position.z);
        }
        if (Input.GetKey(KeyCode.A))
        {
            this.transform.position = new Vector3(this.transform.position.x- moveSpeed, this.transform.position.y, this.transform.position.z);
        }
        if (Input.GetKey(KeyCode.S))
        {
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - moveSpeed, this.transform.position.z);
        }
        if (Input.GetKey(KeyCode.D))
        {
            this.transform.position = new Vector3(this.transform.position.x+ moveSpeed, this.transform.position.y, this.transform.position.z);
        }


    }
}

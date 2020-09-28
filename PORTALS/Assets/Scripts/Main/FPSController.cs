using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : Traveller
{
    Transform cam;
    Rigidbody body;

    private void Start()
    {
        cam = Camera.main.transform;
        body = transform.GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        Look();
    }

    private void FixedUpdate()
    {
        Movement();

        if (Input.GetKey(KeyCode.Space))
            body.AddForce(Vector3.up * 20);
    }

    private float walkSpeed = 6f, runSpeed = 9f;
    private void Movement()
    {
        float sprintSpeed = (Input.GetKey(KeyCode.LeftShift)) ? runSpeed : walkSpeed;
        float crouchSpeed = (Input.GetKey(KeyCode.LeftControl)) ? (1f / 2f) : 1f;
        float speed = sprintSpeed * crouchSpeed;
        if (Input.GetKey(KeyCode.A))
            transform.position += (-transform.right * speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D))
            transform.position += (transform.right * speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.W))
            transform.position += (transform.forward * speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.S))
            transform.position += (-transform.forward * speed * Time.deltaTime);
    }
    [SerializeField]
    float yawSpeed = 280f, pitchSpeed = 280f;
    private void Look()
    {
        transform.Rotate(0, Input.GetAxis("Mouse X") * yawSpeed * Time.deltaTime, 0);

        Vector3 rotation = new Vector3((cam.localEulerAngles.x), 0, 0);
        cam.localEulerAngles = rotation;
        cam.localRotation *= Quaternion.Euler(-Input.GetAxis("Mouse Y") * pitchSpeed * Time.deltaTime, 0, 0);

    }
}

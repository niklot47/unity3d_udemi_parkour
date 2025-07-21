using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform followTarget;
    [SerializeField] float distance = 5;
    [SerializeField] float minVerticalAngle = -45;
    [SerializeField] float maxVerticalAngle = 45;
    [SerializeField] Vector2 framingOffset;
    [SerializeField] float verticalSpeed = 2;
    [SerializeField] float horizontalSpeed = 2;
    [SerializeField] bool invertX = false;
    [SerializeField] bool invertY = false;
    float rotationX;
    float rotationY;
    float invertXVal;
    float invertYVal;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        invertXVal = (invertX) ? -1 : 1;
        invertYVal = (invertY) ? -1 : 1;

        rotationY += invertYVal * Input.GetAxis("Camera X") * horizontalSpeed;
        rotationX += invertXVal * Input.GetAxis("Camera Y") * verticalSpeed;

        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        var focusPosition = followTarget.position + new Vector3(framingOffset.x, framingOffset.y);

        transform.position = focusPosition - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;
    }

    public Quaternion PlanerRotation => Quaternion.Euler(0, rotationY, 0);
}

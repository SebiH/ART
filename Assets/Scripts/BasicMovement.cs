using UnityEngine;
using System.Collections;

public class BasicMovement : MonoBehaviour {

    public float lookSensitivity = 10f;
    public float movementSpeed = 10f;

    private Vector3 moveDirection = Vector3.zero;

    void Update ()
    {
        transform.RotateAround(transform.position, transform.right, -Input.GetAxis("Mouse Y") * lookSensitivity);
        transform.RotateAround(transform.position, Vector3.up, Input.GetAxis("Mouse X") * lookSensitivity);

        CharacterController controller = GetComponent<CharacterController>();

        moveDirection = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Flying"), Input.GetAxis("Vertical"));
        moveDirection = transform.TransformDirection(moveDirection) * movementSpeed;

        controller.Move(moveDirection * Time.deltaTime);
    }
}

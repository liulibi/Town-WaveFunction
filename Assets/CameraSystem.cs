using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    
    [SerializeField] public bool useEgdeScroll;
    [SerializeField] public float moveSpeed;
    [SerializeField] public float rotateSpeed;
    [SerializeField] public int edgeScrollSize = 20;

    [SerializeField] public float zoomSpeed;
    [SerializeField] public float targetFieldOfView = 50;
    [SerializeField] public float maxFieldOfView = 50;
    [SerializeField] public float mineFieldOfView = 10;

    private void Update()
    {
        HandleCameraMovement();
       
        HandleCameraRotation();

        HandleCameraZoom();

        if (useEgdeScroll)
        {
            HandleCameraMovementEdgeScrolling();
        }

    }

    private void HandleCameraMovement()
    {

        Vector3 inputDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            inputDir.z = +1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputDir.z = -1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputDir.x = -1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputDir.x = +1f;
        }

        Vector3 moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;

        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void HandleCameraMovementEdgeScrolling()
    {
        Vector3 inputDir = Vector3.zero;
        if (Input.mousePosition.x < edgeScrollSize)
        {
            inputDir.x = -1f;
        }
        if (Input.mousePosition.y < edgeScrollSize)
        {
            inputDir.z = -1f;
        }
        if (Input.mousePosition.x > Screen.width - edgeScrollSize)
        {
            inputDir.x = 1f;
        }
        if (Input.mousePosition.y > Screen.height - edgeScrollSize)
        {
            inputDir.z = 1f;
        }

        Vector3 moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;

        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void HandleCameraRotation()
    {
        float rotateDir = 0f;
        if (Input.GetKey(KeyCode.Q))
        {
            rotateDir = 1f;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            rotateDir = -1f;
        }
        transform.eulerAngles += new Vector3(0, rotateDir * rotateSpeed * Time.deltaTime, 0);
    }

    private void HandleCameraZoom()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            targetFieldOfView += 5;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            targetFieldOfView -= 5;
        }

        targetFieldOfView = Mathf.Clamp(targetFieldOfView, mineFieldOfView, maxFieldOfView);
        mainCamera.m_Lens.FieldOfView = Mathf.Lerp(mainCamera.m_Lens.FieldOfView, targetFieldOfView, Time.deltaTime * zoomSpeed);
    }
}

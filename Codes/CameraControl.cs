using UnityEngine;
using UnityEngine.InputSystem;


public class CameraControl : MonoBehaviour
{
    private CameraControlActions cameraActions;
    private InputAction movement;
    private Transform cameraTransform;


    [SerializeField]
    private float maxSpeed = 5f;
    private float speed;

    [SerializeField]
    private float acceleration = 10f;

    [SerializeField]
    private float damping = 15f;


    [SerializeField]
    private float stepSize = 2f;

    [SerializeField]
    private float zoomDampening = 7.5f;

    [SerializeField]
    private float minHeight = 5f;

    [SerializeField]
    private float maxHeight = 50f;

    [SerializeField]
    private float zoomSpeed = 2f;


    [SerializeField]
    private float maxRotationSpeed = 1f;


    [SerializeField]
    [Range(0f, 0.1f)]
    private float edgeTolerance = 0.05f;

    private Vector3 targetPosition;
    private float zoomHeight;
    private Vector3 horizontalVelocity;
    private Vector3 lastPosition;
    private Vector3 startDrag;

    private void Awake()
    {
        cameraActions = new CameraControlActions();
        cameraTransform = this.GetComponentInChildren<Camera>().transform;
    }

    private void OnEnable()
    {
        zoomHeight = cameraTransform.localPosition.y;
        cameraTransform.LookAt(this.transform);
        lastPosition = this.transform.position;
        movement = cameraActions.Camera.Movement;
        cameraActions.Camera.RotateCamera.performed += RotateCamera;
        cameraActions.Camera.ZoomCamera.performed += ZoomCamera;
        cameraActions.Camera.Enable();
    }

    private void OnDisable()
    {
        cameraActions.Camera.RotateCamera.performed -= RotateCamera;
        cameraActions.Camera.ZoomCamera.performed -= ZoomCamera;
        cameraActions.Camera.Disable();
    }

    private void Update()
    {
        GetKeyboardMovement();
        CheckMouseAtScreenEdge();
        DragCamera();

        UpdateBasePosition();
        UpdateCameraPosition();
    }

    private void DragCamera()
    {
        if (!Mouse.current.rightButton.isPressed)
            return;

        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (plane.Raycast(ray, out float distance))
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
                startDrag = ray.GetPoint(distance);
            else
                targetPosition += startDrag - ray.GetPoint(distance);
        }
    }

    private void UpdateBasePosition()
    {
        if (targetPosition.sqrMagnitude > 0.1f)
        {
            speed = Mathf.Lerp(speed, maxSpeed, Time.deltaTime * acceleration);
            transform.position += targetPosition * speed * Time.deltaTime;
        }
        else
        {
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.deltaTime * damping);
            transform.position += horizontalVelocity * Time.deltaTime;
        }
        targetPosition = Vector3.zero; // Reset for the next frame
    }

    private void ZoomCamera(InputAction.CallbackContext obj)
    {
        float inputValue = -obj.ReadValue<Vector2>().y / 100f;

        if (Mathf.Abs(inputValue) > 0.1f)
        {
            zoomHeight = cameraTransform.localPosition.y + inputValue * stepSize;
            zoomHeight = Mathf.Clamp(zoomHeight, minHeight, maxHeight);
        }
    }

    private void UpdateCameraPosition()
    {
        Vector3 zoomTarget = new Vector3(cameraTransform.localPosition.x, zoomHeight, cameraTransform.localPosition.z);
        zoomTarget -= zoomSpeed * (zoomHeight - cameraTransform.localPosition.y) * Vector3.forward;

        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, zoomTarget, Time.deltaTime * zoomDampening);
        cameraTransform.LookAt(this.transform);
    }

    private void RotateCamera(InputAction.CallbackContext obj)
    {
        if (!Mouse.current.middleButton.isPressed)
            return;

        float inputValue = obj.ReadValue<Vector2>().x;
        transform.rotation = Quaternion.Euler(0f, inputValue * maxRotationSpeed + transform.rotation.eulerAngles.y, 0f);
    }

    private Vector3 GetCameraForward()
    {
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        return forward;
    }

    private Vector3 GetCameraRight()
    {
        Vector3 right = cameraTransform.right;
        right.y = 0f;
        return right;
    }

    private void CheckMouseAtScreenEdge()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 moveDirection = Vector3.zero;

        if (mousePosition.x < edgeTolerance * Screen.width)
            moveDirection += -GetCameraRight();
        else if (mousePosition.x > (1f - edgeTolerance) * Screen.width)
            moveDirection += GetCameraRight();

        if (mousePosition.y < edgeTolerance * Screen.height)
            moveDirection += -GetCameraForward();
        else if (mousePosition.y > (1f - edgeTolerance) * Screen.height)
            moveDirection += GetCameraForward();

        targetPosition += moveDirection;
    }

    private void GetKeyboardMovement()
    {
        Vector3 inputvalue = movement.ReadValue<Vector2>().x *
            GetCameraRight() + movement.ReadValue<Vector2>().y * GetCameraForward();
        inputvalue = inputvalue.normalized;
        if (inputvalue.sqrMagnitude > 0.1f)
            targetPosition += inputvalue;
    }


}

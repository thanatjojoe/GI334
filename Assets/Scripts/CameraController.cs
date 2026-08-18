using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private Camera cam;

    private InputAction moveAction;
    private Vector2 moveValue;

    [SerializeField]
    private float moveSpeed;

    private InputAction zoomAction;
    private Vector2 zoomValue;

    [SerializeField]
    private float zoomSpeed;
    [SerializeField]
    private float zoomModifier;

    public static CameraController instance;

    void Awake()
    {
        instance = this;
        cam = Camera.main;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        zoomAction = InputSystem.actions.FindAction("Zoom");
    }

    // Update is called once per frame
    void Update()
    {
        MoveByKB();
        ZoomInOut();
    }

    public void MoveByKB()
    {
        moveValue = moveAction.ReadValue<Vector2>();
        Vector3 dir = new Vector3(moveValue.x, moveValue.y, 0f);

        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    public void ZoomInOut()
    {
        zoomValue = zoomAction.ReadValue<Vector2>();
        zoomModifier = zoomValue.y * 0.05f;

        if (Keyboard.current.zKey.isPressed)
            zoomModifier = -0.005f;
        if (Keyboard.current.xKey.isPressed)
            zoomModifier = 0.005f;

        cam.orthographicSize += zoomModifier * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 4, 10);
    }

    public void MoveCamera(Vector3 pos)
    {
        transform.position = new Vector3(pos.x, pos.y, -10f);
    }
}

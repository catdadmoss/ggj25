using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private int cameraSpeed = 2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var originalZ = transform.position.z;
        transform.position=Vector3.Lerp(transform.position,new Vector3(targetObject.transform.position.x, targetObject.transform.position.y, originalZ),Time.fixedDeltaTime*cameraSpeed);
    }
}

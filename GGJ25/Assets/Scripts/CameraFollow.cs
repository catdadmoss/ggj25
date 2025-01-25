using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var originalZ = transform.position.z;
        transform.position=Vector3.Lerp(transform.position,new Vector3(targetObject.transform.position.x, targetObject.transform.position.y, originalZ),Time.deltaTime);
    }
}

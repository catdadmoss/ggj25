using Unity.VisualScripting;
using Unity.VisualScripting;
using UnityEditor.Media;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

public class CollectorController : MonoBehaviour
{
    private float size = 1f;
    public float Size { get { return size; } }
    private Bounds bounds;
    [SerializeField] private GameObject aura;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        size = transform.localScale.magnitude;
        bounds = GetComponent<Renderer>().bounds;
        foreach(Transform child in transform)
        {
            var childRenderer = child.GetComponent<Renderer>();
            if(childRenderer != null)
            {
                bounds.Encapsulate(childRenderer.bounds);
            }
        }
    }

    private void Update()
    {
        var largerAxisSize = bounds.max.magnitude;
        aura.transform.localScale = Vector3.Lerp(aura.transform.localScale,new Vector3(largerAxisSize, largerAxisSize, 1f),Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Collectible"))
        {

            if (collision.transform.localScale.magnitude <= size)
            {
                //collision.collider.gameObject.GetComponent<SpringJoint2D>().connectedBody = collision.otherCollider.attachedRigidbody;
                collision.transform.parent = transform;
                size += collision.transform.localScale.magnitude;
                GameController.Instance.UpdateScore(size);

                EnemyController controller = collision.gameObject.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.OnEnemyCollected();
                }

                bounds.Encapsulate(collision.gameObject.GetComponentInChildren<Renderer>().bounds);
               
            }
        }
    }
    // Draws a wireframe box around the selected object,
    // indicating world space bounding volume.
    public void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(bounds.center, bounds.extents * 2);
    }

}

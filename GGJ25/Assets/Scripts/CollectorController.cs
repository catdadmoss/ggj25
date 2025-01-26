using Unity.VisualScripting;
using Unity.VisualScripting;
using UnityEditor.Media;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

public class CollectorController : MonoBehaviour
{
    private float size = 0.5f;
    public float Size { get { return size; } }
    private Bounds bounds;
    [SerializeField] private GameObject aura;
    [SerializeField] private Rigidbody2D stickyCenter;
    [SerializeField] private float innerGravityRange = 1;
    [SerializeField] private AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        size = 0.5f;
        bounds = GetComponent<Renderer>().bounds;
        foreach (Transform child in transform)
        {
            var childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null)
            {
                bounds.Encapsulate(childRenderer.bounds);
            }
        }
    }

    private void Update()
    {
        var largerAxisSize = bounds.max.magnitude;
        aura.transform.localScale = Vector3.Lerp(aura.transform.localScale, new Vector3(largerAxisSize, largerAxisSize, 1f), Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Collectible"))
        {
            var parentObject = collision.gameObject.transform.parent.gameObject;
            EnemyController enemyController = parentObject.GetComponent<EnemyController>();

            if (enemyController != null && enemyController.GetGravityModifier() <= size)
            {
                var newGravityObject = parentObject.AddComponent<GravityObject>();
                newGravityObject.GravityType = GravityObject.GravityObjectType.PullsAndGetsPulled;
                newGravityObject.GravityRange = innerGravityRange;
                newGravityObject.Mass = enemyController.GetGravityModifier();
         
                parentObject.transform.parent = stickyCenter.transform;
                size += enemyController.GetGravityModifier();
                GameController.Instance.UpdateScore(size);


                enemyController.OnEnemyCollected();


                bounds.Encapsulate(parentObject.GetComponent<Renderer>().bounds);
                audioSource.Play();

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

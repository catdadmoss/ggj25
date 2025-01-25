using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

public class CollectorController : MonoBehaviour
{
    private float size = 1f;
    public float Size { get { return size; } }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        size = transform.localScale.magnitude;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Collectible"))
        {
            if (collision.transform.localScale.magnitude <= size)
            {
                collision.transform.parent = transform;
                size += collision.transform.localScale.magnitude;
                GameController.Instance.UpdateScore(size);

                EnemyController controller = collision.gameObject.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.OnEnemyCollected();
                }

            }
        }
    }
}

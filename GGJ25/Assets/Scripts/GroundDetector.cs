using UnityEngine;

namespace DefaultNamespace
{
    public class GroundDetector : MonoBehaviour
    {

        public bool isFloored = false;

    
        public void Start()
        {
            isFloored = false;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                isFloored = true;
            }
        }
        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                isFloored = false;
            }        
        }
    }
}
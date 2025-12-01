using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] Animator animator;
    void Update()
    {
        foreach (var i in PlayerMovmant.players)
        {
            if (Vector3.Distance(transform.position, i.transform.position) < 1.5f)
            {
                animator.SetBool("isOpen", true);
                return;
            }
        }
        animator.SetBool("isOpen", false);
    }
}

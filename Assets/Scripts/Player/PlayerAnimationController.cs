
using UnityEngine;
public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] Animator animator;
    private Vector3 position;
    
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] audioClip;
    
    private Vector3 deltaPosition
    {
        get { return transform.position - position; }
    }
    void Update()
    {
        transform.LookAt(transform.position + deltaPosition);
        animator.SetFloat("speed", deltaPosition.magnitude/Time.deltaTime);
        
        position = transform.position;
    }

    public void PlayStepOneShot()
    {
        audioSource.PlayOneShot(audioClip[Random.Range(0, audioClip.Length)]);
    }
}

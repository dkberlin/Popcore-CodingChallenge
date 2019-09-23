using UnityEngine;

public class ParticleSystemAutoDestroyer : MonoBehaviour
{
    private ParticleSystem ps;
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (!ps) return;
        if (!ps.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}

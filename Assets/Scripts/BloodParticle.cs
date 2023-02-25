using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodParticle : MonoBehaviour
{
    public Transform target;
    public ParticleSystem bloodTrail;

    public float speed;
    public float absorbRadius;

    private void FixedUpdate()
    {
        if (target != null)
        {
            transform.position += (target.position - transform.position).normalized * (speed * Time.fixedDeltaTime);
            if (Vector2.Distance(transform.position, target.position) < absorbRadius)
            {
                bloodTrail.transform.parent = null;
                bloodTrail.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                Destroy(gameObject);
            }
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAutoDestroy : MonoBehaviour
{
    private ParticleSystem ps;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        
        if (ps != null)
        {
            var main = ps.main;
            main.loop = false;
            main.stopAction = ParticleSystemStopAction.Disable;
        }
    }

    void OnEnable()
    {
        if (ps != null)
        {
            var main = ps.main;
            main.loop = false;
            main.stopAction = ParticleSystemStopAction.Disable;
            ps.Play();
        }
    }

    void LateUpdate()
    {
        if (ps != null && !ps.IsAlive(true))
        {
            gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}

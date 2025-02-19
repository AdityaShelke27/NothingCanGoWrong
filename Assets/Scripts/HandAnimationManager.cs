using System.Collections;
using UnityEngine;

public class HandAnimationManager : MonoBehaviour
{
    [SerializeField] Animator m_HandAnimator;
    [SerializeField] string[] m_AnimatonStates;
    bool m_IsWorking = false;
    [SerializeField] float anim = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(PlayAnimations());
    }

    // Update is called once per frame
    void Update()
    {
        anim = m_HandAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    IEnumerator PlayAnimations()
    {
        while (true) 
        {
            Debug.Log("Reached");
            yield return new WaitUntil(() => m_IsWorking);
            m_IsWorking = false;
            m_HandAnimator.Play(m_AnimatonStates[Random.Range(0, m_AnimatonStates.Length)]);

            yield return new WaitUntil(() => m_HandAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.5f);
        }
    }

    public void SetWorking()
    {
        m_IsWorking = true;
    }
}

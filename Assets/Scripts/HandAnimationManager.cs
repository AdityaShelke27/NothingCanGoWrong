using System.Collections;
using UnityEngine;

public class HandAnimationManager : MonoBehaviour
{
    [SerializeField] Animator m_HandAnimator;
    [SerializeField] string[] m_AnimatonStates;
    bool m_IsWorking = false;
    bool m_IsAnimationComplete = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(PlayAnimations());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator PlayAnimations()
    {
        while (true) 
        {
            yield return new WaitUntil(() => m_IsWorking);
            
            m_HandAnimator.Play(m_AnimatonStates[Random.Range(0, m_AnimatonStates.Length)]);
            float time = m_HandAnimator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitUntil(() => m_IsAnimationComplete || m_HandAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= time);
            m_IsAnimationComplete = false;
            m_IsWorking = false;
        }
    }

    public void SetWorking()
    {
        m_IsWorking = true;
    }
    public void SetAnimationComplete()
    {
        m_IsAnimationComplete = true;
    }
}

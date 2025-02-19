using UnityEngine;

public class AnimationComplete : MonoBehaviour
{
    [SerializeField] HandAnimationManager m_HandAnimationManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CallAnimationComplete()
    {
        m_HandAnimationManager.SetAnimationComplete();
    }
}

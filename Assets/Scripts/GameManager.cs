using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    float m_GapDistance;
    int m_Score;
    float m_InputSpeed;
    float m_Time;
    int m_InputNum;
    bool m_WasFalse;
    InputActionData m_CurrentAction;
    Queue<InputActionData> m_InputQueue = new();
    HandAnimationManager m_HandAnimationManager;

    [SerializeField] float m_SpawnRate;
    [SerializeField] float m_InitialInputSpeed;
    [SerializeField] int m_DifficultyIncreaseTime;
    [SerializeField] float m_SpeedIncreaseProportion;
    [SerializeField] float m_EndOffset = -1;
    [SerializeField] int m_MaxPoints = 10;
    [SerializeField] float m_SwapMinThreshold;
    [SerializeField] float m_SwapTime;
    [SerializeField] float m_SwapStartTime;
    [SerializeField] float m_SwapThresholdIncreaseRate;
    
    [Header("References")]
    [SerializeField] AudioSource m_AudioSource;
    [SerializeField] AudioClip[] m_SoundEffects;
    [SerializeField] Material m_GlitchMaterial;
    [SerializeField] TMP_Text m_ScoreText;
    [SerializeField] Sprite[] m_InputImg;
    [SerializeField] Transform m_SpawnPoint;
    [SerializeField] Transform m_StartPoint;
    [SerializeField] Transform m_EndPoint;
    [SerializeField] Transform m_SwitchPoint;
    [SerializeField] GameObject m_InputAction;
    [SerializeField] GameObject m_CurrentActionHolder;

    Dictionary<int, KeyCode> m_KeyMaping = new() { 
        { 0, KeyCode.UpArrow }, 
        { 1, KeyCode.DownArrow }, 
        { 2, KeyCode.RightArrow }, 
        { 3, KeyCode.LeftArrow },
        { 4, KeyCode.Mouse0 },
        { 5, KeyCode.Mouse1 }
    };
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_CurrentAction = new(null, -1, 0);
        m_InputNum = m_KeyMaping.Count;
        m_InputSpeed = m_InitialInputSpeed;
        m_HandAnimationManager = GetComponent<HandAnimationManager>();

        m_GapDistance = Mathf.Abs(m_StartPoint.transform.position.x - m_EndPoint.transform.position.x);

        UpdateScore();
        StartCoroutine(Spawner());
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();

        if(Input.GetKeyDown(KeyCode.S))
        {
            SwapInputs();
        }
    }

    IEnumerator MoveInput(InputActionData data)
    {
        data.inputImg.sprite = m_InputImg[data.key];

        bool HasSwitched = false;
        bool isAction = false;
        bool isFalse = false;
        while (data.inputImg.color.a > 0)
        {
            if(!data.isSwapping)
            {
                data.speed = m_InputSpeed;
                data.inputAction.transform.position += data.speed * Time.deltaTime * Vector3.left;
            }
            
            if(!HasSwitched && data.inputAction.transform.position.x < m_SwitchPoint.transform.position.x)
            {
                if(Random.Range(0, 3) == 0)
                {
                    SwitchInput(data);
                    data.inputImg.material = Instantiate(m_GlitchMaterial);
                    data.inputImg.material.SetTexture("_Texture", data.inputImg.mainTexture);
                }
                
                HasSwitched = true;
            }
            if(!isAction && data.inputAction.transform.position.x < m_StartPoint.transform.position.x)
            {
                isAction = true;
                m_WasFalse = false;
                m_CurrentAction = data;
                m_InputQueue.Dequeue();
            }
            else if(data.inputAction.transform.position.x < m_EndPoint.transform.position.x + m_EndOffset)
            {
                if(!isFalse && !data.wasTapped)
                {
                    Debug.Log("Incorrect");
                    data.inputImg.color = Color.red;
                    m_WasFalse = true;
                    isFalse = true;
                }
                Color dataColor = data.inputImg.color;
                dataColor.a -= 1 * Time.deltaTime;
                data.inputImg.color = dataColor;
            }
            yield return null;
        }

        Destroy(data.inputAction);
    }

    IEnumerator Spawner()
    {
        while (true)
        {
            GameObject input = Instantiate(m_InputAction, m_SpawnPoint);
            input.transform.localPosition = Vector3.zero;
            InputActionData data = new(input, Random.Range(0, m_InputNum), m_InputSpeed);
            m_InputQueue.Enqueue(data);
            StartCoroutine(MoveInput(data));

            float timeInterval = m_SpawnRate / m_InputSpeed;
            yield return new WaitForSeconds(timeInterval);
            m_Time += timeInterval;

            IncreaseDifficulty();
        }
    }

    void CheckInput()
    {
        if (Input.anyKeyDown && !m_WasFalse)
        {
            if (m_CurrentAction.key != -1)
            {
                if (Input.GetKeyDown(m_KeyMaping[m_CurrentAction.key]))
                {
                    Debug.Log("Correct");

                    m_CurrentAction.key = -1;
                    m_CurrentAction.inputImg.color = Color.green;
                    m_CurrentAction.wasTapped = true;

                    float start = Mathf.Abs(m_CurrentAction.inputAction.transform.position.x - ((m_StartPoint.transform.position.x + m_EndPoint.transform.position.x) / 2));
                    m_Score += (int) (m_MaxPoints * (1 - (start / m_GapDistance)));
                    m_HandAnimationManager.SetWorking();

                    m_AudioSource.clip = m_SoundEffects[Random.Range(0, m_SoundEffects.Length)];
                    m_AudioSource.Play();

                    UpdateScore();
                }
                else
                {
                    Debug.Log("Incorrect");
                    m_CurrentAction.inputImg.color = Color.red;
                    m_WasFalse = true;
                }
            }
            else
            {
                Debug.Log("Incorrect");
                m_WasFalse = true;
            }
        }
    }

    void IncreaseDifficulty()
    {
        if(m_InputSpeed > m_InitialInputSpeed * 2)
        {
            Vector3 pos = m_CurrentActionHolder.transform.position;
            pos.x -= 2;
            m_CurrentActionHolder.transform.position = pos;
        }
        float m_TargetSpeed = m_InitialInputSpeed * Mathf.Pow(1 + m_SpeedIncreaseProportion, (int)m_Time / m_DifficultyIncreaseTime);

        if(m_TargetSpeed > m_InputSpeed)
        {
            StartCoroutine(LerpToTargetSpeed(m_TargetSpeed));
            m_SwapMinThreshold += m_SwapThresholdIncreaseRate;
        }

        if(m_Time >= m_SwapStartTime)
        {
            if(Random.Range(0, 5) == 0)
            {
                SwapInputs();
            }
        }

        if(Random.Range(0, 10) == 0)
        {
            m_SpawnRate++;
        }
    }

    IEnumerator LerpToTargetSpeed(float speed)
    {
        while(speed - m_InputSpeed > 0.2f)
        {
            m_InputSpeed = Mathf.Lerp(m_InputSpeed, speed, 0.7f);

            yield return null;
        }

        m_InputSpeed = speed;
    }

    void SwitchInput(InputActionData data)
    {
        data.key = Random.Range(0, m_InputNum);
        data.inputImg.sprite = m_InputImg[data.key];
    }
    void SwapInputs()
    {
        InputActionData[] arr = m_InputQueue.ToArray();

        InputActionData first = null;
        InputActionData second = null;

        for (int i = 0; i < arr.Length; i++)
        {
            if(arr[i].inputAction.transform.position.x - m_StartPoint.position.x >= m_SwapMinThreshold)
            {
                first = arr[i];

                if(i < arr.Length - 1)
                {
                    second = arr[Random.Range(i + 1, arr.Length)];
                }
                
                break;
            }
        }
        
        if (first != null && second != null)
        {
            StartCoroutine(Swap(first, second));
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(m_StartPoint.position, 10);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(m_StartPoint.position + new Vector3(m_SwapMinThreshold, 0, 0), 10);
    }
    IEnumerator Swap(InputActionData obj1, InputActionData obj2)
    {
        obj1.inputImg.color = Color.yellow;
        obj2.inputImg.color = Color.yellow;
        obj1.isSwapping = true;
        obj2.isSwapping = true;
        float time = m_SwapTime;
        float speed = obj1.speed;
        float distance = Mathf.Abs(obj1.inputAction.transform.position.x - obj2.inputAction.transform.position.x);

        if(obj1.inputAction.transform.position.x < obj2.inputAction.transform.position.x)
        {
            obj1.speed = speed - (distance / m_SwapTime);
            obj2.speed = speed + (distance / m_SwapTime);
        }
        else
        {
            obj1.speed = speed + (distance / m_SwapTime);
            obj2.speed = speed - (distance / m_SwapTime);
        }
        
        while (time > 0)
        {
            obj1.inputAction.transform.position += obj1.speed * Time.deltaTime * Vector3.left;
            obj2.inputAction.transform.position += obj2.speed * Time.deltaTime * Vector3.left;
            time -= Time.deltaTime;

            yield return null;
        }
        obj1.isSwapping = false;
        obj2.isSwapping = false;

        obj1.inputImg.color = Color.white;
        obj2.inputImg.color = Color.white;
    }

    void UpdateScore()
    {
        m_ScoreText.text = m_Score.ToString();
    }
}

public class InputActionData
{
    public GameObject inputAction = null;
    public int key;
    public float speed;
    public bool wasTapped = false;
    public bool isSwapping = false;
    public Image inputImg = null;

    public InputActionData(GameObject _inputAction, int _key, float _speed)
    {
        inputAction = _inputAction;
        key = _key;
        speed = _speed;
        if(_inputAction)
        {
            inputImg = _inputAction.GetComponent<Image>();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    [SerializeField] Transform m_SpawnPoint;
    [SerializeField] Transform m_StartPoint;
    [SerializeField] Transform m_EndPoint;
    [SerializeField] Transform m_SwitchPoint;
    [SerializeField] GameObject m_InputAction;
    [SerializeField] float m_InitialSpawnInterval;
    [SerializeField] float m_InitialInputSpeed;
    [SerializeField] float m_TimeToDie;
    [SerializeField] TMP_Text m_ScoreText;
    [SerializeField] Sprite[] m_InputImg;
    [SerializeField] int m_DifficultyIncreaseTime;
    [SerializeField] float m_SpeedIncreaseProportion;
    [SerializeField] float m_EndOffset = -1;
    [SerializeField] int m_MaxPoints = 10;
    float m_GapDistance;
    int m_CurrentAction = -1;
    int m_Score;
    float m_InputSpeed;
    float m_Time;
    int m_InputNum;
    bool m_WasFalse;
    Image m_CurrentActionImage;
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
        m_InputNum = m_KeyMaping.Count;
        m_InputSpeed = m_InitialInputSpeed;

        m_GapDistance = Mathf.Abs(m_StartPoint.transform.position.x - m_EndPoint.transform.position.x);

        UpdateScore();
        StartCoroutine(Spawner());
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
    }

    IEnumerator MoveInput(GameObject inputAction, int key)
    {
        inputAction.GetComponent<Image>().sprite = m_InputImg[key];

        bool HasSwitched = false;
        bool isAction = false;
        bool isFalse = false;
        float time = m_TimeToDie;
        while (time > 0)
        {
            inputAction.transform.position += m_InputSpeed * Time.deltaTime * Vector3.left;
            time -= Time.deltaTime;
            if(!HasSwitched && inputAction.transform.position.x < m_SwitchPoint.transform.position.x)
            {
                if(Random.Range(0, 3) == 0)
                {
                    SwitchInput(inputAction, ref key);
                }
                
                HasSwitched = true;
            }
            if(!isAction && inputAction.transform.position.x < m_StartPoint.transform.position.x)
            {
                isAction = true;
                m_WasFalse = false;
                m_CurrentAction = key;
                m_CurrentActionImage = inputAction.GetComponent<Image>();
            }
            else if(!isFalse && inputAction.transform.position.x < m_EndPoint.transform.position.x + m_EndOffset)
            {
                Image image = inputAction.GetComponent<Image>();
                if(image.color == Color.white)
                {
                    Debug.Log("Incorrect");
                    image.color = Color.red;
                    m_WasFalse = true;
                    isFalse = true;
                }
            }
            yield return null;
        }

        Destroy(inputAction);
    }

    IEnumerator Spawner()
    {
        while (true)
        {
            GameObject input = Instantiate(m_InputAction, m_SpawnPoint);
            input.transform.localPosition = Vector3.zero;
            StartCoroutine(MoveInput(input, Random.Range(0, m_InputNum)));

            yield return new WaitForSeconds(m_InitialSpawnInterval);
            m_Time += m_InitialSpawnInterval;

            IncreaseDifficulty();
        }
    }

    void CheckInput()
    {
        if (Input.anyKeyDown && !m_WasFalse)
        {
            if (m_CurrentAction != -1)
            {
                if (Input.GetKeyDown(m_KeyMaping[m_CurrentAction]))
                {
                    Debug.Log("Correct");
                    m_CurrentAction = -1;
                    m_CurrentActionImage.color = Color.green;
                    float start = Mathf.Abs(m_CurrentActionImage.transform.position.x - ((m_StartPoint.transform.position.x + m_EndPoint.transform.position.x) / 2));
                    m_Score += (int) (m_MaxPoints * (1 - (start / m_GapDistance)));

                    UpdateScore();
                }
                else
                {
                    Debug.Log("Incorrect");
                    m_CurrentActionImage.color = Color.red;
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
        float m_TargetSpeed = m_InitialInputSpeed * Mathf.Pow(1 + m_SpeedIncreaseProportion, (int)m_Time / m_DifficultyIncreaseTime);

        if(m_TargetSpeed > m_InputSpeed)
        {
            StartCoroutine(LerpToTargetSpeed(m_TargetSpeed));
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

    void SwitchInput(GameObject inputAction, ref int key)
    {
        key = Random.Range(0, m_InputNum);
        inputAction.GetComponent<Image>().sprite = m_InputImg[key];
    }

    void UpdateScore()
    {
        m_ScoreText.text = m_Score.ToString();
    }
}

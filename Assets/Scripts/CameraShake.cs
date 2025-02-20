using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Shake
{
    public float DurationInit;
    public float Duration;
    public float Strength;
    public Vector3 Direction;
    public Shake(float duration, float strength, Vector3 direction)
    {
        this.DurationInit = duration;
        this.Duration = duration;
        this.Strength = strength;
        this.Direction = direction.normalized;
    }
}
public class CameraShake : MonoBehaviour
{
    GameObject _camera;
    Vector3 _posInit;
    //Vector3 _rotInit;
    List<Shake> _shakes = new List<Shake>();
    public float DurationByStrengthMultiplier;//.05f
    public float MultStrength;//.05f
    public float SinePeriod;//60f
    void Start()
    {
        _camera = Camera.main.gameObject;
        _posInit = _camera.transform.position;
        //_rotInit = _camera.transform.rotation.eulerAngles;
    }

    public void ShakeAdd(float strength)
    {
        _shakes.Add(new Shake(DurationByStrengthMultiplier * Mathf.Log10(strength), Mathf.Log(strength, 3), _camera.transform.forward));
    }
    void Update()
    {
        int count = _shakes.Count;
        if (count > 0)
        {
            Vector3 offset = Vector3.zero;
            for (int i = count - 1; i >= 0; i--)
            {
                _shakes[i].Duration -= Time.deltaTime;
                if (_shakes[i].Duration > 0)
                {
                    offset += (Mathf.Sin(SinePeriod * _shakes[i].Duration) * MultStrength * (_shakes[i].Duration / _shakes[i].DurationInit) * _shakes[i].Strength * _shakes[i].Direction);
                }
                else
                {
                    _shakes.RemoveAt(i);
                    if (_shakes.Count == 0)
                    {
                        offset = Vector3.zero;
                        _camera.transform.position = _posInit;
                    }
                }
            }
            if (offset != Vector3.zero)
            {
                _camera.transform.position = _posInit + offset;
            }
        }
    }
}
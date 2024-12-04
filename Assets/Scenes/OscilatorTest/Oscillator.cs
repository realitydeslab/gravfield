using UnityEngine;

// Makes a transform oscillate relative to its start position
public class Oscillator : MonoBehaviour
{
    public float m_Amplitude = 1.0f;
    public float m_Period = 1.0f;
    public Vector3 m_Direction = Vector3.up;
    Vector3 m_StartPosition;

    float timer = 0;
    void Start()
    {
        m_StartPosition = transform.position;
    }

    //void Update()
    //{
    //    var pos = m_StartPosition + m_Direction * m_Amplitude * Mathf.Sin(2.0f * Mathf.PI * Time.time / m_Period);
    //    transform.position = pos;
    //}

    void FixedUpdate()
    {
        timer += Time.deltaTime;
        transform.Rotate(0, 0, oscillate(timer, 2, 1));
        transform.Translate(Vector2.right * oscillate(timer, 5, 1));
    }

    float oscillate(float time, float speed, float scale)
    {
        return Mathf.Cos(time * speed / Mathf.PI) * scale;
    }
}
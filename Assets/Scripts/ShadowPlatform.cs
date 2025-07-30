using UnityEngine;

public class ShadowPlatform : MonoBehaviour
{
    void Awake()
    {
        // Сделать тень немного прозрачной
        var sr = GetComponent<SpriteRenderer>();
        if (sr)
        {
            var color = sr.color;
            color.a = 0.5f;
            sr.color = color;
        }
    }

    void Start()
    {
        // Отключить любые движения
        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
    }
}

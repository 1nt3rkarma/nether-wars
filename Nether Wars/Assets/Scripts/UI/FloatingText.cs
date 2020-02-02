using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    float speed = 0;

    public static void Create(Vector3 atPoint, string label, Color color, float size, float expirationTime, float speed)
    {
        GameObject textObj = new GameObject("Текст: " + label);
        textObj.transform.position = atPoint;
        textObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        FloatingText floatingText = textObj.AddComponent<FloatingText>();

        textMesh.text = label;
        textMesh.color = color;
        textMesh.fontSize = 64;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.characterSize = size;
        textMesh.anchor = TextAnchor.MiddleCenter;
        floatingText.speed = speed;
        Destroy(textObj, expirationTime);
    }

    public static void Create(Vector3 atPoint, string label)
    {
        Create(atPoint, label, Color.white);
    }

    public static void Create(Vector3 atPoint, string label, Color color)
    {
        Create(atPoint, label, color, 0.5f, 2, 1);
    }

    void Update()
    {
        transform.eulerAngles = UIManager.activeCamera.transform.eulerAngles;
        transform.position += Vector3.up * speed * Time.deltaTime;
    }
}

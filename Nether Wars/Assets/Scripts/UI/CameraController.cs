using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public new Camera camera;
    public Transform focusPoint;
    [Range(0.01f, 0.5f)]
    public float scaleFactor = 0.1f;

    [Range(1f,10f)]
    public float speed = 1;

    [Range(0.05f, 1f)]
    public float threshold = 0.12f;

    void Update()
    {
        if (UIManager.activeMenu == Menues.generic && !Player.mouseOverUI)
            DetectMove();
    }

    public void DetectMove()
    {
        int mouseX = Mathf.RoundToInt(Input.mousePosition.x);
        int mouseY = Mathf.RoundToInt(Input.mousePosition.y);

        if (mouseX < 0 || mouseX > UIManager.screenW || mouseY < 0 || mouseY > UIManager.screenH)
            return;

        int directionX = 0;
        if (mouseX <= UIManager.screenW * threshold)
            directionX = -1;
        else if (mouseX >= UIManager.screenW - UIManager.screenW * threshold)
            directionX = 1;

        int directionZ = 0;
        if (mouseY <= UIManager.screenH * threshold)
            directionZ = 1;
        else if (mouseY >= UIManager.screenH - UIManager.screenH * threshold)
            directionZ = -1;

        transform.position += (directionX * Vector3.forward + directionZ * Vector3.right) * speed * Time.deltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        if (isActiveAndEnabled)
        {
            if (focusPoint && camera)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(focusPoint.position, camera.fieldOfView * scaleFactor);
            }
        }
    }
}

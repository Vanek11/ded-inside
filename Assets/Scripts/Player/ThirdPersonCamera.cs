using UnityEngine;

namespace DI
{
    /// Простая орбитальная камера от третьего лица (серый бокс; на арт-пасе заменим на Cinemachine).
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 5.5f;
        public float height = 1.9f;
        public float sensitivity = 3f;
        public float pitch = 18f;
        public bool freezePitch; // когда держим объект: мышь вверх/вниз = высота руки

        float yaw;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (target) yaw = target.eulerAngles.y;
        }

        void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (!target) return;

            yaw += Input.GetAxis("Mouse X") * sensitivity;
            if (!freezePitch)
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * sensitivity, -15f, 70f);

            var rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pos = target.position + Vector3.up * height - rot * Vector3.forward * distance;
            transform.SetPositionAndRotation(pos, rot);
        }
    }
}

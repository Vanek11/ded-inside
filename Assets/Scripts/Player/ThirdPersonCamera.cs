using UnityEngine;

namespace DI
{
    /// Камера от третьего лица (серый бокс; на арт-пасе заменим на Cinemachine).
    /// F5 — переключение вида как в Minecraft: за спиной / спереди / первое лицо.
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 5.5f;
        public float height = 1.9f;
        public float pitch = 18f;
        public bool freezePitch; // когда держим объект: мышь вверх/вниз = высота руки

        float yaw;
        int viewMode; // 0 = за спиной, 1 = спереди, 2 = первое лицо
        Transform characterVisual;

        float Sens => SessionState.Sens;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (target) yaw = target.eulerAngles.y;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                viewMode = (viewMode + 1) % 3;
                if (characterVisual == null && target != null)
                {
                    var ch = target.Find("Character");
                    if (ch != null) characterVisual = ch;
                }
                if (characterVisual != null)
                    characterVisual.gameObject.SetActive(viewMode != 2); // в первом лице себя не видно
            }
        }

        void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (!target) return;
            if (PauseMenu.Open) return; // на паузе камера замирает — курсор крутит только себя

            yaw += Input.GetAxis("Mouse X") * Sens;
            if (!freezePitch)
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * Sens, -15f, 70f);

            switch (viewMode)
            {
                case 0: // за спиной
                {
                    var rot = Quaternion.Euler(pitch, yaw, 0f);
                    var pos = target.position + Vector3.up * height - rot * Vector3.forward * distance;
                    transform.SetPositionAndRotation(pos, rot);
                    break;
                }
                case 1: // спереди (зеркальный вид)
                {
                    var rot = Quaternion.Euler(pitch, yaw + 180f, 0f);
                    var pos = target.position + Vector3.up * height + rot * Vector3.forward * distance;
                    transform.SetPositionAndRotation(pos, rot);
                    transform.LookAt(target.position + Vector3.up * 1.4f);
                    break;
                }
                case 2: // первое лицо: глаза персонажа, свободный наклон
                {
                    var rot = Quaternion.Euler(Mathf.Clamp(pitch, -25f, 60f), yaw, 0f);
                    var pos = target.position + Vector3.up * 1.55f;
                    transform.SetPositionAndRotation(pos, rot);
                    break;
                }
            }
        }
    }
}

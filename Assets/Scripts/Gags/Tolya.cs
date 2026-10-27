using UnityEngine;

namespace DI
{
    /// Бомж Толя: лежит на скамейке у дороги, просится «подвезти».
    /// Залезает в медленный гроб сам («там тепло»), на сильном крене выпадает.
    /// Если донесли внутри гроба — бонус «Попутчик». Можно схватить ЛКМ и унести.
    public class Tolya : MonoBehaviour
    {
        public Vector3 axis = Vector3.forward;

        Rigidbody rb;
        bool introduced;
        bool aboard;
        float nextAsk = 2f;

        public static GameObject Spawn(Vector3 pos, Vector3 axis)
        {
            // скамейка (модель кладбищенской лавки)
            var bench = Art.Model("graveyard/bench", pos + new Vector3(0f, LevelHelpers.Ground(pos.x, pos.z), 0f),
                90f, 1f, 0.6f);

            // сам Толя
            var go = new GameObject("Tolya");
            go.layer = LayerMask.NameToLayer("Grabbable");
            go.transform.position = pos + new Vector3(0f, 0.62f, 0f);
            go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            go.AddComponent<NetDynamicMarker>();

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 65f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var col = go.AddComponent<CapsuleCollider>();
            col.radius = 0.28f;
            col.height = 1.4f;
            col.center = new Vector3(0f, 0.7f, 0f);

            var coat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            coat.name = "Coat";
            Object.DestroyImmediate(coat.GetComponent<Collider>());
            coat.transform.SetParent(go.transform, false);
            coat.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            coat.transform.localScale = new Vector3(0.62f, 0.9f, 0.45f);
            coat.GetComponent<Renderer>().material.color = new Color(0.4f, 0.33f, 0.24f);

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.transform.SetParent(go.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            head.transform.localScale = Vector3.one * 0.36f;
            head.GetComponent<Renderer>().material.color = new Color(0.82f, 0.65f, 0.52f);

            var beard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beard.name = "Beard";
            Object.DestroyImmediate(beard.GetComponent<Collider>());
            beard.transform.SetParent(go.transform, false);
            beard.transform.localPosition = new Vector3(0f, 1.22f, 0.14f);
            beard.transform.localScale = new Vector3(0.24f, 0.2f, 0.12f);
            beard.GetComponent<Renderer>().material.color = new Color(0.6f, 0.6f, 0.6f);

            var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cap.name = "Cap";
            Object.DestroyImmediate(cap.GetComponent<Collider>());
            cap.transform.SetParent(go.transform, false);
            cap.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            cap.transform.localScale = new Vector3(0.4f, 0.12f, 0.4f);
            cap.GetComponent<Renderer>().material.color = new Color(0.2f, 0.25f, 0.2f);

            var t = go.AddComponent<Tolya>();
            t.rb = rb;
            t.axis = axis.normalized;
            return go;
        }

        void Update()
        {
            var game = DIGameManager.I;
            var coffin = game != null ? game.coffin : null;
            if (coffin == null || game.Phase != OrderPhase.Carry) return;

            float dist = Vector3.Distance(transform.position, coffin.transform.position);

            if (!introduced && dist < 7f)
            {
                introduced = true;
                game.Event("Бомж Толя: «О, похоронка!.. В гробу тепло, подвезите?»");
            }

            if (!aboard)
            {
                nextAsk -= Time.deltaTime;
                if (nextAsk <= 0f && dist < 9f)
                {
                    nextAsk = Random.Range(10f, 18f);
                    game.Event("Толя: «Ну подвезите, а? Мне в том направлении...»");
                }

                var crb = coffin.GetComponent<Rigidbody>();
                float mySpeed = rb.linearVelocity.magnitude;
                if (dist < 2.4f && crb.linearVelocity.magnitude < 1.2f
                    && coffin.TiltDeg < 35f && mySpeed < 1.5f)
                {
                    transform.SetParent(coffin.transform);
                    transform.localPosition = new Vector3(0f, 0.42f, 0f);
                    transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    rb.isKinematic = true;
                    aboard = true;
                    Sfx.PlayAt("thump", transform.position);
                    game.Event("Толя залез в гроб! («там тепло»)");
                    game.AddStyle(10f);
                }
            }
            else if (coffin.TiltDeg > 50f)
            {
                transform.SetParent(null);
                rb.isKinematic = false;
                rb.linearVelocity = coffin.GetComponent<Rigidbody>().linearVelocity + Vector3.up * 2f;
                aboard = false;
                game.Event("Толя выпал на крене! («я сам хотел, между прочим»)");
            }
        }

        /// Сидит ли Толя в гробу (для бонуса «Попутчик»).
        public bool Aboard => aboard && transform.IsChildOf(DIGameManager.I != null && DIGameManager.I.coffin != null
            ? DIGameManager.I.coffin.transform : transform);
    }
}

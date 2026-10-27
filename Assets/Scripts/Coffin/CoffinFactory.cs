using UnityEngine;

namespace DI
{
    /// Сборка гроба из примитивов: корпус с полостью, крышка на петле, 4 ручки.
    /// Дед кладётся отдельно через GrandpaFactory.Build(transform).
    public static class CoffinFactory
    {
        // размеры в метрах
        const float Length = 2.05f, Width = 0.72f, Height = 0.62f, Wall = 0.06f;

        public static CoffinBody Build(Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("Coffin");
            root.layer = LayerMask.NameToLayer("Grabbable");
            root.transform.SetPositionAndRotation(position, rotation);

            var rb = root.AddComponent<Rigidbody>();
            rb.mass = 120f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            Color wood = new Color(0.45f, 0.30f, 0.18f);
            Color brass = new Color(0.80f, 0.65f, 0.20f);

            // дно и стенки (составной коллайдер, полость внутри)
            Box(root, "Bottom", new Vector3(Width - 2 * Wall, 0.08f, Length - 2 * Wall),
                new Vector3(0f, -Height / 2f + 0.04f, 0f), wood);
            Box(root, "WallLeft", new Vector3(Wall, Height, Length),
                new Vector3(-(Width / 2f - Wall / 2f), 0f, 0f), wood);
            Box(root, "WallRight", new Vector3(Wall, Height, Length),
                new Vector3(Width / 2f - Wall / 2f, 0f, 0f), wood);
            Box(root, "WallHead", new Vector3(Width - 2 * Wall, Height, Wall),
                new Vector3(0f, 0f, Length / 2f - Wall / 2f), wood);
            Box(root, "WallFoot", new Vector3(Width - 2 * Wall, Height, Wall),
                new Vector3(0f, 0f, -(Length / 2f - Wall / 2f)), wood);

            // ручки-мишени для захвата, торчат по бокам (латунь)
            for (int i = 0; i < 2; i++)
                for (int s = 0; s < 2; s++)
                {
                    float z = (i == 0 ? -1 : 1) * (Length / 2f - 0.45f);
                    float x = (s == 0 ? -1 : 1) * (Width / 2f + 0.06f);
                    var handle = Box(root, $"Handle_{i}{s}", new Vector3(0.14f, 0.10f, 0.26f),
                        new Vector3(x, 0.02f, z), brass);
                    Art.Paint(handle.GetComponent<Renderer>(), brass, 0.85f, 1f);
                }

            var body = root.AddComponent<CoffinBody>();

            // крышка: отдельное тело на петле у левого края, пружина держит закрытой,
            // сильный удар срывает петли
            var lidGo = new GameObject("Coffin_Lid");
            lidGo.layer = root.layer;
            Vector3 lidLocal = new Vector3(0f, Height / 2f + 0.04f, 0f);
            lidGo.transform.SetPositionAndRotation(root.transform.TransformPoint(lidLocal), rotation);

            var lidRb = lidGo.AddComponent<Rigidbody>();
            lidRb.mass = 10f;
            lidRb.interpolation = RigidbodyInterpolation.Interpolate;
            lidRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var lidCol = lidGo.AddComponent<BoxCollider>();
            lidCol.size = new Vector3(Width + 0.04f, 0.08f, Length + 0.04f);

            var lidMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lidMesh.name = "LidVisual";
            Object.DestroyImmediate(lidMesh.GetComponent<Collider>());
            lidMesh.transform.SetParent(lidGo.transform, false);
            lidMesh.transform.localScale = lidCol.size;
            Art.Paint(lidMesh.GetComponent<Renderer>(), wood, 0.55f, 0f, "wood", 2f); // лак

            // траурные ленты крест-накрест
            foreach (bool along in new[] { true, false })
            {
                var band = GameObject.CreatePrimitive(PrimitiveType.Cube);
                band.name = along ? "RibbonLength" : "RibbonCross";
                Object.DestroyImmediate(band.GetComponent<Collider>());
                band.transform.SetParent(lidGo.transform, false);
                band.transform.localPosition = new Vector3(0f, 0.045f, 0f);
                band.transform.localScale = along
                    ? new Vector3(0.3f, 0.06f, lidCol.size.z + 0.02f)
                    : new Vector3(lidCol.size.x + 0.02f, 0.06f, 0.3f);
                band.GetComponent<Renderer>().material.color = new Color(0.55f, 0.1f, 0.12f);
            }

            var hinge = lidGo.AddComponent<HingeJoint>();
            hinge.connectedBody = rb;
            hinge.anchor = new Vector3(-Width / 2f, 0f, 0f);
            hinge.axis = Vector3.forward;
            hinge.useSpring = true;
            float lidSpring = 200f;
            if (SessionState.HasGear("rems")) lidSpring = 440f; // ремни: крышка держится вдвое крепче
            hinge.spring = new JointSpring { spring = lidSpring, damper = 15f, targetPosition = 0f };
            hinge.breakForce = SessionState.HasGear("tape") ? 9000f : 3500f; // скотч-пистолет
            lidGo.AddComponent<LidBreakReporter>();

            body.Lid = lidRb;
            lidGo.AddComponent<NetDynamicMarker>();
            root.AddComponent<NetDynamicMarker>();
            return body;
        }

        static GameObject Box(GameObject parent, string name, Vector3 size, Vector3 localPos, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.layer = parent.layer;
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = size;
            Art.Paint(go.GetComponent<Renderer>(), color, 0.35f, 0f, "wood", 2f);
            return go;
        }
    }
}

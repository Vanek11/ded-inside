using UnityEngine;

namespace DI
{
    /// Дед-рэгдолл из примитивов: таз, торс, голова (с усами), руки, ноги.
    /// Лежит в гробу вдоль его оси Z, головой к WallHead. НЕ пристёгнут.
    public static class GrandpaFactory
    {
        static readonly Color Suit = new Color(0.35f, 0.37f, 0.45f);
        static readonly Color Skin = new Color(0.85f, 0.68f, 0.55f);

        public static GrandpaState Build(Transform frame)
        {
            Quaternion lying = Quaternion.Euler(90f, 0f, 0f); // капсулы конечностей — вдоль Z

            var pelvis = Part("Pelvis", new Vector3(0f, -0.13f, 0.05f), new Vector3(0.34f, 0.20f, 0.30f), 20f, Suit, frame, PrimitiveType.Cube, Quaternion.identity);
            var torso  = Part("Torso",  new Vector3(0f, -0.13f, 0.445f), new Vector3(0.36f, 0.20f, 0.49f), 30f, Suit, frame, PrimitiveType.Cube, Quaternion.identity);
            var head   = Part("Head",   new Vector3(0f, -0.10f, 0.78f), Vector3.one * 0.26f, 6f, Skin, frame, PrimitiveType.Sphere, Quaternion.identity);
            var armL   = Part("ArmL",   new Vector3(-0.225f, -0.17f, 0.30f), new Vector3(0.12f, 0.30f, 0.12f), 5f, Suit, frame, PrimitiveType.Capsule, lying);
            var armR   = Part("ArmR",   new Vector3(0.225f, -0.17f, 0.30f), new Vector3(0.12f, 0.30f, 0.12f), 5f, Suit, frame, PrimitiveType.Capsule, lying);
            var legL   = Part("LegL",   new Vector3(-0.09f, -0.16f, -0.43f), new Vector3(0.15f, 0.45f, 0.15f), 7f, Suit, frame, PrimitiveType.Capsule, lying);
            var legR   = Part("LegR",   new Vector3(0.09f, -0.16f, -0.43f), new Vector3(0.15f, 0.45f, 0.15f), 7f, Suit, frame, PrimitiveType.Capsule, lying);

            var state = pelvis.gameObject.AddComponent<GrandpaState>();
            Attach(pelvis.gameObject, state, false);
            Attach(torso.gameObject, state, false);
            Attach(head.gameObject, state, true);
            Attach(armL.gameObject, state, false);
            Attach(armR.gameObject, state, false);
            Attach(legL.gameObject, state, false);
            Attach(legR.gameObject, state, false);

            // усы — без коллайдера, чисто для достоинства
            var mustache = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mustache.name = "Mustache";
            Object.DestroyImmediate(mustache.GetComponent<Collider>());
            mustache.transform.SetParent(head.transform, false);
            mustache.transform.localPosition = new Vector3(0f, -0.02f, 0.10f);
            mustache.transform.localScale = new Vector3(0.14f, 0.05f, 0.05f);
            mustache.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f);

            // суставы
            Connect(torso, pelvis, frame, new Vector3(0f, -0.13f, 0.21f));
            Connect(head, torso, frame, new Vector3(0f, -0.11f, 0.66f));
            Connect(armL, torso, frame, new Vector3(-0.225f, -0.13f, 0.60f));
            Connect(armR, torso, frame, new Vector3(0.225f, -0.13f, 0.60f));
            Connect(legL, pelvis, frame, new Vector3(-0.09f, -0.15f, -0.10f));
            Connect(legR, pelvis, frame, new Vector3(0.09f, -0.15f, -0.10f));

            return state;
        }

        static Rigidbody Part(string name, Vector3 localPos, Vector3 scale, float mass, Color color,
                              Transform frame, PrimitiveType type, Quaternion localRot)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.layer = LayerMask.NameToLayer("Grandpa");
            go.transform.SetPositionAndRotation(frame.TransformPoint(localPos), frame.rotation * localRot);
            go.transform.localScale = scale;

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            go.GetComponent<Renderer>().material.color = color;
            return rb;
        }

        static void Attach(GameObject part, GrandpaState root, bool isHead)
        {
            var p = part.AddComponent<GrandpaPart>();
            p.Root = root;
            p.IsHead = isHead;
        }

        static void Connect(Rigidbody child, Rigidbody parent, Transform frame, Vector3 jointLocalInFrame)
        {
            var j = child.gameObject.AddComponent<CharacterJoint>();
            j.connectedBody = parent;
            j.anchor = child.transform.InverseTransformPoint(frame.TransformPoint(jointLocalInFrame));
            j.axis = Vector3.right;
            j.swing1Limit = new SoftJointLimit { limit = 45f };
            j.swing2Limit = new SoftJointLimit { limit = 45f };
            j.lowTwistLimit = new SoftJointLimit { limit = -40f };
            j.highTwistLimit = new SoftJointLimit { limit = 40f };
            j.enablePreprocessing = false; // стабильнее при спагетти-суставах
        }
    }
}

using System.Collections.Generic;
using Clover.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds Khepharu, the Sun-Bearer, from boss_sunform.fbx: twenty rigid pieces (and the falcon's two boned wings)
    /// set round the thorax where the modeller's notes put them, the sun and its dull twin, the parts a blow can land
    /// on, and every mark its attacks draw. The scarab's pieces and the falcon's hang under two holders so the fight
    /// shows one shape at a time; the thorax, the king's face and the wing cases belong to both.
    /// </summary>
    public static class SunBearerBuilder
    {
        // The thorax's origin is the middle of its underside, this far above the floor when the scarab stands.
        private static readonly Vector3 Thorax = new Vector3(0f, 0.7f, 0f);

        public static SunBearer Build(Transform target, float floorRadius, Material marker)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(SunCityKit.SunFormPath);
            if (asset == null)
            {
                Debug.LogError($"[Clover.Build] No model at {SunCityKit.SunFormPath}; the Sun-Bearer was not built.");
                return null;
            }
            var root = new GameObject("Sun Bearer");
            var model = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            SunCityKit.Dress(model);

            var body = BuildKit.Group("Body", root.transform);
            var scarab = BuildKit.Group("Scarab", body);
            var falcon = BuildKit.Group("Falcon", body);

            // Shared by both shapes.
            Piece(model, "thorax", body, Thorax);
            Piece(model, "face", body, Thorax + new Vector3(0f, 1.05f, 1.2f)).localRotation = Quaternion.Euler(-15f, 0f, 0f);
            var caseLeft = Piece(model, "wing_case_left", body, Thorax + new Vector3(-0.03f, 1.6f, -0.7f));
            var caseRight = Piece(model, "wing_case_right", body, Thorax + new Vector3(0.03f, 1.6f, -0.7f));

            // The scarab.
            Piece(model, "scarab_abdomen", scarab, Thorax + new Vector3(0f, -0.35f, -1f));
            Piece(model, "scarab_head", scarab, Thorax + new Vector3(0f, 0.05f, 1.15f));
            var legs = new List<Transform>();
            foreach (var (name, hip) in new[] { ("foreleg", new Vector3(1.45f, 0.5f, 0.75f)), ("midleg", new Vector3(1.6f, 0.5f, -0.2f)), ("hindleg", new Vector3(1.5f, 0.4f, -1.9f)) })
            {
                legs.Add(Piece(model, $"scarab_{name}_left", scarab, Thorax + new Vector3(-hip.x, hip.y, hip.z)));
                legs.Add(Piece(model, $"scarab_{name}_right", scarab, Thorax + hip));
            }

            // The falcon.
            Piece(model, "falcon_head", falcon, Thorax + new Vector3(0f, 1.75f, 0.9f));
            Piece(model, "falcon_tail", falcon, Thorax + new Vector3(0f, 0.6f, -1.1f));
            Piece(model, "falcon_talon_left", falcon, Thorax + new Vector3(-0.7f, 0.15f, -0.2f));
            Piece(model, "falcon_talon_right", falcon, Thorax + new Vector3(0.7f, 0.15f, -0.2f));
            var wingLeft = Piece(model, "falcon_wing_left", falcon, Thorax + new Vector3(-1.5f, 1.5f, -0.1f));
            var wingRight = Piece(model, "falcon_wing_right", falcon, Thorax + new Vector3(1.5f, 1.5f, -0.1f));

            // The sun, burning and dull, under one holder that the fight moves.
            var sun = BuildKit.Group("Sun", root.transform);
            var burning = Piece(model, "sun", sun, Vector3.zero);
            var dull = Piece(model, "sun_loose", sun, Vector3.zero);
            foreach (var renderer in sun.GetComponentsInChildren<Renderer>(true)) renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Object.DestroyImmediate(model);

            var renderers = new List<Renderer>(body.GetComponentsInChildren<Renderer>(true));
            foreach (var renderer in renderers)
                if (renderer is SkinnedMeshRenderer skinned) skinned.updateWhenOffscreen = true;

            var bearer = root.AddComponent<SunBearer>();
            bearer.FloorRadius = floorRadius;

            // What a blow can land on: the head (and the king's face), the trunk, and the sun when it lies loose.
            var headReach = BuildKit.Group("Head Reach", body);
            var skull = headReach.gameObject.AddComponent<SphereCollider>();
            skull.center = new Vector3(0f, 1.3f, 2f);
            skull.radius = 1.6f;
            var headPart = headReach.gameObject.AddComponent<ColossusPart>();
            headPart.Configure(bearer, skull, true, bearer.HeadDamage);
            var trunkReach = BuildKit.Group("Body Reach", body);
            var trunk = trunkReach.gameObject.AddComponent<BoxCollider>();
            // Tall, so the falcon standing on the sun can be reached from the floor.
            trunk.center = new Vector3(0f, 0.4f, -1.1f);
            trunk.size = new Vector3(4f, 6.4f, 5.6f);
            var bodyPart = trunkReach.gameObject.AddComponent<ColossusPart>();
            bodyPart.Configure(bearer, trunk, false, 1f);
            var sunReach = BuildKit.Group("Sun Reach", sun);
            var ball = sunReach.gameObject.AddComponent<SphereCollider>();
            ball.radius = 1.5f;
            var sunPart = sunReach.gameObject.AddComponent<ColossusPart>();
            sunPart.Configure(bearer, ball, false, 1f);
            // The scarab on the floor is solid: nobody walks through it.
            var bulk = BuildKit.Group("Bulk", body).gameObject.AddComponent<BoxCollider>();
            bulk.center = new Vector3(0f, 1.4f, -0.7f);
            bulk.size = new Vector3(3.2f, 2.8f, 4.8f);
            bulk.enabled = false;

            // The falcon's shadow on the floor, the first tell of everything it does; and the line of light the sun comes down.
            var blob = BuildKit.Shape(PrimitiveType.Cylinder, "Falcon Shadow", root.transform, Vector3.zero, new Vector3(5f, 0.004f, 5f), SunCityKit.Shade);
            blob.GetComponent<MeshFilter>().sharedMesh = BuildKit.RoundMesh();
            blob.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            blob.enabled = false;
            var line = BuildKit.Shape(PrimitiveType.Cylinder, "Light Line", root.transform, Vector3.zero, new Vector3(0.3f, 20f, 0.3f), SunCityKit.For("m_sunform_sun"));
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.enabled = false;

            bearer.Configure(target, body, scarab, falcon, caseLeft, caseRight, legs.ToArray(), Chain(wingLeft, "wing_l_"), Chain(wingRight, "wing_r_"),
                sun, burning.gameObject, dull.gameObject, blob.transform, line.transform, bulk, headPart, bodyPart, sunPart, renderers.ToArray());

            var marks = BuildKit.Group("Marks", root.transform);
            bearer.ConfigureMarks(Pairs("Lane", 2, marks, marker), Pairs("Circle", 2, marks, marker), Pairs("Fall", 18, marks, marker),
                Pairs("Hot Lane", 5, marks, marker), Pairs("Hot Circle", 3, marks, marker), Pairs("Floor", 1, marks, marker), Pairs("Shadow", 1, marks, marker),
                WorldBars.Marker("Roar Ring", marks, 0f, marker));

            // Drawn through whatever is in front of it: where the player is, while the falcon or the sun covers them.
            var finderLight = BuildKit.Glow(BuildKit.GeneratedRoot + "/Enemies", "m_player_finder", new Color(1.6f, 1.45f, 0.9f, 0.95f), 4, false, 2f, 0f, true);
            finderLight.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay;
            var finder = BuildKit.Shape(PrimitiveType.Quad, "Player Finder", root.transform, Vector3.zero, Vector3.one * 2.2f, finderLight);
            finder.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            finder.receiveShadows = false;
            finder.enabled = false;
            var towardCamera = Camera.main != null ? Camera.main.transform.rotation : Quaternion.Euler(55f, 0f, 0f);
            bearer.ConfigureFeedback(finder, WorldBars.FinishMark(root.transform, new Vector3(0f, 4f, 0f), 4f, towardCamera));
            return bearer;
        }

        // One piece of the model under a holder of its own, at the point the game moves it from. The mesh keeps the
        // turn the import gave it; a boned piece brings its rig with it.
        private static Transform Piece(GameObject model, string modelName, Transform parent, Vector3 at)
        {
            var holder = BuildKit.Group(modelName, parent);
            holder.localPosition = at;
            foreach (string name in new[] { modelName + "_rig", modelName })
            {
                var mesh = BuildKit.FindDeep(model.transform, name);
                if (mesh == null)
                {
                    if (name == modelName) Debug.LogError($"[Clover.Build] The Sun-Bearer's model lacks {modelName}.");
                    continue;
                }
                var turn = mesh.localRotation;
                mesh.SetParent(holder, false);
                mesh.localPosition = Vector3.zero;
                mesh.localRotation = turn;
                mesh.localScale = Vector3.one;
            }
            return holder;
        }

        private static Transform[] Chain(Transform wing, string prefix)
        {
            var bones = new List<Transform>();
            for (int i = 1; i <= 3; i++)
            {
                var bone = BuildKit.FindDeep(wing, prefix + i);
                if (bone != null) bones.Add(bone);
            }
            if (bones.Count < 3) Debug.LogError($"[Clover.Build] The falcon's wing has no bones {prefix}1..3; it will not beat.");
            return bones.ToArray();
        }

        private static MeshFilter[] Pairs(string name, int count, Transform marks, Material marker)
        {
            var pairs = new MeshFilter[count * 2];
            for (int i = 0; i < count; i++)
            {
                pairs[i * 2] = WorldBars.Marker($"{name} Outline {i + 1}", marks, 0f, marker);
                pairs[i * 2 + 1] = WorldBars.Marker($"{name} Fill {i + 1}", marks, 0f, marker);
            }
            return pairs;
        }
    }
}

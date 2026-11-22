using System.Collections.Generic;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Where the effects on a body go, measured from that body: its feet, waist, chest, head and crown, how far its
    /// torso reaches to the sides, the front and the back at each height, where its hands are, and its size. The
    /// element looks were drawn round one clay stand-in figure (the Blender worker's enemy_husk) and every look on an
    /// enemy is carried from that figure to the real body by these: a piece on the stand-in's chest lands on the real
    /// chest, a crystal on its back on the real back, and the piece is grown or shrunk with the body (the owner,
    /// 2026-11-21: "some effects look misplaced on smaller enemies compared to bigger ones and vice versa").
    ///
    /// Measured when the enemy is built (<see cref="BodyMeasurement"/>, which the game cannot do itself: its models'
    /// meshes are not kept readable in play), from its own skinned mesh and skeleton in the pose it was bound in (so a
    /// crouch or a swing at that moment does not count): the joints by the bones' names every
    /// humanoid model here shares (hips, chest, neck, head, hand.L, hand.R), the crown by the highest point the head
    /// carries, the reach by the body's own vertices with the arms, the weapon and the cloth that trails left out. This
    /// also keeps the table the player's surge looks were fitted with before (the median reach in each height band of
    /// the posed body, <see cref="Ratio"/>), which the player's own looks still use as they did.
    ///
    /// A fixed table of bodies; measuring one allocates once, nothing is allocated after.
    /// </summary>
    public static class BodyAnchors
    {
        /// <summary>Height bands a body is measured in, and the bodies kept measured at once.</summary>
        public const int Bands = 10, Most = 32;

        /// <summary>The stand-in's reach out from its upright, all round: legs, chest, head (by share of its height).</summary>
        public const float StandInLegs = 0.32f, StandInChest = 0.42f, StandInHead = 0.27f;

        /// <summary>The least and most a piece's reach out from the player's body is scaled by (the surge looks' fit).</summary>
        public const float LeastRatio = 0.4f, MostRatio = 1.25f;

        /// <summary>The least and most a piece's reach out from an enemy's body is scaled by.</summary>
        public const float LeastReach = 0.25f, MostReach = 1.8f;

        /// <summary>The least and most a piece on an enemy's body is grown by (the owner's 0.6x to 1.8x).</summary>
        public const float LeastSize = 0.6f, MostSize = 1.8f;

        /// <summary>
        /// One body's anchors, metres in its own frame from the ground under its feet (x to its right, y up, z ahead).
        /// The joints are heights; <see cref="Width"/>, <see cref="Front"/> and <see cref="Back"/> are how far the torso
        /// reaches at the chest to either side, ahead and behind.
        /// </summary>
        [System.Serializable]
        public struct Anchors
        {
            public float Waist, Chest, Head, Crown;
            public float Width, Front, Back;

            /// <summary>The torso's reach out from its upright at the chest, the mean of the four ways.</summary>
            public float Girth => 0.5f * Width + 0.25f * (Front + Back);
        }

        /// <summary>The bones the anchors are read from as the body moves: its hips, chest, neck and head, and its hands.</summary>
        [System.Serializable]
        public struct Joints
        {
            public Transform Hips, Chest, Neck, Head, HandLeft, HandRight;

            public bool Whole => Hips != null && Chest != null && Neck != null && Head != null;
        }

        /// <summary>
        /// Where the anchors stand on a body as it is posed now (a crouch, a lean, a body knocked down): each height read
        /// from its bones, and how far the hips, the chest and the head stand off the upright through its feet, along the
        /// floor. The torso's reach and the crown's height over the head are the bound pose's.
        /// </summary>
        public struct Posed
        {
            public Anchors Anchors;
            public Vector3 WaistShift, ChestShift, HeadShift;
        }

        /// <summary>The least share of its size a piece keeps on a folded body (<see cref="FoldOf"/>).</summary>
        public const float LeastFold = 0.75f;

        /// <summary>
        /// How much smaller a piece on a body is drawn while the body is folded (a crouch, a hunch): its torso from hips to
        /// face now over as it was bound, never under <see cref="LeastFold"/> and never over 1. A dagger in a crouching
        /// Knife Acolyte's back kept its standing length and stood up over its head (the size check of 2026-11-21).
        /// </summary>
        public static float FoldOf(in Anchors bound, in Anchors posed)
        {
            float standing = bound.Head - bound.Waist;
            return standing > 1e-3f ? Mathf.Clamp((posed.Head - posed.Waist) / standing, LeastFold, 1f) : 1f;
        }

        /// <summary>
        /// How far along the floor a piece at a height on the stand-in is carried with the body's lean: nothing at the feet,
        /// the hips' shift at the waist, the chest's at the chest, the head's at the head and over it.
        /// </summary>
        public static Vector3 ShiftAt(in Posed body, float standInHeight)
        {
            float y = standInHeight;
            if (y <= 0f) return Vector3.zero;
            if (y <= StandIn.Waist) return Vector3.Lerp(Vector3.zero, body.WaistShift, y / StandIn.Waist);
            if (y <= StandIn.Chest) return Vector3.Lerp(body.WaistShift, body.ChestShift, (y - StandIn.Waist) / (StandIn.Chest - StandIn.Waist));
            if (y <= StandIn.Head) return Vector3.Lerp(body.ChestShift, body.HeadShift, (y - StandIn.Chest) / (StandIn.Head - StandIn.Chest));
            return body.HeadShift;
        }

        /// <summary>
        /// A body's anchors as its joints stand now, about its feet (in the world's axes): the bound anchors moved to the
        /// joints' heights, kept in order from the feet up however the body is folded.
        /// </summary>
        public static Posed PoseOf(in Anchors bound, Vector3 feet, Vector3 hips, Vector3 chest, Vector3 neck, Vector3 head)
        {
            Vector3 chestAt = Vector3.Lerp(chest, neck, 0.5f);
            // The middle of the face: as far on past the skull's joint as the neck runs to it.
            Vector3 face = head + (head - neck);
            var posed = new Posed { Anchors = bound };
            posed.Anchors.Waist = Mathf.Max(0.05f, hips.y - feet.y);
            posed.Anchors.Chest = Mathf.Max(posed.Anchors.Waist + 0.02f, chestAt.y - feet.y);
            posed.Anchors.Head = Mathf.Max(posed.Anchors.Chest + 0.02f, face.y - feet.y);
            posed.Anchors.Crown = posed.Anchors.Head + Mathf.Max(0f, bound.Crown - bound.Head);
            posed.WaistShift = new Vector3(hips.x - feet.x, 0f, hips.z - feet.z);
            posed.ChestShift = new Vector3(chestAt.x - feet.x, 0f, chestAt.z - feet.z);
            posed.HeadShift = new Vector3(face.x - feet.x, 0f, face.z - feet.z);
            return posed;
        }

        /// <summary>
        /// The Temple Soldier as it stands in the castle (its model at its kind's 1.12): the body every look was filmed and
        /// approved on, so the looks keep on it exactly the size and place they had (measured by
        /// <see cref="MeasureAnchors"/>, BodyAnchorsTests holds it to the model).
        /// </summary>
        public static readonly Anchors Soldier = new Anchors
        {
            Waist = 1.098f, Chest = 1.557f, Head = 1.949f, Crown = 2.464f, Width = 0.151f, Front = 0.241f, Back = 0.218f,
        };

        /// <summary>
        /// Before these anchors every look was grown on the castle's soldiers by their bars' height over 1.75 m: 2.0 m over
        /// 1.75, about 1.14. The stand-in is the soldier made that much smaller, so a look on the soldier is unchanged.
        /// </summary>
        public const float SoldierGrowth = 2f / 1.75f;

        /// <summary>The clay stand-in the looks were drawn on, as the looks place things on it (the soldier over <see cref="SoldierGrowth"/>).</summary>
        public static readonly Anchors StandIn = Scaled(Soldier, 1f / SoldierGrowth);

        /// <summary>Every anchor of a body grown by a factor: a bigger model of the same build.</summary>
        public static Anchors Scaled(in Anchors body, float factor) => new Anchors
        {
            Waist = body.Waist * factor, Chest = body.Chest * factor, Head = body.Head * factor, Crown = body.Crown * factor,
            Width = body.Width * factor, Front = body.Front * factor, Back = body.Back * factor,
        };

        /// <summary>
        /// How much a piece on this body is grown: the height of its head over the stand-in's, held between 0.6x and 1.8x.
        /// The head and not the crown, so a soldier's crested helmet or a tall hat does not grow every look on it.
        /// </summary>
        public static float SizeOf(in Anchors body) =>
            StandIn.Head > 0f ? Mathf.Clamp(body.Head / StandIn.Head, LeastSize, MostSize) : 1f;

        /// <summary>
        /// Where a height on the stand-in falls on this body: feet to feet, waist to waist, chest to chest, head to head
        /// and crown to crown, straight between them. Below the feet, and above the crown (a halo, a cloud over the
        /// head), it goes on by the body's size.
        /// </summary>
        public static float HeightOn(in Anchors body, float standInHeight)
        {
            float y = standInHeight;
            if (y <= 0f) return y * SizeOf(body);
            if (y <= StandIn.Waist) return Between(y, 0f, StandIn.Waist, 0f, body.Waist);
            if (y <= StandIn.Chest) return Between(y, StandIn.Waist, StandIn.Chest, body.Waist, body.Chest);
            if (y <= StandIn.Head) return Between(y, StandIn.Chest, StandIn.Head, body.Chest, body.Head);
            if (y <= StandIn.Crown) return Between(y, StandIn.Head, StandIn.Crown, body.Head, body.Crown);
            return body.Crown + (y - StandIn.Crown) * SizeOf(body);
        }

        private static float Between(float y, float from, float to, float onFrom, float onTo) =>
            to - from > 1e-5f ? Mathf.Lerp(onFrom, onTo, (y - from) / (to - from)) : onFrom;

        /// <summary>The stand-in's reach out from its upright at a share (0 feet, 1 crown) of its height.</summary>
        public static float StandInReach(float share) => share < 0.45f ? StandInLegs : share < 0.82f ? StandInChest : StandInHead;

        /// <summary>
        /// How far out a torso shaped as an ellipse reaches toward a way along the floor in its own frame (x to its right,
        /// z ahead): <paramref name="width"/> to either side, <paramref name="front"/> ahead, <paramref name="back"/> behind.
        /// </summary>
        public static float ReachToward(float width, float front, float back, Vector2 way)
        {
            if (way.sqrMagnitude < 1e-8f) return 0.5f * width + 0.25f * (front + back);
            way.Normalize();
            float depth = way.y >= 0f ? front : back;
            if (width <= 1e-4f || depth <= 1e-4f) return Mathf.Max(width, depth);
            float x = way.x / width, z = way.y / depth;
            return 1f / Mathf.Sqrt(x * x + z * z);
        }

        // ---- The table.

        private static readonly Transform[] bodies = new Transform[Most];
        private static readonly float[] heights = new float[Most];
        private static readonly float[] radii = new float[Most * Bands];
        private static readonly bool[] anchored = new bool[Most];
        private static readonly Anchors[] anchors = new Anchors[Most];
        private static readonly float[] widths = new float[Most * Bands], fronts = new float[Most * Bands], backs = new float[Most * Bands];
        private static readonly Joints[] joints = new Joints[Most];
        private static readonly Posed[] posedNow = new Posed[Most];
        private static readonly int[] posedFrame = new int[Most];
        private static int next;
        private static Mesh baked;
        private static readonly List<Vector3> vertices = new List<Vector3>();
        private static readonly List<BoneWeight> weights = new List<BoneWeight>();
        private static readonly List<Matrix4x4> binds = new List<Matrix4x4>();
        private static readonly List<float>[] byBand = new List<float>[Bands];
        private static readonly List<float>[] sideBand = new List<float>[Bands], frontBand = new List<float>[Bands], backBand = new List<float>[Bands];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            System.Array.Clear(bodies, 0, Most);
            System.Array.Clear(heights, 0, Most);
            System.Array.Clear(radii, 0, radii.Length);
            System.Array.Clear(anchored, 0, Most);
            System.Array.Clear(anchors, 0, Most);
            System.Array.Clear(widths, 0, widths.Length);
            System.Array.Clear(fronts, 0, fronts.Length);
            System.Array.Clear(backs, 0, backs.Length);
            System.Array.Clear(joints, 0, Most);
            System.Array.Clear(posedNow, 0, Most);
            System.Array.Clear(posedFrame, 0, Most);
            System.Array.Clear(Band, 0, Bands);
            System.Array.Clear(BandFront, 0, Bands);
            System.Array.Clear(BandBack, 0, Bands);
            next = 0;
            baked = null;
            vertices.Clear();
            weights.Clear();
            binds.Clear();
        }

        /// <summary>The body's slot in the table, measuring it the first time; -1 for one with no skinned body to measure.</summary>
        public static int Slot(Transform body)
        {
            if (body == null) return -1;
            for (int i = 0; i < Most; i++)
                if (ReferenceEquals(bodies[i], body)) return heights[i] > 0f ? i : -1;
            int slot = next;
            next = (next + 1) % Most;
            bodies[slot] = body;
            heights[slot] = Measure(body, slot) ? heights[slot] : 0f;
            anchored[slot] = heights[slot] > 0f && Anchor(body, slot);
            posedFrame[slot] = -1;
            return heights[slot] > 0f ? slot : -1;
        }

        /// <summary>The slot of a measured body standing within half a metre of a point on the floor; -1 for none.</summary>
        public static int At(Vector3 feet)
        {
            for (int i = 0; i < Most; i++)
            {
                var body = bodies[i];
                if (body == null || heights[i] <= 0f) continue;
                Vector3 off = body.position - feet;
                off.y = 0f;
                if (off.sqrMagnitude < 0.25f) return i;
            }
            return -1;
        }

        /// <summary>A body's anchors; false (and the stand-in's grown as the old looks grew it) when it could not be measured.</summary>
        public static bool TryAnchors(int slot, out Anchors body)
        {
            if (slot >= 0 && slot < Most && anchored[slot])
            {
                body = anchors[slot];
                return true;
            }
            body = Soldier;
            return false;
        }

        /// <summary>Where a measured body's hand is now (it moves with the animation); its feet when it has no such hand.</summary>
        public static Vector3 Hand(int slot, bool left)
        {
            if (slot < 0 || slot >= Most || bodies[slot] == null) return Vector3.zero;
            var hand = left ? joints[slot].HandLeft : joints[slot].HandRight;
            return hand != null ? hand.position : bodies[slot].position;
        }

        /// <summary>
        /// A measured body's anchors as it stands this frame (<see cref="PoseOf"/>), worked out once a frame; as bound,
        /// with no lean, when its joints are not known. False for a body not measured.
        /// </summary>
        public static bool TryPosed(int slot, out Posed body)
        {
            body = default;
            if (slot < 0 || slot >= Most || !anchored[slot] || bodies[slot] == null) return false;
            int frame = Time.frameCount;
            if (posedFrame[slot] != frame)
            {
                var j = joints[slot];
                posedNow[slot] = j.Whole
                    ? PoseOf(anchors[slot], bodies[slot].position, j.Hips.position, j.Chest.position, j.Neck.position, j.Head.position)
                    : new Posed { Anchors = anchors[slot] };
                posedFrame[slot] = frame;
            }
            body = posedNow[slot];
            return true;
        }

        /// <summary>
        /// How far out a measured enemy's torso reaches at a height over its feet toward a way along the floor in the
        /// world (its own turn taken off); the stand-in's reach for a body not measured.
        /// </summary>
        public static float ReachAt(int slot, float height, Vector3 worldWay) => ReachAt(slot, height, anchors[Mathf.Clamp(slot, 0, Most - 1)].Crown, worldWay);

        /// <summary>The same, for a body standing <paramref name="crown"/> tall now (crouched, it is shorter than it was bound).</summary>
        public static float ReachAt(int slot, float height, float crown, Vector3 worldWay)
        {
            if (slot < 0 || slot >= Most || !anchored[slot] || bodies[slot] == null || crown <= 0f) return StandInChest;
            Vector3 local = Quaternion.Inverse(bodies[slot].rotation) * worldWay;
            float band = Mathf.Clamp(height / crown * Bands - 0.5f, 0f, Bands - 1f);
            int low = Mathf.FloorToInt(band);
            int high = Mathf.Min(Bands - 1, low + 1);
            float blend = band - low;
            var way = new Vector2(local.x, local.z);
            float lower = ReachToward(widths[slot * Bands + low], fronts[slot * Bands + low], backs[slot * Bands + low], way);
            float upper = ReachToward(widths[slot * Bands + high], fronts[slot * Bands + high], backs[slot * Bands + high], way);
            return Mathf.Lerp(lower, upper, blend);
        }

        /// <summary>The body's measured height, metres from its feet to the highest point it carries; 0 for a slot not measured.</summary>
        public static float Height(int slot) => slot >= 0 && slot < Most ? heights[slot] : 0f;

        /// <summary>How far out the body reaches at <paramref name="height"/> metres over its feet, all round (the player's fit).</summary>
        public static float RadiusAt(int slot, float height)
        {
            if (slot < 0 || heights[slot] <= 0f) return StandInChest;
            float band = Mathf.Clamp(height / heights[slot] * Bands - 0.5f, 0f, Bands - 1f);
            int low = Mathf.FloorToInt(band);
            int high = Mathf.Min(Bands - 1, low + 1);
            return Mathf.Lerp(radii[slot * Bands + low], radii[slot * Bands + high], band - low);
        }

        /// <summary>
        /// How much a piece laid on the stand-in at <paramref name="standInHeight"/> metres (its share of the stand-in's
        /// height carried to the real body's) is drawn in toward the real body's upright: its reach over the stand-in's.
        /// The player's surge looks are fitted by this, as they were before the anchors.
        /// </summary>
        public static float Ratio(int slot, float standInHeight, float standInTall)
        {
            if (slot < 0) return 1f;
            float share = Mathf.Clamp01(standInHeight / standInTall);
            return Mathf.Clamp(RadiusAt(slot, share * heights[slot]) / StandInReach(share), LeastRatio, MostRatio);
        }

        // The median reach out from the upright through the feet of the posed body's drawn vertices in each height band
        // (the weapon held and the cloth trailing are fewer than the body's own, so the median is the body).
        private static bool Measure(Transform body, int slot)
        {
            var skin = body.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skin == null || skin.sharedMesh == null) return false;
            if (baked == null) baked = new Mesh { name = "Body Anchors" };
            skin.BakeMesh(baked, true);
            baked.GetVertices(vertices);
            if (vertices.Count == 0) return false;
            var from = skin.transform;
            Vector3 feet = body.position;
            float top = 0f;
            for (int i = 0; i < vertices.Count; i++) top = Mathf.Max(top, from.TransformPoint(vertices[i]).y - feet.y);
            if (top < 0.5f) return false;
            Clear(byBand);
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 at = from.TransformPoint(vertices[i]) - feet;
                int b = Mathf.Clamp((int)(at.y / top * Bands), 0, Bands - 1);
                byBand[b].Add(new Vector2(at.x, at.z).magnitude);
            }
            float last = StandInChest;
            for (int b = 0; b < Bands; b++)
            {
                last = Median(byBand[b], last);
                radii[slot * Bands + b] = last;
            }
            heights[slot] = top;
            return true;
        }

        // A body's anchors into the table: those measured when it was built, or measured now where its mesh can be read
        // (in the editor out of play, as the tests do).
        private static bool Anchor(Transform body, int slot)
        {
            var measured = body.GetComponent<BodyMeasurement>();
            if (measured != null && measured.Valid)
            {
                anchors[slot] = measured.Anchors;
                System.Array.Copy(measured.Widths, 0, widths, slot * Bands, Bands);
                System.Array.Copy(measured.Fronts, 0, fronts, slot * Bands, Bands);
                System.Array.Copy(measured.Backs, 0, backs, slot * Bands, Bands);
                joints[slot] = measured.Joints;
                return true;
            }
            if (!MeasureBody(body, out var found, Band, BandFront, BandBack, out var bones)) return false;
            anchors[slot] = found;
            System.Array.Copy(Band, 0, widths, slot * Bands, Bands);
            System.Array.Copy(BandFront, 0, fronts, slot * Bands, Bands);
            System.Array.Copy(BandBack, 0, backs, slot * Bands, Bands);
            joints[slot] = bones;
            return true;
        }

        private static readonly float[] Band = new float[Bands], BandFront = new float[Bands], BandBack = new float[Bands];

        /// <summary>
        /// The body's anchors from its mesh and bones as they were bound (not as posed now), in the body's own frame:
        /// joint heights from the named bones, the crown from what the head carries, and the torso's reach in each height
        /// band (into <paramref name="sides"/>, <paramref name="aheads"/> and <paramref name="behinds"/>, <see cref="Bands"/>
        /// long) from the vertices of everything but the arms, the weapon and the trailing cloth. False when the body has
        /// no skeleton of the humanoid names, or its mesh cannot be read (in play, where the models are not kept readable).
        /// </summary>
        public static bool MeasureBody(Transform body, out Anchors found, float[] sides, float[] aheads, float[] behinds, out Joints held)
        {
            found = default;
            held = default;
            var skin = body.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skin == null || skin.sharedMesh == null) return false;
            var mesh = skin.sharedMesh;
            if (!mesh.isReadable && Application.isPlaying) return false;
            var bones = skin.bones;
            mesh.GetVertices(vertices);
            mesh.GetBoneWeights(weights);
            mesh.GetBindposes(binds);
            if (vertices.Count == 0 || weights.Count != vertices.Count) return false;
            int hips = -1, chest = -1, neck = -1, head = -1, handL = -1, handR = -1, armL = -1, armR = -1;
            for (int b = 0; b < bones.Length; b++)
            {
                if (bones[b] == null) continue;
                string name = bones[b].name;
                if (name == "hips") hips = b;
                else if (name == "chest") chest = b;
                else if (name == "neck") neck = b;
                else if (name == "head") head = b;
                else if (name == "hand.L") handL = b;
                else if (name == "hand.R") handR = b;
                else if (name == "upperarm.L") armL = b;
                else if (name == "upperarm.R") armR = b;
            }
            if (hips < 0 || chest < 0 || neck < 0 || head < 0 || hips >= binds.Count || chest >= binds.Count || neck >= binds.Count || head >= binds.Count) return false;
            // The bound mesh's space carried into the body's own frame.
            Matrix4x4 toBody = body.worldToLocalMatrix * skin.transform.localToWorldMatrix;
            float Joint(int bone) => toBody.MultiplyPoint3x4(binds[bone].inverse.GetColumn(3)).y;
            float crown = 0f;
            for (int i = 0; i < vertices.Count; i++)
            {
                var weight = weights[i];
                if (weight.boneIndex0 != head || weight.weight0 < 0.5f) continue;
                crown = Mathf.Max(crown, toBody.MultiplyPoint3x4(vertices[i]).y);
            }
            float headJoint = Joint(head), neckJoint = Joint(neck);
            if (crown <= headJoint) crown = headJoint + 2f * (headJoint - neckJoint);
            found = new Anchors
            {
                Waist = Joint(hips),
                Chest = Mathf.Lerp(Joint(chest), neckJoint, 0.5f),
                // The middle of the face: as far over the skull's joint as the neck runs under it (a helmet's crest or a
                // hat raises the crown, not the face).
                Head = Mathf.Min(crown, headJoint + (headJoint - neckJoint)),
                Crown = crown,
            };
            // The torso is no wider than the shoulders it hangs from: drums, sleeves or a ring of wings bound to the chest
            // (Narukami's) would otherwise stand every piece at its side out in the air.
            float shoulders = float.MaxValue;
            if (armL >= 0 && armR >= 0 && armL < binds.Count && armR < binds.Count)
                shoulders = 0.5f * Mathf.Abs(toBody.MultiplyPoint3x4(binds[armL].inverse.GetColumn(3)).x - toBody.MultiplyPoint3x4(binds[armR].inverse.GetColumn(3)).x);
            if (found.Crown < 0.5f || found.Waist <= 0f || found.Chest <= found.Waist || found.Head <= found.Chest) return false;

            // The torso's reach in each height band: to the sides, ahead and behind, from all but the arms and what they hold.
            Clear(sideBand);
            Clear(frontBand);
            Clear(backBand);
            for (int i = 0; i < vertices.Count; i++)
            {
                var weight = weights[i];
                int bone = weight.boneIndex0;
                if (bone < 0 || bone >= bones.Length || bones[bone] == null || !IsTrunk(bones[bone].name)) continue;
                Vector3 at = toBody.MultiplyPoint3x4(vertices[i]);
                int b = Mathf.Clamp((int)(at.y / crown * Bands), 0, Bands - 1);
                sideBand[b].Add(Mathf.Abs(at.x));
                if (at.z >= 0f) frontBand[b].Add(at.z);
                else backBand[b].Add(-at.z);
            }
            float side = StandIn.Width, ahead = StandIn.Front, behind = StandIn.Back;
            for (int b = 0; b < Bands; b++)
            {
                side = Mathf.Min(Outer(sideBand[b], side), shoulders);
                ahead = Outer(frontBand[b], ahead);
                behind = Outer(backBand[b], behind);
                sides[b] = side;
                aheads[b] = ahead;
                behinds[b] = behind;
            }
            int chestBand = Mathf.Clamp((int)(found.Chest / crown * Bands), 0, Bands - 1);
            found.Width = sides[chestBand];
            found.Front = aheads[chestBand];
            found.Back = behinds[chestBand];
            held = new Joints
            {
                Hips = bones[hips], Chest = bones[chest], Neck = bones[neck], Head = bones[head],
                HandLeft = handL >= 0 ? bones[handL] : null, HandRight = handR >= 0 ? bones[handR] : null,
            };
            return true;
        }

        // The body itself: not an arm or what it holds, not the cloth, hair and bands that trail.
        private static bool IsTrunk(string bone) =>
            !(bone.StartsWith("upperarm") || bone.StartsWith("forearm") || bone.StartsWith("hand") || bone.StartsWith("weapon")
              || bone.StartsWith("scarf") || bone.StartsWith("band_") || bone.StartsWith("pony") || bone.StartsWith("shoulder"));

        private static void Clear(List<float>[] lists)
        {
            for (int b = 0; b < lists.Length; b++)
            {
                if (lists[b] == null) lists[b] = new List<float>(256);
                lists[b].Clear();
            }
        }

        private static float Median(List<float> band, float otherwise)
        {
            if (band.Count == 0) return otherwise;
            band.Sort();
            return band[band.Count / 2];
        }

        // Near the surface: four in five of the band's points lie within it (a few points of a buckle or a crest stand past).
        private static float Outer(List<float> band, float otherwise)
        {
            if (band.Count < 4) return otherwise;
            band.Sort();
            return band[Mathf.Min(band.Count - 1, (int)(band.Count * 0.8f))];
        }
    }
}

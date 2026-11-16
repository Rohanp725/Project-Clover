using System.Collections.Generic;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Runs the spell forms that take time. A form begins a run here when it is cast; every frame each run in progress
    /// is handed back to its form, on the game's own clock, so a stop of time, the slow of a perfect dodge and a pause
    /// all hold a spell exactly as they hold everything else. A fixed number of runs is kept ready, each with its own
    /// lists of the solids, marks and barriers it holds, which are put away for it when it ends. Every run ends when
    /// the player falls. The scene has one of these, made by the builder; with none, forms that last do nothing.
    /// </summary>
    public class SpellWorks : MonoBehaviour
    {
        [SerializeField] private Afterimage image;

        public int Capacity = 24;
        public int BarrierCount = 16;

        public const int SolidsEach = 16;
        public const int MarksEach = 6;
        public const int BarriersEach = 14;
        public const int NotesEach = 16;

        /// <summary>How many runs are in progress, and how many have ever begun, so probes can see them without the screen.</summary>
        public static int Running
        {
            get
            {
                if (instance == null) return 0;
                int count = 0;
                for (int i = 0; i < instance.runs.Length; i++)
                    if (instance.runs[i].Active) count++;
                return count;
            }
        }
        public static int Begun { get; private set; }

        private static SpellWorks instance;
        private static SpellRun spare;

        private SpellRun[] runs;
        private int[][] solids, marks, barriers;
        private Vector3[][] points;
        private float[][] times;
        private Component[][] things;
        private HashSet<IDamageable>[] once;
        private bool[] screens;
        private Vector3[] screenA, screenB;
        private BoxCollider[] walls;
        private bool[] wallUsed;
        private int nextToken = 1;
        private SpellClone clone;

        private PlayerController gesturer;
        private int gestureToken, gesturePose;
        private float gestureAge, gestureSeconds, gestureFrom, gestureTo;

        // Domain reload is off: the runs of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            spare = default;
            Begun = 0;
        }

        /// <summary>The builder hands over the player's afterimage, whose body a copy of the player is made from.</summary>
        public void Configure(Afterimage playerImage) => image = playerImage;

        private void Awake()
        {
            runs = new SpellRun[Capacity];
            solids = new int[Capacity][];
            marks = new int[Capacity][];
            barriers = new int[Capacity][];
            points = new Vector3[Capacity][];
            times = new float[Capacity][];
            things = new Component[Capacity][];
            once = new HashSet<IDamageable>[Capacity];
            screens = new bool[Capacity];
            screenA = new Vector3[Capacity];
            screenB = new Vector3[Capacity];
            for (int i = 0; i < Capacity; i++)
            {
                solids[i] = new int[SolidsEach];
                marks[i] = new int[MarksEach];
                barriers[i] = new int[BarriersEach];
                points[i] = new Vector3[NotesEach];
                times[i] = new float[NotesEach];
                things[i] = new Component[NotesEach];
                once[i] = new HashSet<IDamageable>();
                Clear(i);
            }
            walls = new BoxCollider[BarrierCount];
            wallUsed = new bool[BarrierCount];
            for (int i = 0; i < BarrierCount; i++)
            {
                var wall = new GameObject("Spell Barrier");
                wall.transform.SetParent(transform, false);
                walls[i] = wall.AddComponent<BoxCollider>();
                walls[i].enabled = false;
            }
            clone = gameObject.AddComponent<SpellClone>();
            clone.Configure(image);
        }

        private void Clear(int slot)
        {
            for (int k = 0; k < SolidsEach; k++) solids[slot][k] = -1;
            for (int k = 0; k < MarksEach; k++) marks[slot][k] = -1;
            for (int k = 0; k < BarriersEach; k++) barriers[slot][k] = -1;
            for (int k = 0; k < NotesEach; k++)
            {
                points[slot][k] = Vector3.zero;
                times[slot][k] = 0f;
                things[slot][k] = null;
            }
            once[slot].Clear();
            screens[slot] = false;
        }

        private void OnEnable() => instance = this;

        private void OnDisable()
        {
            if (runs != null)
                for (int i = 0; i < runs.Length; i++)
                    if (runs[i].Active) Finish(i);
            StopGesture();
            if (instance == this) instance = null;
        }

        // ---- What a form asks for.

        /// <summary>
        /// Begins a run of a form. The form fills in what comes back. When no run can be had (no works in the scene)
        /// <paramref name="begun"/> is false and what comes back is a spare that is thrown away.
        /// </summary>
        public static ref SpellRun Begin(RunningForm form, in SpellCast cast, out bool begun)
        {
            begun = false;
            if (instance == null || cast.Player == null)
            {
                spare = default;
                return ref spare;
            }
            var works = instance;
            int slot = -1;
            for (int i = 0; i < works.runs.Length && slot < 0; i++)
                if (!works.runs[i].Active) slot = i;
            if (slot < 0)
            {
                // Every place in use: the oldest run that does not hold the body gives way.
                float oldest = -1f;
                for (int i = 0; i < works.runs.Length; i++)
                    if (!works.runs[i].HoldsBody && works.runs[i].Age > oldest)
                    {
                        oldest = works.runs[i].Age;
                        slot = i;
                    }
                if (slot < 0)
                {
                    spare = default;
                    return ref spare;
                }
                works.Finish(slot);
            }
            works.Clear(slot);
            works.runs[slot] = new SpellRun { Active = true, Slot = slot, Token = works.nextToken++, Form = form, Cast = cast };
            if (works.nextToken > 1000000) works.nextToken = 1;
            Begun++;
            begun = true;
            return ref works.runs[slot];
        }

        /// <summary>Ends every run of a form in progress: for forms of which the player has one at a time.</summary>
        public static void EndRunsOf(RunningForm form)
        {
            if (instance == null) return;
            for (int i = 0; i < instance.runs.Length; i++)
                if (instance.runs[i].Active && instance.runs[i].Form == form) instance.Finish(i);
        }

        /// <summary>How many runs of the form of this name are in progress.</summary>
        public static int RunningOf(string id)
        {
            if (instance == null) return 0;
            int count = 0;
            for (int i = 0; i < instance.runs.Length; i++)
                if (instance.runs[i].Active && instance.runs[i].Form.Id == id) count++;
            return count;
        }

        /// <summary>The run's own lists. Handles of solids and marks kept in the first two are put away when the run ends; -1 is an empty place.</summary>
        public static int[] Solids(in SpellRun run) => instance.solids[run.Slot];
        public static int[] Marks(in SpellRun run) => instance.marks[run.Slot];
        public static Vector3[] Points(in SpellRun run) => instance.points[run.Slot];
        public static float[] Times(in SpellRun run) => instance.times[run.Slot];
        public static Component[] Things(in SpellRun run) => instance.things[run.Slot];
        /// <summary>A list of who this run has struck, for strikes that touch each enemy once.</summary>
        public static HashSet<IDamageable> Once(in SpellRun run) => instance.once[run.Slot];

        /// <summary>The copy of the player a form may show; null when the scene has none.</summary>
        public static SpellClone Clone => instance != null ? instance.clone : null;

        /// <summary>Leaves a shape of the player's body where they stand now, in a light, to fade over some seconds.</summary>
        public static void Ghost(float seconds, Color light)
        {
            if (instance != null && instance.image != null && instance.image.isActiveAndEnabled) instance.image.Leave(seconds, light);
        }

        /// <summary>Gives the player's body to a run. False when it cannot be taken.</summary>
        public static bool TakeBody(ref SpellRun run, bool untouchable, bool cancellable)
        {
            if (run.Cast.Player == null || !run.Cast.Player.BeginFormHold(run.Token, untouchable, cancellable)) return false;
            run.HoldsBody = true;
            return true;
        }

        public static void GiveBody(ref SpellRun run)
        {
            if (!run.HoldsBody) return;
            run.HoldsBody = false;
            if (run.Cast.Player != null) run.Cast.Player.EndFormHold(run.Token);
        }

        /// <summary>
        /// Stands a barrier nothing walks through: a box of this size at this place. It is taken down when the run ends.
        /// False when no more can be had.
        /// </summary>
        public static bool Barrier(in SpellRun run, Vector3 centre, Quaternion rotation, Vector3 size)
        {
            if (instance == null) return false;
            var mine = instance.barriers[run.Slot];
            int place = -1;
            for (int k = 0; k < mine.Length && place < 0; k++)
                if (mine[k] < 0) place = k;
            if (place < 0) return false;
            for (int i = 0; i < instance.walls.Length; i++)
            {
                if (instance.wallUsed[i]) continue;
                instance.wallUsed[i] = true;
                mine[place] = i;
                var wall = instance.walls[i];
                wall.transform.SetPositionAndRotation(centre, rotation);
                wall.size = size;
                wall.enabled = true;
                return true;
            }
            return false;
        }

        /// <summary>Says that a wall stands between two points on the ground for as long as the run lasts: blows from its far side do not reach the player.</summary>
        public static void Screen(in SpellRun run, Vector3 from, Vector3 to)
        {
            if (instance == null) return;
            instance.screens[run.Slot] = true;
            instance.screenA[run.Slot] = from;
            instance.screenB[run.Slot] = to;
        }

        /// <summary>True when a spell's wall stands between where a blow comes from and where it is going.</summary>
        public static bool Screens(Vector3 from, Vector3 to)
        {
            if (instance == null) return false;
            for (int i = 0; i < instance.screens.Length; i++)
            {
                if (!instance.screens[i] || !instance.runs[i].Active) continue;
                if (FormRules.Crosses(from, to, instance.screenA[i], instance.screenB[i])) return true;
            }
            return false;
        }

        /// <summary>
        /// The body's part in a cast that does not otherwise hold it: a pose played through over some seconds, the
        /// player rooted while it lasts. The dodge breaks it.
        /// </summary>
        public static void Gesture(in SpellCast cast, int pose, float seconds, float from = 0f, float to = 1f, bool swordOut = false)
        {
            if (instance == null || cast.Player == null || seconds <= 0f) return;
            instance.StopGesture();
            int token = -(instance.nextToken++);
            if (!cast.Player.BeginFormHold(token, false, true)) return;
            instance.gesturer = cast.Player;
            instance.gestureToken = token;
            instance.gesturePose = pose;
            instance.gestureAge = 0f;
            instance.gestureSeconds = seconds;
            instance.gestureFrom = from;
            instance.gestureTo = to;
            cast.Player.FormPose = pose;
            cast.Player.FormPoseProgress = from;
            cast.Player.FormSwordOut = swordOut;
        }

        private void StopGesture()
        {
            if (gestureToken == 0) return;
            if (gesturer != null) gesturer.EndFormHold(gestureToken);
            gestureToken = 0;
            gesturer = null;
        }

        // ---- What the weapon tells the forms.

        /// <summary>The weapon was swung from a place, facing a way.</summary>
        public static void Swing(AttackStep step, Vector3 origin, Vector3 forward)
        {
            if (instance == null) return;
            for (int i = 0; i < instance.runs.Length; i++)
                if (instance.runs[i].Active) instance.runs[i].Form.Swung(ref instance.runs[i], step, origin, forward);
        }

        /// <summary>A blow of the weapon landed on something.</summary>
        public static void Blow(AttackStep step, Component target, float damage)
        {
            if (instance == null) return;
            for (int i = 0; i < instance.runs.Length; i++)
                if (instance.runs[i].Active) instance.runs[i].Form.Struck(ref instance.runs[i], step, target, damage);
        }

        // ---- The runs themselves.

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;
            if (gestureToken != 0)
            {
                if (gesturer == null || gesturer.FormHold != gestureToken) gestureToken = 0;
                else
                {
                    gestureAge += deltaTime;
                    gesturer.FormPose = gesturePose;
                    gesturer.FormPoseProgress = Mathf.Lerp(gestureFrom, gestureTo, Mathf.Clamp01(gestureAge / gestureSeconds));
                    if (gestureAge >= gestureSeconds) StopGesture();
                }
            }
            for (int i = 0; i < runs.Length; i++)
            {
                if (!runs[i].Active) continue;
                ref var run = ref runs[i];
                var player = run.Cast.Player;
                // The caster gone or fallen: everything of theirs goes with them.
                if (player == null || (run.Cast.Defence != null && run.Cast.Defence.IsDead))
                {
                    Finish(i);
                    continue;
                }
                if (run.HoldsBody && player.FormHold != run.Token)
                {
                    run.HoldsBody = false;
                    if (!run.Form.BodyLost(ref run))
                    {
                        Finish(i);
                        continue;
                    }
                }
                run.Age += deltaTime;
                if (!run.Form.Tick(ref run, deltaTime)) Finish(i);
            }
        }

        private void Finish(int slot)
        {
            ref var run = ref runs[slot];
            if (!run.Active) return;
            run.Form.End(ref run);
            GiveBody(ref run);
            for (int k = 0; k < SolidsEach; k++) FormFx.Release(solids[slot][k], 0.12f);
            for (int k = 0; k < MarksEach; k++) FormFx.ReleaseMark(marks[slot][k], 0.15f);
            for (int k = 0; k < BarriersEach; k++)
            {
                int wall = barriers[slot][k];
                if (wall < 0) continue;
                walls[wall].enabled = false;
                wallUsed[wall] = false;
            }
            Clear(slot);
            run.Active = false;
            run.Form = null;
            run.Cast = default;
            run.Victim = null;
        }
    }
}

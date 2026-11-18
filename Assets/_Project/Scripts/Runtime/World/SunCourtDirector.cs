using System.Collections.Generic;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// Runs the fight in the sun-court (fire_land.md, section 6, and WORLD.md, "Every god transforms"): turns the
    /// obelisk's shadow and the court's light with it; begins the fight when the gold line is crossed (the king speaks
    /// first, click-through, and a wall of heat closes behind); shows the thrown spear standing in the floor until he
    /// calls it back. Beaten, the man does not die: he gets up, lifts the spear, the sun comes down it into him and he
    /// is gone into the disc the scarab rises under. When the form falls the disc cracks and he lies in the obelisk's
    /// shadow, sane for a breath; he says his one line, and his fire is taken from him there. Then the sun moves for the
    /// first time in ages and sets in the west, and the way out is the low sun's beam through the west gate. A fall
    /// before the end puts the god back as he was, whole, at the far side of the court.
    /// </summary>
    public class SunCourtDirector : MonoBehaviour
    {
        [SerializeField] private Enemy boss;
        [SerializeField] private PlayerDefence player;
        [SerializeField] private HitStop hitStop;
        [SerializeField] private GodlyForm form;
        [SerializeField] private BossHud bossHud;
        [SerializeField] private GodReward hisFire;
        [SerializeField] private SceneDoor wayOut;
        [SerializeField] private Collider heatWall;
        [SerializeField] private Renderer heatGlow;
        [SerializeField] private Transform keyLight;
        [SerializeField] private Transform shade;
        [SerializeField] private Transform standingSpear;
        [SerializeField] private Transform weaponBone;
        [SerializeField] private LineRenderer ray;
        [SerializeField] private SpearRay rayLook;
        [SerializeField] private TrailRenderer spearTrail;
        [SerializeField] private UnityEngine.Rendering.Volume sunsetLook;
        [SerializeField] private Transform westGate;
        [SerializeField] private Collider westGateWall;
        [SerializeField] private Renderer sunRoad;
        [SerializeField] private Renderer lowSun;
        /// <summary>Seconds the thrown spear takes to fly its lane, point first, before it stands.</summary>
        public float SpearFlight = 0.2f;

        /// <summary>The gold line: the fight begins when the player is north of it.</summary>
        public float LineZ = -20f;
        /// <summary>Seconds the beaten man takes to get up off his knee and lift the spear to the sky before the sun comes down it.</summary>
        public float RisingDelay = 1.1f;
        /// <summary>Seconds after the form falls (the disc already cracked and he lying there) before he speaks.</summary>
        public float RegretDelay = 0.4f;
        public int Embers = 5;
        public string ManName = "Khepharu,  God of Fire";
        public string GodlyName = "Khepharu,  the Sun-Bearer";
        public string Speaker = "Khepharu";
        [TextArea] public string[] Greeting = new string[0];
        /// <summary>His one line, sane for a moment on the floor of the court (WORLD.md: the regret lines stay as written).</summary>
        [TextArea] public string[] Regret = { "I promised them gold. I gave them fire." };
        /// <summary>The sponsor's words after the fire is taken. (The name is from when the barque was the way out.)</summary>
        [TextArea] public string[] Barque = new string[0];
        public string BarqueSpeaker = "";
        public string HalfCallout = "It is always noon.";
        public string RisingCallout = "The sun comes down";
        public string RoarCallout = "Khepharu, the Sun-Bearer";
        public string FuryCallout = "Noon breaks";
        /// <summary>The man's two clips that are only his: lifting the spear as the sun comes into him, and lying fallen.</summary>
        public string RisingClip = "sunking_transform", FallenClip = "sunking_fallen";
        public float FallenBreath = 3.2f;
        /// <summary>How he burns from inside as the sun comes down the spear into him, at its brightest.</summary>
        [ColorUsage(true, true)] public Color PouredGlow = new Color(5f, 3.6f, 1.5f, 1f);
        /// <summary>How the camera stands for the transformation: far enough off to hold him, the sky's thread and whoever watches.</summary>
        public float ShowDistance = 1.5f, ShowPitch = -8f;
        /// <summary>How far the sun leans from noon once he lies fallen, so the obelisk's real shadow is laid across him.</summary>
        public float FallenPitch = 42f;
        /// <summary>How the camera stands back for the sunset, looking at this point of the court, and for the way out after.</summary>
        public float SunsetDistance = 2.3f, SunsetViewPitch = -6f;
        public Vector3 SunsetLookAt = new Vector3(-1f, 0f, -1f);
        /// <summary>Where, past the gate's mark, the camera holds in the picture once the way is open: toward the sun.</summary>
        public Vector3 WestBeyond = new Vector3(-5f, 0f, 0f);
        /// <summary>Metres of him and the spear lifted over him that the camera must keep in the picture.</summary>
        public float ManHeight = 4f;
        /// <summary>Seconds the camera takes to settle on him once he is beaten (its usual is slower).</summary>
        public float TransformFraming = 0.35f;
        /// <summary>The sun's last light: its colour and strength, the air's, the sky's.</summary>
        public Color SunsetKey = new Color(1f, 0.58f, 0.3f);
        public float SunsetIntensity = 2.3f;
        public Color SunsetAmbient = new Color(0.4f, 0.28f, 0.32f);
        public Color SunsetFog = new Color(0.5f, 0.32f, 0.26f);
        public Color SunsetSky = new Color(0.36f, 0.2f, 0.18f);
        [ColorUsage(true, true)] public Color RoadColour = new Color(3.2f, 1.6f, 0.45f, 1f);
        [ColorUsage(true, true)] public Color LowSunColour = new Color(6f, 2.6f, 0.7f, 1f);
        /// <summary>Metres the west gate sinks into the floor as it opens.</summary>
        public float GateDrop = 3.4f;
        public Sunset Sun = new Sunset();

        public ShadowHand Hand { get; private set; }
        public bool Begun { get; private set; }
        public bool Felled { get; private set; }
        public bool HasChosen { get; private set; }
        public bool SecondFormCalled { get; private set; }
        public bool SpearStands { get; private set; }
        public int Retries { get; private set; }
        /// <summary>True from the man's beating until he is gone into the sun.</summary>
        public bool Transforming => transformFor >= 0f;
        /// <summary>The man is lying in the court where the disc cracked.</summary>
        public bool ManLaid { get; private set; }
        /// <summary>True while his body is not drawn: from his going into the sun until the disc cracks.</summary>
        public bool ManGone { get; private set; }
        public bool RegretSaid { get; private set; }
        public bool WayOpen { get; private set; }

        /// <summary>How the camera stands for the duel with the man: a little further off and steeper, so the obelisk hides less of the court behind it.</summary>
        public float DuelDistance = 1.3f, DuelPitch = -8f;

        private static readonly int ColourId = Shader.PropertyToID("_Color");

        private float lightPitch = 78f, transformFor = -1f, fallenFor = -1f, untilRegret = -1f;
        private Vector3 lastBlade, standAt, flyFrom, flyAlong, gateShut;
        private Quaternion standTurn;
        private float flightLeft;
        private float untilSmoke, untilGlow;
        private bool spoken, framed, rayStruck, sponsorDone;
        private TopDownCamera view;
        private OcclusionFader fader;
        private float usualFraming = -1f;
        private Camera eye;
        private Light keySource;
        private PlayerUpgrades upgrades;
        private readonly List<Renderer> hidden = new List<Renderer>();
        private MaterialPropertyBlock block;
        private Color noonKey, noonAmbient, noonFog, noonSky;
        private float noonIntensity;

        public void Configure(Enemy bossToWatch, PlayerDefence playerToServe, HitStop stop, GodlyForm godlyForm, BossHud hud, GodReward reward, SceneDoor door)
        {
            boss = bossToWatch;
            player = playerToServe;
            hitStop = stop;
            form = godlyForm;
            bossHud = hud;
            hisFire = reward;
            wayOut = door;
        }

        /// <summary>The things of the court it moves: the wall of heat at the gold line, the key light, the drawn shadow, and the thrown spear.</summary>
        public void ConfigureCourt(Collider wall, Renderer wallGlow, Transform light, Transform shadeStrip, Transform spearInFloor, Transform bossWeapon, LineRenderer rayOfLight)
        {
            heatWall = wall;
            heatGlow = wallGlow;
            keyLight = light;
            shade = shadeStrip;
            standingSpear = spearInFloor;
            weaponBone = bossWeapon;
            ray = rayOfLight;
        }

        /// <summary>The ray drawn as a beam of light (fire_boss_visuals.md 2.4); without it the plain line is drawn as before.</summary>
        public void ConfigureRay(SpearRay look) => rayLook = look;

        /// <summary>The fire the thrown spear trails in its flight.</summary>
        public void ConfigureThrow(TrailRenderer trail) => spearTrail = trail;

        /// <summary>What the sunset moves: the warm grade, the west gate and the wall it stands in, and the low sun's beam on the floor.</summary>
        public void ConfigureSunset(UnityEngine.Rendering.Volume look, Transform gate, Collider gateWall, Renderer road, Renderer sunOnTheHorizon)
        {
            sunsetLook = look;
            westGate = gate;
            westGateWall = gateWall;
            sunRoad = road;
            lowSun = sunOnTheHorizon;
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            Hand = new ShadowHand();
            Hand.Reset(ShadowHand.AgainstTheMan);
            // The same shade the sun-blind hunt by: whoever asks whether a point is lit is told of the hand.
            Sunlight.SetOpenSky(true);
            Sunlight.Hand = Hand;
            if (keyLight != null)
            {
                lightPitch = keyLight.eulerAngles.x;
                keySource = keyLight.GetComponent<Light>();
            }
            if (keySource != null)
            {
                noonKey = keySource.color;
                noonIntensity = keySource.intensity;
            }
            noonAmbient = RenderSettings.ambientLight;
            noonFog = RenderSettings.fogColor;
            if (westGate != null) gateShut = westGate.localPosition;
            // He changes in front of the player and falls back as a man: his body is never burnt away.
            if (boss != null && form != null) boss.KeepsBody = true;
        }

        private void Start()
        {
            if (hisFire != null) hisFire.gameObject.SetActive(false);
            ShowWayOut(false);
            SetWall(false);
            if (sunsetLook != null) sunsetLook.weight = 0f;
            if (sunRoad != null) sunRoad.enabled = false;
            if (lowSun != null) lowSun.enabled = false;
            eye = Camera.main;
            view = eye != null ? eye.GetComponent<TopDownCamera>() : null;
            fader = eye != null ? eye.GetComponent<OcclusionFader>() : null;
            if (eye != null) noonSky = eye.backgroundColor;
            ShowSpear(false);
            if (player != null)
            {
                player.Revived += OnRevived;
                upgrades = player.GetComponent<PlayerUpgrades>();
            }
            if (form != null)
            {
                form.Died += OnFormDied;
                form.Roared += OnFormRoared;
                form.Enraged += OnFormEnraged;
            }
            if (boss != null)
            {
                boss.Died += OnBossDied;
                boss.PhaseChanged += OnPhaseChanged;
            }
            TurnCourt();
        }

        private void OnDestroy()
        {
            if (Sunlight.Hand == Hand) Sunlight.Hand = null;
            if (player != null) player.Revived -= OnRevived;
            if (form != null)
            {
                form.Died -= OnFormDied;
                form.Roared -= OnFormRoared;
                form.Enraged -= OnFormEnraged;
            }
            if (boss != null)
            {
                boss.Died -= OnBossDied;
                boss.PhaseChanged -= OnPhaseChanged;
            }
        }

        // The mark of the way out is not on the floor until the way is open.
        private void ShowWayOut(bool open)
        {
            if (wayOut == null) return;
            wayOut.Open = open;
            for (int i = 0; i < wayOut.transform.childCount; i++) wayOut.transform.GetChild(i).gameObject.SetActive(open);
        }

        private void SetWall(bool closed)
        {
            if (heatWall != null) heatWall.enabled = closed;
            if (heatGlow != null) heatGlow.enabled = closed;
        }

        private void OnPhaseChanged(Enemy who)
        {
            if (player != null) player.Callout(HalfCallout);
            ScreenFlash.Flash(new Color(1f, 0.85f, 0.5f, 0.35f), 0.09f);
        }

        private void OnBossDied(Enemy who)
        {
            if (Felled || SecondFormCalled) return;
            ShowSpear(false);
            if (rayLook != null) rayLook.Clear();
            if (form == null)
            {
                Fell();
                return;
            }
            // Beaten, he does not fall. He gets up and lifts the spear, and the sun he carried comes down it.
            SecondFormCalled = true;
            if (hitStop != null) hitStop.Slow(1.4f, 0.25f);
            ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.5f), 0.09f);
            if (form is SunBearer bearer) bearer.RiseFrom = who.transform.position;
            if (player != null)
            {
                // A breath before the worst of it: the flasks are not given back, but the wounds of the duel are eased.
                player.Health.Heal(player.Health.Maximum * 0.35f);
            }
            transformFor = 0f;
            boss.HoldPose(RisingClip, 0f);
            // The camera goes to him quicker than to an ordinary framing: he is only on his knee for a moment.
            if (view != null && usualFraming < 0f)
            {
                usualFraming = view.FramingTime;
                view.FramingTime = TransformFraming;
            }
        }

        private void OnFormRoared(GodlyForm who)
        {
            if (player != null) player.Callout(RoarCallout);
        }

        private void OnFormEnraged(GodlyForm who)
        {
            if (player != null) player.Callout(FuryCallout);
        }

        private void OnFormDied(GodlyForm who) => Fell();

        private void Fell()
        {
            if (Felled) return;
            Felled = true;
            RunState.GodsFelled++;
            SetWall(false);
            if (hitStop != null) hitStop.Slow(1.8f, 0.2f);
            ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.55f), 0.1f);
            if (player != null) player.Callout("A god falls");
            // The five Shards of a god are paid when the form falls, not the man.
            if (upgrades != null) upgrades.Earn(Embers);
            // The disc has cracked and he lies in the shadow (or, with no form, where he fell); a breath, then he speaks once.
            if (!ManLaid && boss != null) LayMan(form is SunBearer bearer && bearer.SunCracked ? bearer.CrackedAt : boss.transform.position);
            untilRegret = RegretDelay;
        }

        // The player fell before the end: the god is whole again and waits at the far side, the shadow points north,
        // the way back out is open, and the fight begins again when the line is crossed.
        private void OnRevived()
        {
            if (Felled) return;
            Retries++;
            Begun = false;
            SecondFormCalled = false;
            transformFor = -1f;
            fallenFor = -1f;
            ManLaid = false;
            if (view != null && usualFraming >= 0f) view.FramingTime = usualFraming;
            usualFraming = -1f;
            if (fader != null) fader.WatchAlso = false;
            SetWall(false);
            ShowSpear(false);
            if (rayLook != null) rayLook.Clear();
            if (form != null) form.Retire();
            ShowMan();
            if (boss != null && boss.IsDead) boss.Restore();
            Hand.Reset(ShadowHand.AgainstTheMan);
            lightPitch = Sun.NoonPitch;
            if (bossHud != null) bossHud.Show(boss, ManName);
        }

        private void Update()
        {
            // The shadow stops when the man lies in it; until then it never does, and after his fire is taken the sunset moves it.
            if (!Felled && !ManLaid) Hand.Tick(Time.deltaTime * WorldClock.EnemyScale);
            AdvanceSunset(Time.deltaTime);
            TurnCourt();
            if (!Begun && !Felled && player != null && !player.IsDead && player.transform.position.z > LineZ + 0.6f)
            {
                Begun = true;
                SetWall(true);
                if (!spoken)
                {
                    spoken = true;
                    ReadingBox.Show(Speaker, Greeting);
                }
            }
            WatchSpear();
            AdvanceTransformation(Time.deltaTime);
            if (form is SunBearer bearer && bearer.SunCracked && !ManLaid) LayMan(bearer.CrackedAt);
            AdvanceFallen(Time.deltaTime);
            Frame();
        }

        // ------------------------------------------------------------------ the man becomes the sun

        // He gets up and lifts the spear; the thread of light stands out of it, the sun comes down it into him and he burns
        // gold and then white from inside; in the flash as it arrives he is gone, and the scarab rises under the disc.
        private void AdvanceTransformation(float deltaTime)
        {
            if (transformFor < 0f || boss == null) return;
            transformFor += deltaTime;
            var bearer = form as SunBearer;
            bool entering = form != null && form.HasRisen && form.Doing == "Entering";
            float rose = RisingDelay > 0f ? Mathf.Clamp01(transformFor / RisingDelay) : 1f;
            float poured = bearer != null && entering ? Mathf.Clamp01(form.DoingFor / Mathf.Max(0.01f, bearer.SunLandsAt)) : 0f;
            boss.HoldPose(RisingClip, 0.48f * rose + 0.52f * poured);
            float lit = 0f;
            if (bearer != null && entering)
            {
                float from = bearer.SunDownAt * 0.5f;
                lit = Mathf.Clamp01((form.DoingFor - from) / Mathf.Max(0.01f, bearer.SunLandsAt - from));
            }
            boss.Shine(PouredGlow * (lit * lit));
            if (lit > 0.2f && Every(ref untilGlow, 0.09f, deltaTime))
                Vfx.Play(VfxKind.Embers, boss.transform.position + Vector3.up * Random.Range(0.4f, 2.2f), Vector3.up, 0.6f + lit);
            // He turns to face whoever beat him.
            if (player != null)
            {
                Vector3 toPlayer = player.transform.position - boss.transform.position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude > 0.01f)
                    boss.transform.rotation = Quaternion.RotateTowards(boss.transform.rotation, Quaternion.LookRotation(toPlayer), 200f * deltaTime);
            }
            // The obelisk is thinned for him as well as for the player while he gets up: from the south it can stand in front of him.
            if (fader != null && form != null && !form.HasRisen)
            {
                fader.WatchAlso = true;
                fader.AlsoAt = boss.transform.position + Vector3.up * 1.2f;
                fader.AndAt = boss.transform.position + Vector3.up * 3.2f;
            }
            if (form != null && !form.HasRisen && transformFor >= RisingDelay)
            {
                form.Rise();
                if (bossHud != null) bossHud.Show(form, GodlyName);
                if (player != null) player.Callout(RisingCallout);
                // The sun has only begun to come down: he stays until it arrives.
                return;
            }
            if (form != null && form.HasRisen && (!entering || bearer == null || form.DoingFor >= bearer.SunLandsAt))
            {
                if (view != null && usualFraming >= 0f) view.FramingTime = usualFraming;
                usualFraming = -1f;
                HideMan();
                boss.Shine(Color.black);
                transformFor = -1f;
            }
        }

        private static bool Every(ref float clock, float interval, float passed)
        {
            clock -= passed;
            if (clock > 0f) return false;
            clock = interval;
            return true;
        }

        // His body, drawn or not, as it was drawn when it went.
        private void HideMan()
        {
            if (boss == null || ManGone) return;
            ManGone = true;
            hidden.Clear();
            foreach (var shown in boss.GetComponentsInChildren<Renderer>())
            {
                if (!shown.enabled) continue;
                hidden.Add(shown);
                shown.enabled = false;
            }
        }

        private void ShowMan()
        {
            if (!ManGone) return;
            ManGone = false;
            foreach (var shown in hidden)
                if (shown != null) shown.enabled = true;
            hidden.Clear();
        }

        /// <summary>
        /// Which way a man lying across the obelisk's shadow faces: his length square to the way the shadow points, so
        /// it falls across him (within <paramref name="mostFromLevel"/> degrees of lying east and west, so the camera
        /// sees the length of him), and his feet toward whoever watches, so his lifted head looks at them.
        /// </summary>
        public static Quaternion LyingAcross(Vector3 shadow, Vector3 toWatcher, float mostFromLevel = 35f)
        {
            shadow.y = 0f;
            if (shadow.sqrMagnitude < 0.0001f) shadow = Vector3.forward;
            Vector3 length = Vector3.Cross(Vector3.up, shadow.normalized);
            // The camera looks north: a body lying north and south is seen end on and reads as sitting up. He is turned
            // no further than this from lying east and west, as near square to the shadow as that allows.
            float degrees = Mathf.Atan2(length.z, length.x) * Mathf.Rad2Deg;
            if (degrees > 90f) degrees -= 180f;
            else if (degrees < -90f) degrees += 180f;
            degrees = Mathf.Clamp(degrees, -mostFromLevel, mostFromLevel) * Mathf.Deg2Rad;
            length = new Vector3(Mathf.Cos(degrees), 0f, Mathf.Sin(degrees));
            toWatcher.y = 0f;
            if (Vector3.Dot(length, toWatcher) < 0f) length = -length;
            return Quaternion.LookRotation(length);
        }

        // ------------------------------------------------------------------ the man again

        // The disc has cracked: he is lying where it lay, across the obelisk's shadow, and the sun leans a little from
        // noon for the first time, so the obelisk's own shadow is laid over him.
        private void LayMan(Vector3 where)
        {
            if (ManLaid || boss == null) return;
            ManLaid = true;
            transformFor = -1f;
            // On the shadow's middle line, however far the hand turned while the disc rolled and shuddered.
            float along = Mathf.Max(Vector3.Dot(where - Hand.Centre, Hand.Direction), Hand.From + 4f);
            where = Hand.Centre + Hand.Direction * along;
            Vector3 toWatcher = player != null ? player.transform.position - where : Vector3.back;
            boss.transform.SetPositionAndRotation(new Vector3(where.x, 0.05f, where.z), LyingAcross(Hand.Direction, toWatcher));
            ShowMan();
            boss.Shine(Color.black);
            boss.HoldPose(FallenClip, 0f);
            fallenFor = 0f;
            Vfx.Play(VfxKind.Dust, where, Vector3.up, 1.6f, new Color(1f, 0.88f, 0.64f, 1f));
            if (hisFire != null)
            {
                // What is left of the sun is the fire in his chest: taken from him there.
                hisFire.transform.SetPositionAndRotation(boss.transform.position - boss.transform.forward * 0.35f, boss.transform.rotation);
                hisFire.gameObject.SetActive(false);
            }
        }

        private void AdvanceFallen(float deltaTime)
        {
            if (fallenFor < 0f || boss == null) return;
            fallenFor += deltaTime;
            boss.HoldPose(FallenClip, FallenBreath > 0f ? Mathf.Repeat(fallenFor / FallenBreath, 1f) : 0f);
            // The sun starts to move the moment he is down: it leans from noon until the obelisk's shadow lies over him.
            if (!Sun.Begun) lightPitch = Mathf.MoveTowards(lightPitch, FallenPitch, 14f * deltaTime);
            if (!HasChosen && hisFire != null && hisFire.gameObject.activeSelf && Every(ref untilGlow, 0.35f, deltaTime))
                Vfx.Play(VfxKind.Embers, hisFire.transform.position + Vector3.up * 0.3f, Vector3.up, 0.5f);
            if (untilRegret < 0f) return;
            untilRegret -= deltaTime;
            if (untilRegret > 0f) return;
            untilRegret = -1f;
            if (RegretSaid) return;
            RegretSaid = true;
            ReadingBox.Show(Speaker, Regret, () =>
            {
                if (hisFire == null) return;
                hisFire.gameObject.SetActive(true);
                Vfx.Play(VfxKind.Embers, hisFire.transform.position + Vector3.up * 0.4f, Vector3.up, 1.2f);
                Vfx.Play(VfxKind.Ring, hisFire.transform.position + Vector3.up * 0.2f, Vector3.up);
            });
        }

        // ------------------------------------------------------------------ the sunset

        private void AdvanceSunset(float deltaTime)
        {
            if (!Sun.Begun) return;
            Sun.Tick(deltaTime);
            Hand.PointTo(Sun.ShadowAngle);
            lightPitch = Sun.Pitch;
            float warm = Sun.Warmth;
            if (keySource != null)
            {
                keySource.color = Color.Lerp(noonKey, SunsetKey, warm);
                keySource.intensity = Mathf.Lerp(noonIntensity, SunsetIntensity, warm);
            }
            RenderSettings.ambientLight = Color.Lerp(noonAmbient, SunsetAmbient, warm);
            RenderSettings.fogColor = Color.Lerp(noonFog, SunsetFog, warm);
            if (eye != null) eye.backgroundColor = Color.Lerp(noonSky, SunsetSky, warm);
            if (sunsetLook != null) sunsetLook.weight = warm;
            if (shade != null) shade.localScale = new Vector3(1f, 1f, Sun.ShadowStretch);
            float open = Sun.GateOpen;
            if (westGate != null)
            {
                if (open > 0f && westGateWall != null && westGateWall.enabled)
                {
                    westGateWall.enabled = false;
                    if (view != null) view.Shake(0.15f, 1.2f);
                    Vfx.Play(VfxKind.Dust, westGate.position, Vector3.up, 2.5f, new Color(1f, 0.82f, 0.6f, 1f));
                }
                westGate.localPosition = gateShut + Vector3.down * (GateDrop * Mathf.SmoothStep(0f, 1f, open));
            }
            if (sunRoad != null)
            {
                bool shown = open > 0.001f;
                if (sunRoad.enabled != shown) sunRoad.enabled = shown;
                if (shown)
                {
                    block.Clear();
                    block.SetColor(ColourId, RoadColour * open);
                    sunRoad.SetPropertyBlock(block);
                }
            }
            if (lowSun != null)
            {
                // The sun itself, low beyond the gate, is seen once it is low enough to be seen through it.
                float seen = Mathf.Clamp01((Sun.Progress - 0.45f) / 0.4f);
                bool shown = seen > 0.001f;
                if (lowSun.enabled != shown) lowSun.enabled = shown;
                if (shown)
                {
                    block.Clear();
                    block.SetColor(ColourId, LowSunColour * seen);
                    lowSun.SetPropertyBlock(block);
                }
            }
            if (!WayOpen && Sun.Done && sponsorDone)
            {
                WayOpen = true;
                ShowWayOut(true);
                if (player != null) player.Callout("West, after the sun");
            }
        }

        // ------------------------------------------------------------------ the camera

        private void Frame()
        {
            if (view == null) return;
            bool duel = Begun && !Felled && !SecondFormCalled && boss != null && !boss.IsDead;
            if (duel) view.SetFraming(DuelDistance, Vector3.zero, DuelPitch);
            // From his beating until the form has risen the camera holds him and whoever beat him; the form holds itself after.
            else if (Transforming && form != null && !form.HasRisen) view.FrameBoth(boss.transform.position, ShowDistance, ShowPitch, 3f, ManHeight);
            else if (Felled && ManLaid && !HasChosen) view.FrameBoth(boss.transform.position, 1.25f, -4f);
            else if (Sun.Begun && !WayOpen && player != null)
            {
                Vector3 toCourt = SunsetLookAt - player.transform.position;
                toCourt.y = 0f;
                view.SetFraming(SunsetDistance, toCourt, SunsetViewPitch);
            }
            // The way out holds the player and the low sun beyond the gate, so the road is seen to go into it.
            else if (WayOpen && wayOut != null) view.FrameBoth(wayOut.transform.position + WestBeyond, 1.2f, -4f);
            else
            {
                if (framed) view.ClearFraming();
                framed = false;
                return;
            }
            framed = true;
        }

        // The drawn shadow and the light that throws the real one both lie along the hand.
        private void TurnCourt()
        {
            // While the floor fills and blazes the shade is drawn over the floor's mark, so the one safe strip is the darkest thing in the court.
            bool overMarks = form is SunBearer sunBearer && sunBearer.AtZenith;
            if (shade != null) shade.SetPositionAndRotation(Hand.Centre + Vector3.up * (overMarks ? 0.1f : 0.045f), Quaternion.Euler(0f, Hand.Angle, 0f));
            if (keyLight != null) keyLight.rotation = Quaternion.Euler(lightPitch, Hand.Angle, 0f);
        }

        // The thrown spear stands where its lane ended until the ray has brought it back to his hand.
        private void WatchSpear()
        {
            if (boss == null) return;
            var attack = boss.Brain.Current;
            bool attacking = !boss.IsDead && boss.Brain.IsAttacking && attack != null;
            bool thrown = attacking && attack.Name == "sunking_throw" && boss.Brain.State != EnemyState.Telegraph;
            bool calling = attacking && attack.Name == "sunking_ray" && boss.Brain.State == EnemyState.Telegraph;
            if (thrown && !SpearStands && standingSpear != null)
            {
                Vector3 from = boss.transform.position, along = boss.transform.forward;
                along.y = 0f;
                float reach = Mathf.Min(attack.Reach, StormDragon.Chord(from, along.normalized, 19f));
                standingSpear.position = new Vector3(from.x, 0f, from.z) + along.normalized * reach;
                standingSpear.rotation = Quaternion.LookRotation(along.normalized) * Quaternion.Euler(-14f, 0f, 0f);
                // It flies the lane first, point first, trailing fire; where it lands is where it stands.
                standAt = standingSpear.position;
                standTurn = standingSpear.rotation;
                flyFrom = new Vector3(from.x, 0f, from.z) + along.normalized * 0.8f;
                flyAlong = along.normalized;
                flightLeft = SpearFlight;
                if (spearTrail != null)
                {
                    spearTrail.Clear();
                    spearTrail.emitting = true;
                }
            }
            if (flightLeft > 0f && standingSpear != null)
            {
                flightLeft -= Time.deltaTime;
                float t = 1f - Mathf.Clamp01(flightLeft / SpearFlight);
                // The group stands its blade up its own up axis: in flight that axis points down the lane.
                standingSpear.SetPositionAndRotation(Vector3.Lerp(flyFrom, standAt, t) - flyAlong * 3.6f + Vector3.up * 1.6f, Quaternion.FromToRotation(Vector3.up, flyAlong));
                if (flightLeft <= 0f)
                {
                    standingSpear.SetPositionAndRotation(standAt, standTurn);
                    if (spearTrail != null) spearTrail.emitting = false;
                    // Driven into the floor: a star, a gold ring, sand thrown up and the view shaken.
                    Vfx.Play(VfxKind.Impact, standAt + Vector3.up * 0.5f, Vector3.up, 1f, new Color(1f, 0.92f, 0.62f, 1f));
                    Vfx.Play(VfxKind.TintRing, standAt + Vector3.up * 0.25f, Vector3.up, 1.2f, new Color(1f, 0.72f, 0.2f, 1f));
                    for (int i = 0; i < 4; i++) Vfx.Play(VfxKind.Dust, standAt + Quaternion.Euler(0f, i * 90f + 45f, 0f) * Vector3.forward * 0.8f, Vector3.up, 1f, new Color(1f, 0.88f, 0.64f, 1f));
                    if (view != null) view.Shake(0.3f, 0.2f);
                }
            }
            ShowSpear(thrown || calling);
            // Standing, it smokes from where it went in and drops embers from the sun's disc.
            if (SpearStands && standingSpear != null)
            {
                untilSmoke -= Time.deltaTime;
                if (untilSmoke <= 0f)
                {
                    untilSmoke = 0.2f;
                    Vfx.Play(VfxKind.Ash, standingSpear.position + Vector3.up * 0.3f, Vector3.up, 0.5f, new Color(0.26f, 0.24f, 0.23f, 0.85f));
                    Vfx.Play(VfxKind.Embers, standingSpear.position + Vector3.up * 3.3f, Vector3.down, 0.25f);
                }
            }
            if (rayLook != null)
            {
                Vector3 blade = standingSpear != null ? standingSpear.position + Vector3.up * 1.6f : boss.transform.position;
                Vector3 hand = boss.transform.position + Vector3.up * 2.4f;
                if (calling) rayLook.Thread(blade, hand, boss.Brain.TelegraphProgress);
                else rayLook.HideThread();
                bool striking = attacking && attack.Name == "sunking_ray" && boss.Brain.State == EnemyState.Strike;
                if (striking && !rayStruck)
                {
                    // The spear is already back in his hand; the beam is drawn from where it stood.
                    rayLook.Strike(lastBlade, hand);
                    ScreenFlash.Flash(new Color(1f, 0.95f, 0.8f, 0.3f), 0.06f);
                    if (view != null) view.Shake(0.3f, 0.2f);
                    Vfx.Play(VfxKind.Clang, hand, Vector3.up, 1f);
                    Vfx.Play(VfxKind.TintRing, new Vector3(hand.x, 0.3f, hand.z), Vector3.up, 1f, new Color(1f, 0.72f, 0.2f, 1f));
                }
                rayStruck = striking;
                if (calling) lastBlade = blade;
                return;
            }
            if (ray == null) return;
            // The last of the call: a ray of light along the line the spear will come back down.
            bool lit = calling && boss.Brain.TelegraphProgress > 0.55f;
            if (ray.enabled != lit) ray.enabled = lit;
            if (!lit) return;
            ray.SetPosition(0, standingSpear.position + Vector3.up * 1.6f);
            ray.SetPosition(1, boss.transform.position + Vector3.up * 2.4f);
            ray.widthMultiplier = Mathf.Lerp(0.1f, 0.9f, (boss.Brain.TelegraphProgress - 0.55f) / 0.45f);
        }

        private void ShowSpear(bool stands)
        {
            if (SpearStands == stands && standingSpear != null && standingSpear.gameObject.activeSelf == stands) return;
            SpearStands = stands;
            if (standingSpear != null) standingSpear.gameObject.SetActive(stands);
            if (!stands && ray != null) ray.enabled = false;
            if (!stands)
            {
                flightLeft = 0f;
                if (spearTrail != null) spearTrail.emitting = false;
            }
        }

        // After the animation has posed him: his hands are empty while the spear stands in the floor, and once it is taken from him.
        private void LateUpdate()
        {
            if (weaponBone != null) weaponBone.localScale = SpearStands || HasChosen ? Vector3.one * 0.001f : Vector3.one;
        }

        /// <summary>
        /// His fire is taken: the sun moves for the first time in ages and starts down into the west while the sponsor
        /// speaks; when both are done the west gate stands open to the low sun and its beam is the way out.
        /// </summary>
        public void Chose(GodReward chosen)
        {
            HasChosen = true;
            if (hisFire != null) hisFire.gameObject.SetActive(false);
            Sun.Begin(Hand.Angle, lightPitch);
            ReadingBox.Show(BarqueSpeaker, Barque, () => sponsorDone = true);
        }
    }
}

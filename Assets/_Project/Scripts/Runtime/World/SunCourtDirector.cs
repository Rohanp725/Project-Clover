using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// Runs the fight in the sun-court (fire_land.md, section 6): turns the obelisk's shadow and the court's light
    /// with it; begins the fight when the gold line is crossed (the king speaks first, click-through, and a wall of
    /// heat closes behind); shows the thrown spear standing in the floor until he calls it back; brings the sun down
    /// when the man falls and raises what he became; and when that falls, lays the coal in the shadow, which can
    /// only be taken. A fall before the end puts the god back as he was, whole, at the far side of the court.
    /// </summary>
    public class SunCourtDirector : MonoBehaviour
    {
        [SerializeField] private Enemy boss;
        [SerializeField] private PlayerDefence player;
        [SerializeField] private HitStop hitStop;
        [SerializeField] private GodlyForm form;
        [SerializeField] private BossHud bossHud;
        [SerializeField] private GodReward coal;
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
        /// <summary>Seconds the thrown spear takes to fly its lane, point first, before it stands.</summary>
        public float SpearFlight = 0.2f;

        /// <summary>The gold line: the fight begins when the player is north of it.</summary>
        public float LineZ = -20f;
        /// <summary>Short, so the sun is already coming for him while his body burns away.</summary>
        public float RisingDelay = 0.9f;
        public float RewardDelay = 1.2f;
        public int Embers = 5;
        public string ManName = "Khepharu,  God of Fire";
        public string GodlyName = "Khepharu,  the Sun-Bearer";
        public string Speaker = "Khepharu";
        [TextArea] public string[] Greeting = new string[0];
        [TextArea] public string[] DyingWords = new string[0];
        [TextArea] public string[] Barque = new string[0];
        public string BarqueSpeaker = "";
        public string HalfCallout = "It is always noon.";
        public string RisingCallout = "The sun comes down";
        public string RoarCallout = "Khepharu, the Sun-Bearer";
        public string FuryCallout = "Noon breaks";

        public ShadowHand Hand { get; private set; }
        public bool Begun { get; private set; }
        public bool Felled { get; private set; }
        public bool HasChosen { get; private set; }
        public bool SecondFormCalled { get; private set; }
        public bool SpearStands { get; private set; }
        public int Retries { get; private set; }

        /// <summary>How the camera stands for the duel with the man: a little further off and steeper, so the obelisk hides less of the court behind it.</summary>
        public float DuelDistance = 1.3f, DuelPitch = -8f;

        private float untilRising = -1f, untilReward = -1f, lightPitch = 78f;
        private Vector3 lastBlade, standAt, flyFrom, flyAlong;
        private Quaternion standTurn;
        private float flightLeft;
        private float untilSmoke;
        private bool spoken, framed, rayStruck;
        private TopDownCamera view;
        private PlayerUpgrades upgrades;

        public void Configure(Enemy bossToWatch, PlayerDefence playerToServe, HitStop stop, GodlyForm godlyForm, BossHud hud, GodReward reward, SceneDoor door)
        {
            boss = bossToWatch;
            player = playerToServe;
            hitStop = stop;
            form = godlyForm;
            bossHud = hud;
            coal = reward;
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

        private void Awake()
        {
            Hand = new ShadowHand();
            Hand.Reset(ShadowHand.AgainstTheMan);
            // The same shade the sun-blind hunt by: whoever asks whether a point is lit is told of the hand.
            Sunlight.SetOpenSky(true);
            Sunlight.Hand = Hand;
            if (keyLight != null) lightPitch = keyLight.eulerAngles.x;
        }

        private void Start()
        {
            if (coal != null) coal.gameObject.SetActive(false);
            ShowWayOut(false);
            SetWall(false);
            var main = Camera.main;
            view = main != null ? main.GetComponent<TopDownCamera>() : null;
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
            // The man is dead. The sun he carried is not.
            SecondFormCalled = true;
            if (hitStop != null) hitStop.Slow(1.4f, 0.25f);
            ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.5f), 0.09f);
            if (form is SunBearer bearer) bearer.RiseFrom = who.transform.position;
            if (player != null)
            {
                // A breath before the worst of it: the flasks are not given back, but the wounds of the duel are eased.
                player.Health.Heal(player.Health.Maximum * 0.35f);
            }
            untilRising = RisingDelay;
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
            // The five embers of a god are paid when the form falls, not the man.
            if (upgrades != null) upgrades.Earn(Embers);
            // He speaks, kneeling, and is ash; then the coal lies in the shadow.
            ReadingBox.Show(Speaker, DyingWords, () => untilReward = RewardDelay);
        }

        // The player fell before the end: the god is whole again and waits at the far side, the shadow points north,
        // the way back out is open, and the fight begins again when the line is crossed.
        private void OnRevived()
        {
            if (Felled) return;
            Retries++;
            Begun = false;
            SecondFormCalled = false;
            untilRising = -1f;
            SetWall(false);
            ShowSpear(false);
            if (rayLook != null) rayLook.Clear();
            if (form != null) form.Retire();
            if (boss != null && boss.IsDead) boss.Restore();
            Hand.Reset(ShadowHand.AgainstTheMan);
            if (bossHud != null) bossHud.Show(boss, ManName);
        }

        private void Update()
        {
            // The shadow stops when he is dead; until then it never does.
            if (!Felled) Hand.Tick(Time.deltaTime * WorldClock.EnemyScale);
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
            bool duel = Begun && !Felled && !SecondFormCalled && boss != null && !boss.IsDead;
            if (view != null)
            {
                if (duel) view.SetFraming(DuelDistance, Vector3.zero, DuelPitch);
                else if (framed) view.ClearFraming();
                framed = duel;
                // Between his fall and the form's rising the camera is already going to where he lies.
                if (untilRising >= 0f && player != null && form is SunBearer coming)
                {
                    Vector3 toHim = coming.RiseFrom - player.transform.position;
                    toHim.y = 0f;
                    view.SetFraming(coming.ShowDistance, toHim, coming.ViewPitch);
                }
            }
            if (untilRising >= 0f)
            {
                untilRising -= Time.unscaledDeltaTime;
                if (untilRising < 0f && form != null)
                {
                    form.Rise();
                    if (bossHud != null) bossHud.Show(form, GodlyName);
                    if (player != null) player.Callout(RisingCallout);
                }
            }
            if (untilReward < 0f) return;
            untilReward -= Time.unscaledDeltaTime;
            if (untilReward > 0f) return;
            untilReward = -1f;
            if (coal == null) return;
            // In the obelisk's shadow, where the sun rolled to and shrank.
            Vector3 lies = Hand.Centre + Hand.Direction * 4.6f;
            coal.transform.position = new Vector3(lies.x, coal.transform.position.y, lies.z);
            coal.gameObject.SetActive(true);
            Vfx.Play(VfxKind.Embers, coal.transform.position + Vector3.up, Vector3.up, 1.5f);
            Vfx.Play(VfxKind.Ring, coal.transform.position + Vector3.up * 0.2f, Vector3.up);
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

        // After the animation has posed him: his hands are empty while the spear stands in the floor.
        private void LateUpdate()
        {
            if (weaponBone != null) weaponBone.localScale = SpearStands ? Vector3.one * 0.001f : Vector3.one;
        }

        /// <summary>The coal is taken: the sponsor speaks, and then the way out of the land opens.</summary>
        public void Chose(GodReward chosen)
        {
            HasChosen = true;
            if (coal != null) coal.gameObject.SetActive(false);
            ReadingBox.Show(BarqueSpeaker, Barque, () =>
            {
                ShowWayOut(true);
            });
        }
    }
}

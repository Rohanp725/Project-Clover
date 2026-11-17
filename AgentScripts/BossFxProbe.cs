using System;
using System.Collections;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the look of the Fire bosses' attacks (docs/design/fire_boss_visuals.md): Khepharu's thrusts,
// sweep, plant, noon, throw and ray, then the Sun-Bearer's roll, stoop and pass, each pictured with time stopped at the
// moment its effect is strongest: review/shots/current/bossfx_<what>_<n>.png. Run with
//   bash AgentScripts/runprobe.sh SunCourt BossFxProbe 120
// AgentScripts/bossfx_only.txt may hold words (thrust sweep plant noon throw ray roll stoop pass parry) to run only those.
public static class BossFxProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Boss Fx Probe").AddComponent<BossFxProbeRunner>();
        return "probe started";
    }
}

public class BossFxProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerDefence defence;
    private SunCourtDirector court;
    private SunBearer form;
    private Enemy boss;
    private string only = "";
    private bool struckNow;
    private Func<Vector3> follow;

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
        if (follow != null && player != null) player.Teleport(follow(), player.transform.rotation);
    }

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private bool Wants(string word) => only.Length == 0 || only.Contains(word);

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.realtimeSinceStartup + timeout;
        while (!condition() && Time.realtimeSinceStartup < end) yield return null;
    }

    private IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
        yield return null;
    }

    // Time is stopped for the picture, so what is drawn is what was there the instant it was asked for.
    private IEnumerator Frozen(string name)
    {
        // A hit-stop in force lets go when anything else sets the time scale, so time is put back to its usual speed after.
        Time.timeScale = 0f;
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/bossfx_" + name + ".png");
        yield return null;
        yield return null;
        Time.timeScale = 1f;
        Say($"   picture bossfx_{name}");
    }

    private IEnumerator Read()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        var box = ReadingBox.Instance;
        for (int i = 0; i < 30 && box != null && box.IsOpen; i++)
        {
            yield return Tap(Key.E);
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }

    // A picture taken as time runs, for the beats whose slowing is what is being looked at.
    private IEnumerator Shot(string name)
    {
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/bossfx_" + name + ".png");
        yield return null;
        Say($"   picture bossfx_{name} at time scale {Time.timeScale:0.00}");
    }

    private void Go(Vector3 to, Vector3 facing)
    {
        facing.y = 0f;
        player.Teleport(new Vector3(to.x, 0.1f, to.z), facing.sqrMagnitude > 0.01f ? Quaternion.LookRotation(facing) : Quaternion.identity);
    }

    private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    private void OnStruck(Enemy who, EnemyAttack attack) => struckNow = true;

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        court = FindAnyObjectByType<SunCourtDirector>();
        form = FindAnyObjectByType<SunBearer>();
        boss = FindAnyObjectByType<AreaDirector>().Enemies[0];
        only = System.IO.File.Exists("AgentScripts/bossfx_only.txt") ? System.IO.File.ReadAllText("AgentScripts/bossfx_only.txt").Trim() : "";
        boss.AttackStruck += OnStruck;
        yield return new WaitForSeconds(1.2f);
        yield return Read();
        Go(new Vector3(0f, 0f, -18.6f), Vector3.forward);
        yield return new WaitForSeconds(0.3f);
        yield return Read();
        Say($"start: begun {court.Begun} boss {boss.name}");

        if (Wants("thrust")) yield return Man("sunking_thrust_1", 4f, "thrust", 3);
        if (Wants("thrust")) yield return Man("sunking_reach", 5.5f, "reach", 1);
        if (Wants("sweep")) yield return Man("sunking_sweep", 2.4f, "sweep", 1);
        if (Wants("plant")) yield return Man("sunking_plant", 3f, "plant", 1);
        if (Wants("noon")) yield return Man("sunking_noon_1", 10f, "noon", 3);
        if (Wants("throw") || Wants("ray")) yield return ThrowAndRay();
        if (Wants("sunspear")) yield return SunSpear();

        if (Wants("roll") || Wants("stoop") || Wants("pass") || Wants("parry") || Wants("zenith") || Wants("sparks") || Wants("feathers") || Wants("rising"))
        {
            yield return Rise();
            if (Wants("rising")) yield return Rising();
            if (Wants("roll")) yield return Roll();
            if (Wants("parry")) yield return Parry();
            if (Wants("stoop")) yield return Stoop();
            if (Wants("pass")) yield return Pass();
            if (Wants("zenith")) yield return Zenith();
            if (Wants("sparks")) yield return Moment(SunBearerAttack.Sparks, "sparks", 1.15f, 1.3f);
            if (Wants("feathers")) yield return Moment(SunBearerAttack.Feathers, "feathers", 1.0f, 1.25f);
        }
        boss.AttackStruck -= OnStruck;
        Say("done");
        Debug.Log("[Probe] done");
        Destroy(gameObject);
    }

    // Forces a move of the man at a player standing still and pictures each blow of its chain the frame after it lands, then once more a little later.
    private IEnumerator Man(string move, float distance, string name, int blows)
    {
        boss.Restore();
        boss.transform.SetPositionAndRotation(new Vector3(6f, 0.05f, 6f), Quaternion.LookRotation(Vector3.back));
        Go(boss.transform.position + Vector3.back * distance, Vector3.forward);
        boss.TakeHit(new Hit(0.01f, 0f, Vector3.forward));
        yield return new WaitForSeconds(0.2f);
        Vector3 stand = boss.transform.position + boss.transform.forward * distance;
        Go(stand, boss.transform.position - stand);
        bool forced = boss.Brain.Force(move);
        if (name == "noon")
        {
            // The shaft standing over its circle while the circle fills.
            yield return Until(() => boss.Brain.TelegraphProgress > 0.75f, 3f);
            yield return Frozen("noon_tell");
        }
        for (int i = 0; i < blows; i++)
        {
            struckNow = false;
            yield return Until(() => struckNow, 4f);
            yield return new WaitForSeconds(name == "noon" ? 0.02f : 0.04f);
            yield return Frozen($"{name}_{i + 1}");
        }
        yield return new WaitForSeconds(0.25f);
        yield return Frozen($"{name}_after");
        Say($"{move}: forced {forced}");
        yield return new WaitForSeconds(1.5f);
    }

    private IEnumerator ThrowAndRay()
    {
        boss.Restore();
        boss.transform.SetPositionAndRotation(new Vector3(-8f, 0.05f, 6f), Quaternion.LookRotation(Vector3.right));
        Go(new Vector3(2f, 0f, 6f), Vector3.left);
        boss.TakeHit(new Hit(0.01f, 0f, Vector3.forward));
        yield return new WaitForSeconds(0.4f);
        Go(new Vector3(boss.transform.position.x + boss.transform.forward.x * 10f, 0f, boss.transform.position.z + boss.transform.forward.z * 10f), -boss.transform.forward);
        boss.Brain.Force("sunking_throw");
        struckNow = false;
        yield return Until(() => struckNow, 4f);
        yield return new WaitForSeconds(0.04f);
        yield return Frozen("throw_1");
        yield return Until(() => court.SpearStands, 3f);
        yield return new WaitForSeconds(0.4f);
        yield return Frozen("throw_2");
        yield return Until(() => boss.Brain.Current != null && boss.Brain.Current.Name == "sunking_ray" && boss.Brain.TelegraphProgress > 0.3f, 4f);
        yield return Frozen("ray_1");
        yield return Until(() => boss.Brain.Current == null || boss.Brain.Current.Name != "sunking_ray" || boss.Brain.TelegraphProgress > 0.85f, 4f);
        yield return Frozen("ray_2");
        yield return Until(() => boss.Brain.Current == null || boss.Brain.State != Clover.Gameplay.EnemyState.Telegraph, 3f);
        yield return new WaitForSeconds(0.05f);
        yield return Frozen("ray_3");
        yield return new WaitForSeconds(0.5f);
        yield return Frozen("ray_after");
        Say($"throw and ray: spear stood and came back {!court.SpearStands}");
        yield return new WaitForSeconds(1.5f);
    }

    // The Sun-Spear's own move: thrown at open floor, it stands; the man is put on the line home; the ray must strike him.
    private IEnumerator SunSpear()
    {
        var combat = player.GetComponent<PlayerCombat>();
        var shots = player.GetComponent<WeaponShots>();
        combat.Equip(Weapons.ByName(Weapons.SunSpearName));
        boss.Restore();
        Vector3 stand = new Vector3(-6f, 0f, -6f);
        Go(stand, Vector3.right);
        boss.transform.SetPositionAndRotation(new Vector3(-6f, 0.05f, 6f), Quaternion.LookRotation(Vector3.back));
        yield return new WaitForSeconds(0.5f);
        combat.Mana.Gain(200f);
        int thrown = shots.Thrown, rays = shots.Rays, rayStruck = shots.RayStruck;
        float health = boss.Health.Current;
        // A blow from him in the meantime can cut the hold short: it is tried again, up to three times.
        for (int attempt = 0; attempt < 3 && !combat.WeaponAway; attempt++)
        {
            boss.transform.SetPositionAndRotation(new Vector3(-6f, 0.05f, 6f), Quaternion.LookRotation(Vector3.back));
            Go(stand, Vector3.right);
            combat.Mana.Gain(200f);
            yield return new WaitForSeconds(0.3f);
            Keys(Key.K);
            yield return new WaitForSeconds(combat.Weapon.Combo.SkillChargeTime + 0.15f);
            Keys();
            yield return Until(() => combat.WeaponAway, 1.5f);
        }
        yield return Until(() => SpearStands(shots), 3f);
        yield return new WaitForSeconds(0.1f);
        yield return Frozen("sunspear_1");
        // Him on the line between where it stands and the hand, held there.
        Vector3 between = Vector3.Lerp(player.transform.position, player.transform.position + Vector3.right * 10f, 0.5f);
        Func<bool> notYet = () => shots.Rays == rays;
        while (notYet() && combat.WeaponAway)
        {
            boss.transform.position = new Vector3(between.x, 0.05f, between.z);
            yield return null;
        }
        yield return null;
        yield return Frozen("sunspear_2");
        yield return Until(() => !combat.WeaponAway, 2f);
        yield return Frozen("sunspear_3");
        Say($"the Sun-Spear: equipped {combat.Weapon.Name} carry {combat.Weapon.Carry}, thrown +{shots.Thrown - thrown}, rays +{shots.Rays - rays}, ray struck +{shots.RayStruck - rayStruck}, "
            + $"the man lost {health - boss.Health.Current:0.#}, in the hand again {!combat.WeaponAway} {(shots.Rays > rays && shots.RayStruck > rayStruck && boss.Health.Current < health ? "OK" : "WRONG")}");
        combat.Equip(Weapons.ByName(Weapons.GreatSpearName));
        yield return new WaitForSeconds(1f);
    }

    private static bool SpearStands(WeaponShots shots)
    {
        for (int i = 0; i < 6; i++)
            if (shots.IsStuck(i)) return true;
        return false;
    }

    private IEnumerator Rise()
    {
        boss.Restore();
        boss.transform.SetPositionAndRotation(new Vector3(0f, 0.05f, 8f), Quaternion.LookRotation(Vector3.back));
        Go(new Vector3(0f, 0f, -6f), Vector3.forward);
        yield return new WaitForSeconds(0.3f);
        for (int blow = 0; blow < 200 && !boss.IsDead; blow++) boss.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
        form.OnlyWhenAsked = true;
        yield return Until(() => form.HasRisen, 8f);
        yield return Until(() => form.Doing == "Idle", 14f);
        Say($"the form has risen: doing {form.Doing}");
    }

    private Vector3 RoundFrom(float degrees, float radius)
    {
        Vector3 flat = Flat(form.BodyAt);
        if (flat.sqrMagnitude < 1f) flat = Vector3.forward;
        return Quaternion.Euler(0f, degrees, 0f) * flat.normalized * radius;
    }

    private IEnumerator Roll()
    {
        yield return Until(() => form.Doing == "Idle", 14f);
        yield return Until(() => form.HotLanes == 0, 16f);
        Vector3 stand = RoundFrom(75f, 12.5f);
        Go(stand, form.BodyAt - stand);
        form.Queued = SunBearerAttack.Roll;
        yield return Until(() => form.Doing == "Roll", 12f);
        yield return Until(() => form.HotLanes > 0 || form.Doing != "Roll", 8f);
        // Off the lane once it is rolling, so the camera sees the lane and the sun go by.
        Vector3 across = new Vector3(form.LaneAlong.z, 0f, -form.LaneAlong.x);
        Go(stand + across * 6.5f, form.SunAt - stand);
        yield return new WaitForSeconds(0.45f);
        yield return Frozen("roll_1");
        yield return new WaitForSeconds(0.5f);
        yield return Frozen("roll_2");
        yield return Until(() => form.Doing != "Roll", 6f);
        yield return new WaitForSeconds(1.2f);
        yield return Frozen("roll_3");
        yield return new WaitForSeconds(5f);
        yield return Frozen("roll_cooling");
        Say($"roll: hot lanes {form.HotLanes} doing {form.Doing}");
    }

    private IEnumerator Parry()
    {
        yield return Until(() => form.Doing == "Idle", 20f);
        yield return Until(() => form.HotLanes == 0, 16f);
        Vector3 stand = RoundFrom(75f, 12.5f);
        form.Queued = SunBearerAttack.Roll;
        Go(stand, form.BodyAt - stand);
        yield return Until(() => form.Doing == "Roll", 12f);
        yield return Until(() => form.Locked || form.Doing != "Roll", 6f);
        Go(stand, form.SunAt - stand);
        int knocks = form.SunKnocks, perfect = defence.PerfectGuards, taken = defence.BlowsTaken;
        float until = Time.time + 8f;
        bool pressed = false;
        while (Time.time < until && form.Doing == "Roll" && form.SunKnocks == knocks && defence.BlowsTaken == taken)
        {
            if (!pressed && Flat(form.SunAt - player.transform.position).magnitude < 3.1f)
            {
                Keys(Key.L);
                pressed = true;
            }
            yield return null;
        }
        float knockedAt = Time.realtimeSinceStartup;
        yield return new WaitForSecondsRealtime(0.08f);
        yield return Shot("parry_1");
        yield return new WaitForSecondsRealtime(0.25f);
        yield return Shot("parry_2");
        yield return Until(() => Time.realtimeSinceStartup - knockedAt > 0.6f, 2f);
        yield return Shot("parry_3");
        yield return new WaitForSecondsRealtime(0.8f);
        Keys();
        yield return Frozen("parry_4");
        Say($"parry: perfect +{defence.PerfectGuards - perfect} knocks +{form.SunKnocks - knocks} loose {form.SunLoose} exposed {form.Exposed} broken {form.IsStaggered} blows taken +{defence.BlowsTaken - taken}");
        yield return Until(() => !form.Exposed && !form.IsStaggered, 20f);
    }

    private IEnumerator Stoop()
    {
        yield return Until(() => form.Doing == "Idle", 20f);
        Vector3 stand = new Vector3(6f, 0f, -8f);
        Go(stand, form.BodyAt - stand);
        form.Queued = SunBearerAttack.Stoop;
        yield return Until(() => form.Doing == "Stoop", 20f);
        float tell = form.StoopFollow + form.StoopHold;
        yield return Until(() => form.DoingFor >= tell - 0.15f || form.Doing != "Stoop", 8f);
        yield return Frozen("stoop_1");
        yield return Until(() => form.DoingFor >= tell + 0.17f || form.Doing != "Stoop", 4f);
        yield return Frozen("stoop_2");
        yield return Until(() => form.DoingFor >= tell + 0.24f + 0.07f || form.Doing != "Stoop", 4f);
        yield return Frozen("stoop_3");
        yield return new WaitForSeconds(0.5f);
        yield return Frozen("stoop_4");
        Say($"stoop: hot patches {form.HotPatches}");
        yield return Until(() => form.Doing != "Stoop", 8f);
    }

    // The Rising (a falcon's move asked of the scarab): the pillar of light as the sun is thrown up, and its trail.
    private IEnumerator Rising()
    {
        yield return Until(() => form.Doing == "Idle", 25f);
        if (form.IsFalcon) yield break;
        Vector3 stand = Flat(form.BodyAt) + Flat(new Vector3(0f, 0f, -9f) - form.BodyAt).normalized * 8f;
        Go(stand, form.BodyAt - stand);
        form.Queued = SunBearerAttack.Stoop;
        yield return Until(() => form.Doing == "Rising", 25f);
        yield return Until(() => form.DoingFor >= 1.26f || form.Doing != "Rising", 8f);
        yield return Frozen("rising_1");
        yield return Until(() => form.DoingFor >= 1.55f || form.Doing != "Rising", 8f);
        yield return Frozen("rising_2");
        yield return Until(() => form.Doing == "Idle", 25f);
        Say($"rising: falcon {form.IsFalcon}");
    }

    // A move asked of the form with the player standing clear of the obelisk, pictured at two moments of it.
    private IEnumerator Moment(SunBearerAttack attack, string name, float first, float second)
    {
        yield return Until(() => form.Doing == "Idle", 25f);
        Vector3 stand = Flat(form.BodyAt) + Flat(new Vector3(0f, 0f, -9f) - form.BodyAt).normalized * 7f;
        Go(stand, form.BodyAt - stand);
        form.Queued = attack;
        string doing = attack.ToString();
        yield return Until(() => form.Doing == doing, 25f);
        yield return Until(() => form.DoingFor >= first || form.Doing != doing, 8f);
        yield return Frozen(name + "_1");
        yield return Until(() => form.DoingFor >= second || form.Doing != doing, 8f);
        yield return Frozen(name + "_2");
        Say($"{name}: falls laid {form.FallsLaid}");
        yield return Until(() => form.Doing != doing, 10f);
    }

    // The blaze: the court in the sun, shimmering, and the shadow still.
    private IEnumerator Zenith()
    {
        yield return Until(() => form.Doing == "Idle", 20f);
        Vector3 stand = new Vector3(-8f, 0f, -9f);
        Go(stand, Vector3.forward);
        form.Queued = SunBearerAttack.Zenith;
        yield return Until(() => form.Doing == "Zenith", 25f);
        // The floor filling red while the court whitens: the warning must still read.
        yield return Until(() => form.DoingFor >= 3.6f || form.Doing != "Zenith", 10f);
        yield return Frozen("zenith_fill");
        yield return Until(() => form.Blazing || form.Doing != "Zenith", 10f);
        yield return new WaitForSeconds(1.2f);
        yield return Frozen("zenith_1");
        follow = () => court.Hand.Centre + court.Hand.Direction * 7f + Vector3.up * 0.1f;
        yield return new WaitForSeconds(1.5f);
        yield return Frozen("zenith_2");
        follow = null;
        Say($"zenith: blazing {form.Blazing}");
        yield return Until(() => form.Doing != "Zenith", 12f);
    }

    private IEnumerator Pass()
    {
        yield return Until(() => form.Doing == "Idle", 20f);
        yield return Until(() => form.HotLanes == 0, 16f);
        if (!form.IsFalcon)
        {
            form.Queued = SunBearerAttack.Stoop;
            yield return Until(() => form.Doing == "Stoop", 20f);
            yield return Until(() => form.Doing == "Idle", 20f);
        }
        Vector3 stand = new Vector3(8f, 0f, -7f);
        Go(stand, form.BodyAt - stand);
        form.Queued = SunBearerAttack.Pass;
        yield return Until(() => form.Doing == "Pass", 20f);
        yield return Until(() => form.DoingFor >= form.PassShown + 0.1f || form.Doing != "Pass", 8f);
        Vector3 across = new Vector3(form.LaneAlong.z, 0f, -form.LaneAlong.x);
        Go(stand + across * 7f, Vector3.forward);
        yield return Until(() => form.DoingFor >= form.PassShown + 0.45f || form.Doing != "Pass", 8f);
        yield return Frozen("pass_1");
        yield return Until(() => form.DoingFor >= form.PassShown + 0.75f || form.Doing != "Pass", 8f);
        yield return Frozen("pass_2");
        yield return new WaitForSeconds(1.2f);
        yield return Frozen("pass_3");
        Say($"pass: hot lanes {form.HotLanes}");
    }
}

using Clover.Gameplay;
using Clover.World;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Stone Oak: a sapling of stone and leaf planted Reach ahead, facing the player, living Seconds. Every six of the player's
    /// weapon blows landed within eight metres of it grow it an age (OakRules). The sapling strikes nothing; the young tree's
    /// roots strike the nearest enemy within 5 m every Interval (Damage, Stagger and a Tremor); the old oak's strike every
    /// enemy within 6 m and Seed them, and the player heals 3 a second in its shade (4 m). It stands in the way like a pillar.
    /// Its body is the stone oak model when the editor step has wired it (CreatureLibrary), else a trunk and a crown of rock.
    /// </summary>
    public class OakForm : RunningForm
    {
        public override string Id => SpellForms.Oak;

        // The note the place of the standing thing it blocks the way with is kept in (one more than the place; 0 for none).
        private const int PillarNote = 15;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            Vector3 forward = cast.Forward;
            forward.y = 0f;
            run.B = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            run.A = cast.Origin + run.B * Mathf.Max(0.5f, cast.Reach);
            run.T0 = Mathf.Max(1f, cast.Spec.Seconds);
            run.T1 = Mathf.Max(0.1f, cast.Spec.Delay) + Mathf.Max(0.5f, cast.Spec.Interval);
            run.Done = 0;
            run.Count = 1;
            // It stands in the way like a pillar (no body of its own there: the oak is drawn here).
            SpellWorks.Times(run)[PillarNote] = 1 + StandingThings.Raise(StandingKind.StonePillar, run.A, -run.B, run.T0, 0f, Time.time, -1, 0f, 0f, false);
            var rig = CreatureLibrary.Make(CreatureKind.StoneOak, run.A, Quaternion.LookRotation(-run.B, Vector3.up));
            if (rig != null)
            {
                rig.ShowAge(1);
                rig.Play("oak_grow");
                SpellWorks.Things(run)[0] = rig;
            }
            else
            {
                SpellWorks.Solids(run)[0] = FormFx.Hold(FormMesh.Pillar, cast.Colour, 0.04f);
                SpellWorks.Solids(run)[1] = FormFx.Hold(FormMesh.Rock, cast.Colour, 0.04f);
            }
            ElementLook.Puff(cast.Element, run.A + Vector3.up * 0.4f, Vector3.up, 1.2f);
            return 0;
        }

        // A weapon blow of the player's landed: within eight metres of it, it counts toward the next age.
        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage)
        {
            if (target == null) return;
            Vector3 gap = target.transform.position - run.A;
            gap.y = 0f;
            if (gap.magnitude <= OakRules.GrowReach) run.Done++;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            if (cast.Player == null || cast.Combat == null) return false;
            var rig = SpellWorks.Things(run)[0] as CreatureRig;
            int age = OakRules.AgeFor(run.Done);
            if (age != run.Count)
            {
                run.Count = age;
                if (rig != null)
                {
                    rig.ShowAge(age);
                    rig.Play("oak_grow");
                }
                ElementLook.Puff(cast.Element, run.A + Vector3.up * (0.8f * age), Vector3.up, 1f + 0.3f * age);
            }
            Show(run, rig, age);
            if (run.Age >= run.T0) return false;
            // Its roots on their beat.
            float reach = OakRules.StrikeReach(age);
            if (run.Age >= run.T1)
            {
                run.T1 = run.Age + Mathf.Max(0.5f, cast.Spec.Interval);
                if (reach > 0f) Strike(run, rig, age, reach);
            }
            // Its shade.
            float heal = OakRules.HealsInShade(age);
            if (heal > 0f && cast.Defence != null && !cast.Defence.IsDead)
            {
                Vector3 gap = cast.Player.transform.position - run.A;
                gap.y = 0f;
                if (gap.magnitude <= OakRules.ShadeReach) cast.Defence.Heal(heal * deltaTime, false);
            }
            return true;
        }

        private static void Strike(in SpellRun run, CreatureRig rig, int age, float reach)
        {
            var cast = run.Cast;
            var shake = cast.Blow(cast.Damage, cast.Stagger, 1);
            shake.Affliction = AfflictionKind.Tremor;
            if (rig != null) rig.Play("oak_strike");
            if (!OakRules.StrikesAll(age))
            {
                var nearest = cast.Combat.EnemyNear(run.A, reach, null);
                if (nearest == null) return;
                cast.Combat.StrikeOne(nearest, shake, run.A);
                Root(cast, nearest.transform.position);
                return;
            }
            // The old oak: every enemy in reach, shaken and Seeded.
            var once = SpellWorks.Once(run);
            once.Clear();
            cast.Combat.StrikeArea(run.A, reach, shake, once);
            var seed = cast.Blow(0f, 0f, 1);
            seed.Affliction = AfflictionKind.Seeded;
            cast.Combat.StrikeArea(run.A, reach, seed);
            for (int i = 0; i < 6; i++) Root(cast, run.A + Quaternion.Euler(0f, i * 60f, 0f) * Vector3.forward * (reach * 0.6f));
        }

        // A root bursting up out of the ground.
        private static void Root(in SpellCast cast, Vector3 at) =>
            FormFx.Erupt(FormMesh.Spike, at, Quaternion.Euler(Random.Range(-15f, 15f), Random.value * 360f, 0f), new Vector3(0.6f, 1.3f, 0.6f), cast.Colour, 0.08f, 0.3f, 0.3f);

        private static void Show(in SpellRun run, CreatureRig rig, int age)
        {
            if (rig != null) return;
            float tall = age == 1 ? 1.25f : age == 2 ? 2.26f : 3.66f;
            var solids = SpellWorks.Solids(run);
            FormFx.Place(solids[0], run.A, Quaternion.identity, new Vector3(0.5f + 0.15f * age, tall * 0.7f, 0.5f + 0.15f * age));
            FormFx.Place(solids[1], run.A + Vector3.up * (tall * 0.8f), Quaternion.Euler(0f, run.Age * 6f, 0f), Vector3.one * (0.6f * age));
        }

        public override void End(ref SpellRun run)
        {
            int pillar = (int)SpellWorks.Times(run)[PillarNote] - 1;
            if (pillar >= 0) StandingThings.End(pillar, false);
            SpellWorks.Times(run)[PillarNote] = 0f;
            CreatureLibrary.Unmake(SpellWorks.Things(run)[0] as CreatureRig);
            SpellWorks.Things(run)[0] = null;
        }
    }
}

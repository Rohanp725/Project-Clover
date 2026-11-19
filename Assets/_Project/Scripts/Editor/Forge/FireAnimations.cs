using System;
using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// The animations of the Fire land's four ordinary enemies, written as poses on the shared skeleton and baked with
    /// the castle's by Clover > Forge > Bake Enemy Animations. Each moves as what it is: the thirsting starved and
    /// desperate, stooped and quick; the wick stiff and smouldering, then suddenly fast; the gilded soldier heavy and
    /// proud, the gold on his spear side dragging at him; the priest swaying with his face to the sky.
    /// </summary>
    public static class FireAnimations
    {
        public static IEnumerable<ForgeClip> For(EnemyKind kind)
        {
            switch (kind.Prefix)
            {
                case "thirst": return Thirst(kind);
                case "wick": return Wick(kind);
                case "ashen": return Ashen(kind);
                case "sunblind": return SunBlind(kind).Where(clip => clip.Name != "sunblind_beam").Concat(new[] { Borrowed(kind, EnemyKind.SunBlind(), "sunblind_beam", "sunblind_ray", SunBlind) });
                // The three ranks share one set of clips; the warden has every attack, so his bake is the set.
                case "gilded": return kind.Name == EnemyKind.GildedWardenName
                    ? Gilded(kind).Where(clip => clip.Name != "gilded_pour" && clip.Name != "gilded_cast").Concat(new[] { Borrowed(kind, EnemyKind.Gilded(3), "gilded_pour", "gilded_plant", Gilded), Borrowed(EnemyKind.Gilded(2), EnemyKind.Gilded(3), "gilded_cast", "gilded_lunge", Gilded) })
                    : Enumerable.Empty<ForgeClip>();
                case "champion": return Champion(kind);
                case "sunking": return SunKingAnimations.For(kind);
                default: return Enumerable.Empty<ForgeClip>();
            }
        }

        // The champion is the gilded soldier wholly cast: he moves as they move, on his own (larger) body and to his
        // own timings. Each of his clips is the gilded clip named beside it, made again with his attack's seconds.
        private static readonly (string mine, string theirs)[] ChampionMoves =
        {
            ("champion_thrust_1", "gilded_thrust_1"), ("champion_thrust_2", "gilded_thrust_2"), ("champion_thrust_3", "gilded_thrust_2"),
            ("champion_sweep", "gilded_sweep"), ("champion_butt", "gilded_thrust"), ("champion_throw", "gilded_lunge"),
            ("champion_plant", "gilded_plant"), ("champion_reap", "gilded_sweep"), ("champion_reap_thrust", "gilded_thrust_2"),
            ("champion_throw_twice", "gilded_lunge"), ("champion_throw_again", "gilded_lunge"),
        };

        private static IEnumerable<ForgeClip> Champion(EnemyKind kind)
        {
            foreach (var clip in Gilded(EnemyKind.Gilded(3)))
            {
                string rest = clip.Name.Substring("gilded".Length);
                if (rest != "_idle_loop" && rest != "_walk_loop" && rest != "_hurt" && rest != "_stagger" && rest != "_death" && !IsDownClip(rest)) continue;
                clip.Name = "champion" + rest;
                yield return clip;
            }
            foreach (var (mine, theirs) in ChampionMoves)
            {
                var own = Named(kind, mine);
                var standIn = EnemyKind.Gilded(3);
                var borrowed = Named(standIn, theirs);
                borrowed.Telegraph = own.Telegraph;
                borrowed.Strike = own.Strike;
                borrowed.Recovery = own.Recovery;
                var clip = Gilded(standIn).First(made => made.Name == theirs);
                clip.Name = mine;
                yield return clip;
            }
        }

        // A move of the second pass made from one of the first: the same motion, made again to the new attack's own seconds.
        private static ForgeClip Borrowed(EnemyKind kind, EnemyKind standIn, string mine, string theirs, Func<EnemyKind, IEnumerable<ForgeClip>> make)
        {
            var own = Named(kind, mine);
            var borrowed = Named(standIn, theirs);
            borrowed.Telegraph = own.Telegraph;
            borrowed.Strike = own.Strike;
            borrowed.Recovery = own.Recovery;
            var clip = make(standIn).First(made => made.Name == theirs);
            clip.Name = mine;
            return clip;
        }

        private static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);
        private static EnemyAttack Named(EnemyKind kind, string name) => kind.Brain.Attacks.Find(a => a.Name == name);
        private static ForgeClip Attack(EnemyKind kind, string name, out float hit, out float done) => EnemyAnimations.AttackClip(Named(kind, name), out hit, out done);

        // The flinch, the broken balance and being knocked flat are the shared ones, made from each kind's own stance; the
        // rest is written here.
        private static IEnumerable<ForgeClip> HurtAndStagger(string prefix, Func<ForgePose> ready, ForgePose kneel) =>
            EnemyAnimations.CommonClips(prefix, ready, 0.2f, 1f, kneel).Where(clip => clip.Name.EndsWith("_hurt") || clip.Name.EndsWith("_stagger") || IsDownClip(clip.Name));

        private static bool IsDownClip(string name) => name.EndsWith("_knockdown") || name.EndsWith("_down_loop") || name.EndsWith("_getup");

        // Down on both knees, sitting back on the heels.
        private static ForgePose OnKnees(ForgePose from)
        {
            var p = from.Copy();
            p.Hips = V(0f, -0.56f, 0f);
            p.HipLean = 6f;
            p.HipYaw = 0f;
            p.HipRoll = 0f;
            p.FootL = V(-0.17f, 0f, -0.32f);
            p.FootR = V(0.17f, 0f, -0.3f);
            p.FootLTip = 70f;
            p.FootRTip = 70f;
            p.FootLTurn = 4f;
            p.FootRTurn = 4f;
            p.KneeL = V(-0.2f, -0.3f, 1f);
            p.KneeR = V(0.2f, -0.3f, 1f);
            return p;
        }

        // ------------------------------------------------------------------ the thirsting

        // Dried out standing up: stooped over its own hollow belly, head craned forward, long hands half raised and never still.
        private static ForgePose ThirstReady() => new ForgePose
        {
            Hips = V(0f, -0.24f, -0.04f),
            HipLean = 20f,
            WaistLean = 34f,
            HeadLean = -34f,
            FootL = V(-0.21f, 0f, 0.14f),
            FootR = V(0.2f, 0f, -0.12f),
            FootLTurn = 14f,
            FootRTurn = 18f,
            HandL = V(-0.3f, 0.98f, 0.5f),
            HandR = V(0.32f, 0.9f, 0.44f),
            ElbowL = V(-1f, -0.7f, -0.2f),
            ElbowR = V(1f, -0.7f, -0.2f),
            HandLBend = -35f,
            HandRBend = -35f,
            ShoulderLUp = 12f,
            ShoulderRUp = 12f,
            ShoulderLForward = 14f,
            ShoulderRForward = 14f,
        };

        private static IEnumerable<ForgeClip> Thirst(EnemyKind kind)
        {
            var kneel = OnKnees(ThirstReady());
            kneel.WaistLean = 40f;
            kneel.HeadLean = 24f;
            kneel.HandL = V(-0.26f, 0.2f, 0.42f);
            kneel.HandR = V(0.26f, 0.24f, 0.36f);
            foreach (var clip in HurtAndStagger("thirst", ThirstReady, kneel)) yield return clip;

            // It cannot stand still: it pants, sways from the ankles, and the hands open and close on nothing.
            yield return new ForgeClip("thirst_idle_loop", 2.2f, true).From(t =>
            {
                float a = t * Mathf.PI * 2f;
                float sway = Mathf.Sin(a), pant = Mathf.Sin(a * 3f), shake = Mathf.Sin(a * 7f);
                var p = ThirstReady();
                p.Hips += V(0.05f * sway, 0.02f * pant, 0.03f * Mathf.Cos(a));
                p.HipRoll = 5f * sway;
                p.WaistLean += 5f * pant + 4f * Mathf.Cos(a);
                p.WaistRoll = -6f * sway;
                p.WaistYaw = 8f * Mathf.Sin(a + 0.8f);
                p.HeadYaw = 22f * Mathf.Sin(a + 1.6f);
                p.HeadRoll = 10f * Mathf.Sin(a * 2f);
                p.HeadLean += 6f * pant;
                p.HandL += V(0.03f * shake, 0.06f * Mathf.Sin(a + 1f), 0.07f * Mathf.Sin(a * 2f));
                p.HandR += V(-0.03f * shake, 0.07f * Mathf.Sin(a + 2.4f), 0.06f * Mathf.Cos(a * 2f));
                p.HandLBend = -35f + 25f * Mathf.Sin(a * 2f);
                p.HandRBend = -35f + 25f * Mathf.Cos(a * 2f);
                return p;
            });

            // A quick stooped scuttle: short fast steps, the body low and thrown forward, the hands clawing the air ahead in turn.
            yield return new ForgeClip("thirst_walk_loop", 0.52f, true) { Wind = _ => V(0f, 0.4f, -5f) }.From(t =>
            {
                float a = t * Mathf.PI * 2f;
                float left = Mathf.Sin(a), right = -left;
                var p = ThirstReady();
                p.Hips = V(0.04f * left, -0.3f + 0.045f * Mathf.Cos(a * 2f), 0.06f);
                p.HipLean = 28f;
                p.WaistLean = 40f;
                p.HeadLean = -44f;
                p.FootL = V(-0.2f, 0.12f * Mathf.Max(0f, Mathf.Cos(a)), 0.3f * left);
                p.FootR = V(0.2f, 0.12f * Mathf.Max(0f, -Mathf.Cos(a)), 0.3f * right);
                p.FootLTip = 26f * Mathf.Max(0f, -left);
                p.FootRTip = 26f * Mathf.Max(0f, -right);
                p.HipYaw = 12f * left;
                p.WaistYaw = -22f * left;
                p.HipRoll = 6f * left;
                p.HeadYaw = 10f * left;
                p.HeadRoll = 6f * Mathf.Sin(a * 2f);
                p.HandL = V(-0.3f, 1.0f + 0.1f * right, 0.56f + 0.22f * right);
                p.HandR = V(0.3f, 0.96f + 0.1f * left, 0.52f + 0.22f * left);
                return p;
            });

            // The two claws: slow and plain, each a whole arm thrown across with the body falling in behind it.
            {
                var clip = Attack(kind, "thirst_claw_1", out float hit, out float done);
                var back = ThirstReady();
                back.Hips = V(0.06f, -0.2f, -0.14f);
                back.HipYaw = 22f;
                back.WaistYaw = 42f;
                back.WaistLean = 16f;
                back.WaistRoll = -8f;
                back.HeadYaw = -40f;
                back.HeadLean = -20f;
                back.HandR = V(0.72f, 1.62f, -0.3f);
                back.ElbowR = V(1f, 0.2f, -0.6f);
                back.ShoulderRUp = 34f;
                back.HandRBend = -60f;
                back.HandL = V(-0.2f, 1.1f, 0.6f);
                var held = back.Copy();
                held.Hips = V(0.08f, -0.24f, -0.2f);
                held.WaistYaw = 50f;
                held.HandR = V(0.78f, 1.7f, -0.4f);
                var rake = ThirstReady();
                rake.Hips = V(-0.06f, -0.36f, 0.26f);
                rake.HipYaw = -16f;
                rake.HipLean = 30f;
                rake.WaistYaw = -46f;
                rake.WaistLean = 44f;
                rake.WaistRoll = 10f;
                rake.HeadYaw = 34f;
                rake.HeadLean = -40f;
                rake.FootR = V(0.2f, 0f, 0.34f);
                rake.FootL = V(-0.22f, 0f, -0.16f);
                rake.FootLTip = 30f;
                rake.HandR = V(-0.36f, 0.72f, 0.74f);
                rake.ElbowR = V(0.6f, -0.2f, -1f);
                rake.ShoulderRForward = 30f;
                rake.HandRBend = 30f;
                rake.HandL = V(-0.5f, 1.05f, -0.1f);
                var through = rake.Copy();
                through.WaistYaw = -58f;
                through.HandR = V(-0.56f, 0.6f, 0.5f);
                yield return clip.Key(0f, ThirstReady()).Key(hit * 0.5f, back, Ease.Out).Key(hit * 0.86f, held, Ease.InOut).Key(hit, rake, Ease.In)
                    .Key(done, through, Ease.Out).Key(1f, through, Ease.Linear);
            }
            {
                var clip = Attack(kind, "thirst_claw_2", out float hit, out float done);
                var from = ThirstReady();
                from.Hips = V(-0.06f, -0.36f, 0.26f);
                from.HipYaw = -16f;
                from.WaistYaw = -58f;
                from.WaistLean = 44f;
                from.FootR = V(0.2f, 0f, 0.34f);
                from.FootL = V(-0.22f, 0f, -0.16f);
                from.HandR = V(-0.56f, 0.6f, 0.5f);
                from.HandL = V(-0.5f, 1.05f, -0.1f);
                var back = from.Copy();
                back.Hips = V(-0.1f, -0.26f, 0.1f);
                back.WaistYaw = -64f;
                back.WaistLean = 20f;
                back.HeadYaw = 44f;
                back.HandL = V(-0.76f, 1.66f, -0.34f);
                back.ElbowL = V(-1f, 0.2f, -0.6f);
                back.ShoulderLUp = 34f;
                back.HandLBend = -60f;
                back.HandR = V(0.1f, 0.9f, 0.5f);
                var rake = ThirstReady();
                rake.Hips = V(0.08f, -0.4f, 0.4f);
                rake.HipYaw = 18f;
                rake.HipLean = 32f;
                rake.WaistYaw = 50f;
                rake.WaistLean = 46f;
                rake.WaistRoll = -10f;
                rake.HeadYaw = -36f;
                rake.HeadLean = -40f;
                rake.FootL = V(-0.2f, 0f, 0.5f);
                rake.FootR = V(0.22f, 0f, -0.06f);
                rake.FootRTip = 34f;
                rake.HandL = V(0.4f, 0.66f, 0.76f);
                rake.ElbowL = V(-0.6f, -0.2f, -1f);
                rake.ShoulderLForward = 30f;
                rake.HandLBend = 30f;
                rake.HandR = V(0.56f, 1.0f, -0.16f);
                var spent = rake.Copy();
                spent.Hips = V(0.1f, -0.5f, 0.42f);
                spent.WaistYaw = 62f;
                spent.WaistLean = 56f;
                spent.HeadLean = -20f;
                spent.HandL = V(0.56f, 0.4f, 0.5f);
                var heave = ThirstReady();
                heave.Hips = V(0.04f, -0.34f, 0.2f);
                heave.WaistLean = 46f;
                heave.HeadLean = -10f;
                yield return clip.Key(0f, from).Key(hit * 0.6f, back, Ease.Out).Key(hit, rake, Ease.In).Key(done, spent, Ease.Out)
                    .Key(Mathf.Lerp(done, 1f, 0.55f), heave, Ease.InOut).Key(1f, ThirstReady(), Ease.InOut);
            }

            // The red pounce: down on all fours like something about to spring, a long held tremble, then flat through the air.
            {
                var clip = Attack(kind, "thirst_pounce", out float hit, out float done);
                var crouch = ThirstReady();
                crouch.Hips = V(0f, -0.56f, -0.2f);
                crouch.HipLean = 44f;
                crouch.WaistLean = 30f;
                crouch.HeadLean = -60f;
                crouch.FootL = V(-0.26f, 0f, 0.0f);
                crouch.FootR = V(0.26f, 0f, -0.3f);
                crouch.FootRTip = 40f;
                crouch.HandL = V(-0.34f, 0.1f, 0.5f);
                crouch.HandR = V(0.34f, 0.1f, 0.42f);
                crouch.ElbowL = V(-1f, 0.4f, -0.2f);
                crouch.ElbowR = V(1f, 0.4f, -0.2f);
                var coil = crouch.Copy();
                coil.Hips = V(0f, -0.62f, -0.34f);
                coil.HipLean = 50f;
                coil.HeadLean = -66f;
                var air = new ForgePose
                {
                    Hips = V(0f, -0.1f, 0.3f),
                    HipLean = 62f,
                    WaistLean = 6f,
                    HeadLean = -60f,
                    FootL = V(-0.16f, 0.5f, -0.7f),
                    FootR = V(0.16f, 0.62f, -0.5f),
                    FootLTip = 80f,
                    FootRTip = 80f,
                    HandL = V(-0.26f, 1.08f, 1.2f),
                    HandR = V(0.26f, 0.98f, 1.24f),
                    ElbowL = V(-1f, 0.2f, 0f),
                    ElbowR = V(1f, 0.2f, 0f),
                    ShoulderLUp = 30f,
                    ShoulderRUp = 30f,
                    HandLBend = 40f,
                    HandRBend = 40f,
                };
                var land = crouch.Copy();
                land.Hips = V(0f, -0.6f, 0.22f);
                land.HipLean = 50f;
                land.WaistLean = 36f;
                land.HeadLean = -30f;
                land.FootL = V(-0.24f, 0f, -0.2f);
                land.FootR = V(0.24f, 0f, -0.36f);
                land.FootLTip = 50f;
                land.FootRTip = 50f;
                land.HandL = V(-0.3f, 0.08f, 0.86f);
                land.HandR = V(0.3f, 0.08f, 0.8f);
                var sag = land.Copy();
                sag.Hips = V(0f, -0.66f, 0.26f);
                sag.HeadLean = 10f;
                sag.WaistLean = 44f;
                var impact = clip.Shape;
                clip.Shape = (t, p) =>
                {
                    p = impact(t, p);
                    // The tremble before it goes.
                    if (t > hit * 0.3f && t < hit * 0.78f)
                    {
                        float shake = Mathf.Sin(t * 260f) * 0.012f;
                        p.Hips += V(shake, 0f, -shake);
                        p.HeadRoll += shake * 500f;
                    }
                    return p;
                };
                yield return clip.Key(0f, ThirstReady()).Key(hit * 0.3f, crouch, Ease.Out).Key(hit * 0.78f, coil, Ease.InOut).Key(hit * 0.92f, air, Ease.Out)
                    .Key(hit, air, Ease.Linear).Key(done, land, Ease.In).Key(Mathf.Lerp(done, 1f, 0.5f), sag, Ease.Out).Key(1f, ThirstReady(), Ease.InOut);
            }

            // The snatch: one long arm reaching low for the belt, palm up, the whole body thrown after it.
            {
                var clip = Attack(kind, "thirst_snatch", out float hit, out float done);
                var want = ThirstReady();
                want.Hips = V(0.04f, -0.4f, -0.16f);
                want.HipLean = 30f;
                want.WaistLean = 40f;
                want.WaistYaw = 30f;
                want.HeadLean = -52f;
                want.HeadYaw = -26f;
                want.HandR = V(0.46f, 0.62f, -0.2f);
                want.HandRBend = 60f;
                want.HandRTwist = 80f;
                want.HandL = V(-0.2f, 1.1f, 0.66f);
                var reach = ThirstReady();
                reach.Hips = V(-0.04f, -0.46f, 0.4f);
                reach.HipLean = 40f;
                reach.HipYaw = -14f;
                reach.WaistLean = 44f;
                reach.WaistYaw = -40f;
                reach.HeadLean = -56f;
                reach.HeadYaw = 30f;
                reach.FootR = V(0.2f, 0f, 0.6f);
                reach.FootL = V(-0.22f, 0f, -0.3f);
                reach.FootLTip = 44f;
                reach.HandR = V(0.02f, 0.82f, 1.22f);
                reach.ElbowR = V(1f, -0.4f, 0f);
                reach.ShoulderRForward = 36f;
                reach.HandRBend = 60f;
                reach.HandRTwist = 80f;
                reach.HandL = V(-0.56f, 0.9f, -0.2f);
                var clutch = ThirstReady();
                clutch.Hips = V(0f, -0.3f, 0.3f);
                clutch.WaistLean = 30f;
                clutch.HeadLean = -10f;
                clutch.FootR = V(0.2f, 0f, 0.44f);
                clutch.FootL = V(-0.22f, 0f, -0.1f);
                clutch.HandR = V(0.08f, 1.16f, 0.3f);
                clutch.HandL = V(-0.06f, 1.1f, 0.32f);
                yield return clip.Key(0f, ThirstReady()).Key(hit * 0.55f, want, Ease.Out).Key(hit * 0.8f, want, Ease.Linear).Key(hit, reach, Ease.In)
                    .Key(done, reach, Ease.Linear).Key(1f, clutch, Ease.InOut);
            }

            // It drinks: both hands to the mouth, head thrown right back, swaying with it. Helpless.
            yield return new ForgeClip("thirst_drink", kind.DrinkSeconds) { SheetMoments = new[] { 0f, 0.1f, 0.25f, 0.4f, 0.55f, 0.7f, 0.85f, 1f } }.From(t =>
            {
                float up = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.14f)) * (1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.9f) / 0.1f)));
                float gulp = Mathf.Sin(t * Mathf.PI * 2f * 5f);
                var drink = ThirstReady();
                drink.Hips = V(0.03f * Mathf.Sin(t * 9f), -0.12f + 0.015f * gulp, -0.08f);
                drink.HipLean = -4f;
                drink.WaistLean = -22f + 3f * gulp;
                drink.WaistRoll = 5f * Mathf.Sin(t * 9f);
                drink.HeadLean = -58f - 5f * gulp;
                drink.FootL = V(-0.24f, 0f, 0.06f);
                drink.FootR = V(0.24f, 0f, -0.06f);
                drink.HandL = V(-0.1f, 1.72f, 0.2f);
                drink.HandR = V(0.1f, 1.76f, 0.22f);
                drink.ElbowL = V(-1f, 0.1f, 0.3f);
                drink.ElbowR = V(1f, 0.1f, 0.3f);
                drink.ShoulderLUp = 24f;
                drink.ShoulderRUp = 24f;
                var from = ThirstReady();
                from.Hips = V(0f, -0.3f, 0.3f);
                from.HandR = V(0.08f, 1.16f, 0.3f);
                from.HandL = V(-0.06f, 1.1f, 0.32f);
                return ForgePose.Blend(from, drink, up);
            });

            // On its knees at a dry pool, scraping at the mud with both hands.
            ForgePose Scrape(float t)
            {
                float a = t * Mathf.PI * 2f;
                var p = OnKnees(ThirstReady());
                p.Hips = V(0f, -0.58f, 0.02f * Mathf.Sin(a));
                p.HipLean = 24f;
                p.WaistLean = 50f + 6f * Mathf.Sin(a * 2f);
                p.WaistYaw = 10f * Mathf.Sin(a);
                p.HeadLean = 16f;
                p.HeadYaw = 8f * Mathf.Sin(a);
                p.HandL = V(-0.2f, 0.06f + 0.05f * Mathf.Max(0f, Mathf.Sin(a)), 0.62f + 0.16f * Mathf.Cos(a));
                p.HandR = V(0.2f, 0.06f + 0.05f * Mathf.Max(0f, -Mathf.Sin(a)), 0.62f - 0.16f * Mathf.Cos(a));
                p.ElbowL = V(-1f, 0.3f, -0.2f);
                p.ElbowR = V(1f, 0.3f, -0.2f);
                return p;
            }
            yield return new ForgeClip("thirst_kneel_loop", 1.3f, true).From(Scrape);
            {
                var startled = OnKnees(ThirstReady());
                startled.Hips = V(0f, -0.46f, -0.06f);
                startled.WaistLean = 10f;
                startled.WaistYaw = 40f;
                startled.HeadYaw = 50f;
                startled.HeadLean = -30f;
                startled.HandL = V(-0.36f, 0.6f, 0.3f);
                startled.HandR = V(0.4f, 0.8f, 0.1f);
                yield return new ForgeClip("thirst_rise", kind.RiseSeconds).Key(0f, Scrape(0f)).Key(0.4f, startled, Ease.Snap).Key(1f, ThirstReady(), Ease.InOut);
            }

            // It folds and breaks like dry clay: stiffens upright, cracks at the waist, and drops in a heap on itself.
            {
                var crack = ThirstReady();
                crack.Hips = V(0f, -0.02f, -0.12f);
                crack.HipLean = -12f;
                crack.WaistLean = -34f;
                crack.HeadLean = -50f;
                crack.HandL = V(-0.7f, 1.3f, -0.2f);
                crack.HandR = V(0.72f, 1.2f, -0.16f);
                crack.ShoulderLUp = 30f;
                crack.ShoulderRUp = 30f;
                var buckle = OnKnees(ThirstReady());
                buckle.Hips = V(0.04f, -0.5f, 0f);
                buckle.WaistLean = -10f;
                buckle.WaistRoll = 14f;
                buckle.HeadLean = -20f;
                buckle.HeadRoll = 20f;
                buckle.HandL = V(-0.5f, 0.6f, 0f);
                buckle.HandR = V(0.5f, 0.5f, 0f);
                var fold = OnKnees(ThirstReady());
                fold.Hips = V(0.06f, -0.66f, -0.04f);
                fold.HipLean = 30f;
                fold.WaistLean = 84f;
                fold.WaistRoll = 12f;
                fold.HeadLean = 40f;
                fold.HandL = V(-0.4f, 0.06f, 0.3f);
                fold.HandR = V(0.46f, 0.06f, 0.2f);
                fold.ElbowL = V(-1f, 0.2f, 0f);
                fold.ElbowR = V(1f, 0.2f, 0f);
                var heap = fold.Copy();
                heap.Hips = V(0.12f, -0.72f, -0.06f);
                heap.HipRoll = -16f;
                heap.WaistLean = 96f;
                heap.WaistRoll = 26f;
                heap.HeadLean = 50f;
                heap.HeadRoll = 30f;
                heap.HandL = V(-0.5f, 0.04f, 0.16f);
                heap.HandR = V(0.56f, 0.04f, 0.02f);
                yield return new ForgeClip("thirst_death", 0.8f) { SheetMoments = new[] { 0f, 0.12f, 0.25f, 0.4f, 0.55f, 0.7f, 0.85f, 1f } }
                    .Key(0f, ThirstReady()).Key(0.16f, crack, Ease.Snap).Key(0.3f, crack, Ease.Linear).Key(0.52f, buckle, Ease.In)
                    .Key(0.78f, fold, Ease.In).Key(1f, heap, Ease.Out);
            }
        }

        // ------------------------------------------------------------------ the wick

        // Wrapped and upright, arms a little out from the body, palms forward, head tipped: a thing that was laid out and stood up.
        private static ForgePose WickReady() => new ForgePose
        {
            Hips = V(0f, -0.03f, 0f),
            HipLean = -2f,
            WaistLean = -5f,
            HeadLean = 8f,
            HeadRoll = 9f,
            FootL = V(-0.12f, 0f, 0.02f),
            FootR = V(0.12f, 0f, -0.02f),
            FootLTurn = 4f,
            FootRTurn = 4f,
            HandL = V(-0.52f, 0.98f, 0.16f),
            HandR = V(0.52f, 0.98f, 0.16f),
            ElbowL = V(-1f, -0.2f, -0.5f),
            ElbowR = V(1f, -0.2f, -0.5f),
            HandLBend = -50f,
            HandRBend = -50f,
        };

        // Arms crossed on the chest and the head bowed, as it was wrapped.
        private static ForgePose WickLaidOut()
        {
            var p = WickReady();
            p.Hips = V(0f, -0.01f, 0f);
            p.WaistLean = 0f;
            p.HeadLean = 22f;
            p.HeadRoll = 0f;
            p.HandL = V(0.13f, 1.36f, 0.2f);
            p.HandR = V(-0.13f, 1.3f, 0.23f);
            p.ElbowL = V(-1f, -1f, 0.1f);
            p.ElbowR = V(1f, -1f, 0.1f);
            p.HandLBend = 0f;
            p.HandRBend = 0f;
            return p;
        }

        // Laid on its back on a table, feet toward where it faces.
        private static ForgePose WickLying() => new ForgePose
        {
            Hips = V(0f, -0.84f, 0f),
            HipLean = -88f,
            HeadLean = 6f,
            FootL = V(-0.11f, 0.06f, 0.86f),
            FootR = V(0.11f, 0.06f, 0.86f),
            FootLTip = -60f,
            FootRTip = -60f,
            KneeL = V(-0.1f, 1f, 0.2f),
            KneeR = V(0.1f, 1f, 0.2f),
            HandL = V(0.1f, 0.34f, -0.36f),
            HandR = V(-0.1f, 0.34f, -0.42f),
            ElbowL = V(-1f, -0.6f, 0f),
            ElbowR = V(1f, -0.6f, 0f),
        };

        private static IEnumerable<ForgeClip> Wick(EnemyKind kind)
        {
            var kneel = OnKnees(WickReady());
            kneel.WaistLean = 30f;
            kneel.HeadLean = 34f;
            kneel.HandL = V(-0.4f, 0.4f, 0.2f);
            kneel.HandR = V(0.4f, 0.4f, 0.2f);
            foreach (var clip in HurtAndStagger("wick", WickReady, kneel)) yield return clip;

            // Almost still. It smoulders: a slow lean round its feet like a flame in no wind, and now and then the head jerks.
            yield return new ForgeClip("wick_idle_loop", 3.6f, true).From(t =>
            {
                float a = t * Mathf.PI * 2f;
                float jerk = Mathf.Exp(-Mathf.Pow((t - 0.62f) / 0.035f, 2f));
                var p = WickReady();
                p.Hips += V(0.02f * Mathf.Sin(a), 0f, 0.02f * Mathf.Cos(a));
                p.HipRoll = 2f * Mathf.Sin(a);
                p.WaistLean += 3f * Mathf.Cos(a);
                p.WaistRoll = 3f * Mathf.Sin(a);
                p.HeadRoll = 9f - 22f * jerk;
                p.HeadYaw = 5f * Mathf.Sin(a) + 14f * jerk;
                p.HandL += V(0f, 0.02f * Mathf.Sin(a + 1f) + 0.05f * jerk, 0f);
                p.HandR += V(0f, 0.02f * Mathf.Sin(a + 2f) + 0.05f * jerk, 0f);
                return p;
            });

            // A slow straight-legged walk, arms out: the whole body rocks over each stiff leg and nothing swings.
            yield return new ForgeClip("wick_walk_loop", 1.3f, true) { Wind = _ => V(0f, 0.6f, -2f) }.From(t =>
            {
                float a = t * Mathf.PI * 2f;
                float left = Mathf.Sin(a), right = -left;
                var p = WickReady();
                p.Hips = V(0.07f * left, -0.02f + 0.012f * Mathf.Cos(a * 2f), 0.02f);
                p.HipRoll = 8f * left;
                p.WaistRoll = -3f * left;
                p.HipYaw = 7f * left;
                p.WaistYaw = 3f * left;
                p.FootL = V(-0.13f, 0.05f * Mathf.Max(0f, Mathf.Cos(a)), 0.18f * left);
                p.FootR = V(0.13f, 0.05f * Mathf.Max(0f, -Mathf.Cos(a)), 0.18f * right);
                p.FootLTip = 8f * Mathf.Max(0f, -left);
                p.FootRTip = 8f * Mathf.Max(0f, -right);
                p.HeadRoll = 9f + 5f * left;
                p.HandL = V(-0.56f, 1.04f + 0.03f * left, 0.3f);
                p.HandR = V(0.56f, 1.04f + 0.03f * right, 0.3f);
                return p;
            });

            // The swipe: the stiff arm carried back by the whole trunk, held, and whipped across level.
            {
                var clip = Attack(kind, "wick_swipe", out float hit, out float done);
                var back = WickReady();
                back.HipYaw = 16f;
                back.WaistYaw = 48f;
                back.WaistLean = -8f;
                back.HeadYaw = -46f;
                back.HandR = V(0.86f, 1.34f, -0.42f);
                back.ElbowR = V(1f, 0.3f, -0.6f);
                back.ShoulderRUp = 20f;
                back.HandL = V(-0.4f, 1.1f, 0.4f);
                var held = back.Copy();
                held.WaistYaw = 56f;
                held.HandR = V(0.9f, 1.4f, -0.52f);
                var cut = WickReady();
                cut.Hips = V(-0.04f, -0.14f, 0.2f);
                cut.HipYaw = -14f;
                cut.WaistYaw = -50f;
                cut.WaistLean = 18f;
                cut.HeadYaw = 40f;
                cut.FootR = V(0.14f, 0f, 0.36f);
                cut.FootL = V(-0.14f, 0f, -0.12f);
                cut.HandR = V(-0.5f, 1.16f, 0.7f);
                cut.ElbowR = V(0.4f, 0.2f, -1f);
                cut.ShoulderRForward = 34f;
                cut.HandL = V(-0.66f, 0.96f, -0.2f);
                var through = cut.Copy();
                through.WaistYaw = -64f;
                through.HandR = V(-0.72f, 1.1f, 0.36f);
                yield return clip.Key(0f, WickReady()).Key(hit * 0.55f, back, Ease.InOut).Key(hit * 0.86f, held, Ease.InOut).Key(hit, cut, Ease.In)
                    .Key(done, through, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), through, Ease.Linear).Key(1f, WickReady(), Ease.InOut);
            }

            // The slam: both arms hauled up over the head with the back arched, and brought down together.
            {
                var clip = Attack(kind, "wick_slam", out float hit, out float done);
                var up = WickReady();
                up.Hips = V(0f, 0.02f, -0.08f);
                up.HipLean = -8f;
                up.WaistLean = -24f;
                up.HeadLean = -20f;
                up.HandL = V(-0.3f, 2.24f, -0.06f);
                up.HandR = V(0.3f, 2.24f, -0.06f);
                up.ElbowL = V(-1f, 0.2f, 0.3f);
                up.ElbowR = V(1f, 0.2f, 0.3f);
                up.ShoulderLUp = 36f;
                up.ShoulderRUp = 36f;
                var top = up.Copy();
                top.Hips = V(0f, 0.04f, -0.14f);
                top.WaistLean = -30f;
                top.HandL = V(-0.32f, 2.26f, -0.2f);
                top.HandR = V(0.32f, 2.26f, -0.2f);
                var down = WickReady();
                down.Hips = V(0f, -0.34f, 0.2f);
                down.HipLean = 20f;
                down.WaistLean = 50f;
                down.HeadLean = -10f;
                down.FootL = V(-0.2f, 0f, 0.2f);
                down.FootR = V(0.2f, 0f, -0.1f);
                down.HandL = V(-0.24f, 0.5f, 0.86f);
                down.HandR = V(0.24f, 0.5f, 0.86f);
                down.ElbowL = V(-1f, 0.3f, 0f);
                down.ElbowR = V(1f, 0.3f, 0f);
                var sunk = down.Copy();
                sunk.Hips = V(0f, -0.4f, 0.24f);
                sunk.WaistLean = 58f;
                sunk.HandL = V(-0.26f, 0.36f, 0.86f);
                sunk.HandR = V(0.26f, 0.36f, 0.86f);
                yield return clip.Key(0f, WickReady()).Key(hit * 0.5f, up, Ease.InOut).Key(hit * 0.88f, top, Ease.InOut).Key(hit, down, Ease.In)
                    .Key(done, sunk, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), sunk, Ease.Linear).Key(1f, WickReady(), Ease.InOut);
            }

            // The red grab. The arms open slowly, wide, as if in welcome, and it leans back; then it is suddenly fast:
            // the whole body thrown forward and the arms shut. Missed, it goes on past its feet and stumbles.
            {
                var clip = Attack(kind, "wick_grab", out float hit, out float done);
                var open = WickReady();
                open.Hips = V(0f, 0f, -0.1f);
                open.HipLean = -6f;
                open.WaistLean = -16f;
                open.HeadLean = -8f;
                open.HeadRoll = 20f;
                open.HandL = V(-0.98f, 1.36f, 0.06f);
                open.HandR = V(0.98f, 1.36f, 0.06f);
                open.ElbowL = V(-1f, -0.5f, -0.4f);
                open.ElbowR = V(1f, -0.5f, -0.4f);
                open.ShoulderLUp = 16f;
                open.ShoulderRUp = 16f;
                open.HandLBend = -70f;
                open.HandRBend = -70f;
                var wide = open.Copy();
                wide.Hips = V(0f, 0.01f, -0.16f);
                wide.WaistLean = -22f;
                wide.HeadRoll = -14f;
                wide.HandL = V(-1.04f, 1.44f, -0.06f);
                wide.HandR = V(1.04f, 1.44f, -0.06f);
                var shut = WickReady();
                shut.Hips = V(0f, -0.2f, 0.44f);
                shut.HipLean = 24f;
                shut.WaistLean = 34f;
                shut.HeadLean = -30f;
                shut.HeadRoll = 0f;
                shut.FootL = V(-0.14f, 0f, 0.7f);
                shut.FootR = V(0.14f, 0f, -0.26f);
                shut.FootRTip = 40f;
                shut.HandL = V(-0.2f, 1.26f, 1.16f);
                shut.HandR = V(0.2f, 1.2f, 1.16f);
                shut.ElbowL = V(-1f, 0.1f, 0f);
                shut.ElbowR = V(1f, 0.1f, 0f);
                shut.ShoulderLForward = 34f;
                shut.ShoulderRForward = 34f;
                shut.HandLBend = 30f;
                shut.HandRBend = 30f;
                var hug = shut.Copy();
                hug.HandL = V(0.16f, 1.2f, 0.9f);
                hug.HandR = V(-0.16f, 1.12f, 0.92f);
                var stumble = shut.Copy();
                stumble.Hips = V(0.1f, -0.3f, 0.56f);
                stumble.HipLean = 26f;
                stumble.WaistLean = 40f;
                stumble.WaistRoll = -14f;
                stumble.HeadLean = 10f;
                stumble.FootR = V(0.2f, 0f, 0.3f);
                stumble.HandL = V(0.1f, 0.6f, 1.0f);
                stumble.HandR = V(0.4f, 0.5f, 0.9f);
                var catchSelf = WickReady();
                catchSelf.Hips = V(0.04f, -0.26f, 0.3f);
                catchSelf.WaistLean = 30f;
                catchSelf.FootL = V(-0.14f, 0f, 0.5f);
                catchSelf.FootR = V(0.2f, 0f, 0.14f);
                var impact = clip.Shape;
                clip.Shape = (t, p) =>
                {
                    p = impact(t, p);
                    // It shivers with wanting while the arms are open.
                    if (t > hit * 0.35f && t < hit * 0.78f)
                    {
                        float shake = Mathf.Sin(t * 300f) * 0.01f;
                        p.HandL += V(0f, shake, 0f);
                        p.HandR += V(0f, -shake, 0f);
                        p.HeadYaw += shake * 400f;
                    }
                    return p;
                };
                yield return clip.Key(0f, WickReady()).Key(hit * 0.45f, open, Ease.InOut).Key(hit * 0.78f, wide, Ease.InOut).Key(hit * 0.94f, shut, Ease.In)
                    .Key(hit, hug, Ease.Out).Key(done, hug, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.35f), stumble, Ease.Out)
                    .Key(Mathf.Lerp(done, 1f, 0.7f), catchSelf, Ease.InOut).Key(1f, WickReady(), Ease.InOut);
            }

            // Holding: arms shut round what it caught, bent over it, straining and shaking with each pulse of the burning.
            yield return new ForgeClip("wick_hold_loop", 0.8f, true).From(t =>
            {
                float a = t * Mathf.PI * 2f;
                float strain = Mathf.Sin(a), shake = Mathf.Sin(a * 5f);
                var p = WickReady();
                p.Hips = V(0.02f * strain, -0.2f, 0.44f);
                p.HipLean = 24f;
                p.WaistLean = 36f + 5f * strain;
                p.WaistRoll = 4f * shake;
                p.HeadLean = 20f + 6f * strain;
                p.HeadRoll = 8f * shake;
                p.FootL = V(-0.14f, 0f, 0.7f);
                p.FootR = V(0.14f, 0f, -0.26f);
                p.FootRTip = 40f;
                p.HandL = V(0.18f + 0.02f * shake, 1.2f, 0.9f - 0.04f * strain);
                p.HandR = V(-0.18f - 0.02f * shake, 1.12f, 0.92f - 0.04f * strain);
                p.ElbowL = V(-1f, 0.1f, 0f);
                p.ElbowR = V(1f, 0.1f, 0f);
                p.ShoulderLForward = 34f;
                p.ShoulderRForward = 34f;
                return p;
            });

            // Thrown off: arms flung wide, reeling back over its heels, two stiff steps to stay up.
            {
                var flung = WickReady();
                flung.Hips = V(0f, -0.06f, -0.2f);
                flung.HipLean = -14f;
                flung.WaistLean = -34f;
                flung.HeadLean = -30f;
                flung.FootL = V(-0.16f, 0f, 0.3f);
                flung.FootR = V(0.16f, 0.1f, -0.3f);
                flung.HandL = V(-0.9f, 1.7f, 0.1f);
                flung.HandR = V(0.9f, 1.6f, 0.2f);
                flung.ShoulderLUp = 30f;
                flung.ShoulderRUp = 30f;
                var reel = flung.Copy();
                reel.Hips = V(-0.08f, -0.14f, -0.3f);
                reel.WaistRoll = 14f;
                reel.FootL = V(-0.2f, 0.1f, -0.1f);
                reel.FootR = V(0.16f, 0f, -0.4f);
                reel.HandL = V(-0.96f, 1.3f, -0.1f);
                reel.HandR = V(0.8f, 1.2f, 0.3f);
                var steady = WickReady();
                steady.Hips = V(0f, -0.1f, -0.1f);
                steady.WaistLean = 8f;
                steady.HeadLean = 20f;
                yield return new ForgeClip("wick_thrown_off", kind.ThrownOffSeconds).Key(0f, flung, Ease.Snap).Key(0.3f, flung, Ease.Linear).Key(0.55f, reel, Ease.InOut)
                    .Key(0.8f, steady, Ease.InOut).Key(1f, WickReady(), Ease.InOut);
            }

            // Dormant, standing in a niche or a doorway: laid out upright, a breath of movement and no more.
            yield return new ForgeClip("wick_dormant_loop", 3f, true).From(t =>
            {
                var p = WickLaidOut();
                p.WaistLean = 1.5f * Mathf.Sin(t * Mathf.PI * 2f);
                return p;
            });
            // Dormant, lying wrapped on a table.
            yield return new ForgeClip("wick_lie_loop", 3f, true).From(t =>
            {
                var p = WickLying();
                p.Hips += V(0f, 0.008f * Mathf.Sin(t * Mathf.PI * 2f), 0f);
                return p;
            });
            // Rising from where it stood: the head comes up first, with a jerk; then the arms unfold, stiffly, one after the other.
            {
                var head = WickLaidOut();
                head.HeadLean = -10f;
                head.HeadRoll = 16f;
                head.HeadYaw = 12f;
                var one = head.Copy();
                one.Hips = V(0.03f, -0.03f, 0.04f);
                one.WaistRoll = -6f;
                one.HandL = V(-0.6f, 1.2f, 0.34f);
                one.ElbowL = V(-1f, -0.2f, -0.5f);
                one.HandLBend = -50f;
                var two = WickReady();
                two.Hips = V(0f, -0.05f, 0.1f);
                two.WaistLean = 8f;
                two.HandL = V(-0.6f, 1.16f, 0.4f);
                two.HandR = V(0.6f, 1.2f, 0.4f);
                yield return new ForgeClip("wick_rise", kind.RiseSeconds) { SheetMoments = new[] { 0f, 0.15f, 0.3f, 0.45f, 0.6f, 0.75f, 0.9f, 1f } }
                    .Key(0f, WickLaidOut()).Key(0.14f, WickLaidOut(), Ease.Linear).Key(0.26f, head, Ease.Snap).Key(0.5f, one, Ease.InOut)
                    .Key(0.8f, two, Ease.InOut).Key(1f, WickReady(), Ease.InOut);
            }
            // Rising from a table: it sits up straight from the waist, as nothing living does, and swings down onto its feet.
            {
                var sit = WickLying();
                sit.Hips = V(0f, -0.8f, 0f);
                sit.HipLean = -20f;
                sit.WaistLean = 10f;
                sit.HeadLean = 10f;
                sit.HandL = V(-0.4f, 0.5f, 0.3f);
                sit.HandR = V(0.4f, 0.5f, 0.3f);
                var crouch = OnKnees(WickReady());
                crouch.Hips = V(0f, -0.5f, 0.1f);
                crouch.WaistLean = 30f;
                crouch.FootL = V(-0.14f, 0f, 0.3f);
                crouch.FootLTip = 0f;
                crouch.KneeL = V(-0.2f, 0f, 1f);
                crouch.HandL = V(-0.4f, 0.5f, 0.4f);
                crouch.HandR = V(0.4f, 0.5f, 0.4f);
                yield return new ForgeClip("wick_rise_lying", kind.RiseSeconds)
                    .Key(0f, WickLying()).Key(0.12f, WickLying(), Ease.Linear).Key(0.42f, sit, Ease.Snap).Key(0.72f, crouch, Ease.InOut).Key(1f, WickReady(), Ease.InOut);
            }

            // At last consumed: to its knees, the arms drawn in, and it burns down on itself to a black stump.
            {
                var flare = WickReady();
                flare.Hips = V(0f, 0.02f, -0.06f);
                flare.WaistLean = -20f;
                flare.HeadLean = -34f;
                flare.HandL = V(-0.8f, 1.5f, 0f);
                flare.HandR = V(0.8f, 1.5f, 0f);
                var knees = OnKnees(WickReady());
                knees.Hips = V(0f, -0.52f, 0f);
                knees.WaistLean = 6f;
                knees.HeadLean = -20f;
                knees.HandL = V(-0.5f, 0.7f, 0.1f);
                knees.HandR = V(0.5f, 0.7f, 0.1f);
                var curl = OnKnees(WickReady());
                curl.Hips = V(0f, -0.62f, -0.04f);
                curl.HipLean = 16f;
                curl.WaistLean = 60f;
                curl.HeadLean = 46f;
                curl.HandL = V(0.1f, 0.5f, 0.24f);
                curl.HandR = V(-0.1f, 0.46f, 0.26f);
                curl.ElbowL = V(-1f, -1f, 0f);
                curl.ElbowR = V(1f, -1f, 0f);
                var stump = curl.Copy();
                stump.Hips = V(0f, -0.7f, -0.06f);
                stump.WaistLean = 76f;
                stump.HeadLean = 56f;
                stump.HandL = V(0.08f, 0.34f, 0.2f);
                stump.HandR = V(-0.08f, 0.3f, 0.22f);
                yield return new ForgeClip("wick_death", 0.8f) { SheetMoments = new[] { 0f, 0.12f, 0.25f, 0.4f, 0.55f, 0.7f, 0.85f, 1f } }
                    .Key(0f, WickReady()).Key(0.14f, flare, Ease.Snap).Key(0.28f, flare, Ease.Linear).Key(0.52f, knees, Ease.In)
                    .Key(0.78f, curl, Ease.InOut).Key(1f, stump, Ease.Out);
            }
        }

        // ------------------------------------------------------------------ the ashen

        // A wick burned down to its last: hunched round the core in its chest, one arm hanging dead, the other held
        // out for balance, head lolling. It never quite stands straight.
        private static ForgePose AshenReady()
        {
            var p = WickReady();
            p.Hips = V(0f, -0.1f, 0.04f);
            p.HipLean = 10f;
            p.WaistLean = 22f;
            p.WaistRoll = -6f;
            p.HeadLean = 18f;
            p.HeadRoll = 16f;
            p.FootL = V(-0.16f, 0f, 0.08f);
            p.FootR = V(0.15f, 0f, -0.1f);
            p.HandL = V(-0.3f, 0.66f, 0.12f);
            p.ElbowL = V(-1f, -0.6f, 0f);
            p.HandR = V(0.6f, 1.02f, 0.32f);
            p.HandLBend = 10f;
            return p;
        }

        private static IEnumerable<ForgeClip> Ashen(EnemyKind kind)
        {
            var kneel = OnKnees(AshenReady());
            kneel.WaistLean = 40f;
            kneel.HeadLean = 30f;
            foreach (var clip in HurtAndStagger("ashen", AshenReady, kneel)) yield return clip;

            // Smouldering where it stands: it sways and jerks as the core flares inside it.
            yield return new ForgeClip("ashen_idle_loop", 2.4f, true).From(t =>
            {
                float a = t * Mathf.PI * 2f;
                float jerk = Mathf.Exp(-Mathf.Pow((t - 0.4f) / 0.03f, 2f)) + Mathf.Exp(-Mathf.Pow((t - 0.83f) / 0.025f, 2f));
                var p = AshenReady();
                p.Hips += V(0.03f * Mathf.Sin(a), 0.01f * Mathf.Sin(a * 2f), 0f);
                p.WaistRoll += 4f * Mathf.Sin(a) - 10f * jerk;
                p.HeadRoll += -20f * jerk;
                p.HeadYaw = 12f * jerk;
                p.HandR += V(0f, 0.05f * jerk, 0f);
                return p;
            });

            // The stumbling run: fast, uneven, thrown forward and always about to fall, the dead arm swinging loose
            // and the good one flung out to catch itself. One step is longer than the other.
            yield return new ForgeClip("ashen_walk_loop", 0.62f, true) { Wind = _ => V(0f, 0.6f, -3f) }.From(t =>
            {
                float a = t * Mathf.PI * 2f;
                // The left step is snatched short; the right one lurches long.
                float s = Mathf.Sin(a + 0.35f * Mathf.Sin(a));
                float left = s, right = -s;
                var p = AshenReady();
                p.Hips = V(0.06f * left, -0.16f + 0.05f * Mathf.Abs(Mathf.Cos(a)), 0.12f);
                p.HipLean = 22f;
                p.WaistLean = 30f + 6f * Mathf.Cos(a * 2f);
                p.HipRoll = 10f * left;
                p.HipYaw = 12f * left;
                p.WaistYaw = -10f * left;
                p.FootL = V(-0.15f, 0.14f * Mathf.Max(0f, Mathf.Cos(a)), 0.38f * left);
                p.FootR = V(0.15f, 0.16f * Mathf.Max(0f, -Mathf.Cos(a)), 0.46f * right);
                p.FootLTip = 30f * Mathf.Max(0f, -left);
                p.FootRTip = 30f * Mathf.Max(0f, -right);
                p.HeadLean = 4f + 10f * Mathf.Cos(a * 2f);
                p.HeadRoll = 16f + 8f * left;
                p.HandL = V(-0.24f + 0.08f * right, 0.6f, 0.1f + 0.3f * right);
                p.HandR = V(0.7f, 1.12f + 0.12f * left, 0.5f + 0.1f * right);
                return p;
            });

            // The swell: it stops dead, the arms are dragged back and wide, the chest thrown out round the core, the head
            // goes back, and it rises on its toes, shaking harder and harder. Then the burst flings everything open.
            {
                var clip = Attack(kind, "ashen_swell", out float hit, out float done);
                var catchUp = AshenReady();
                catchUp.Hips = V(0f, -0.2f, -0.06f);
                catchUp.WaistLean = 10f;
                catchUp.FootL = V(-0.24f, 0f, 0.12f);
                catchUp.FootR = V(0.24f, 0f, -0.12f);
                var open = AshenReady();
                open.Hips = V(0f, -0.02f, -0.1f);
                open.HipLean = -8f;
                open.WaistLean = -26f;
                open.WaistRoll = 0f;
                open.HeadLean = -36f;
                open.HeadRoll = 0f;
                open.FootL = V(-0.24f, 0f, 0.06f);
                open.FootR = V(0.24f, 0f, -0.06f);
                open.HandL = V(-0.86f, 1.18f, -0.36f);
                open.HandR = V(0.86f, 1.22f, -0.36f);
                open.ElbowL = V(-1f, -0.3f, -0.5f);
                open.ElbowR = V(1f, -0.3f, -0.5f);
                open.ShoulderLUp = 18f;
                open.ShoulderRUp = 18f;
                open.HandLBend = -60f;
                open.HandRBend = -60f;
                var toes = open.Copy();
                toes.Hips = V(0f, 0.08f, -0.12f);
                toes.WaistLean = -34f;
                toes.HeadLean = -48f;
                toes.FootLTip = -30f;
                toes.FootRTip = -30f;
                toes.HandL = V(-0.98f, 1.36f, -0.44f);
                toes.HandR = V(0.98f, 1.4f, -0.44f);
                var burst = open.Copy();
                burst.Hips = V(0f, 0.12f, 0f);
                burst.WaistLean = -10f;
                burst.HeadLean = -20f;
                burst.HandL = V(-1.1f, 1.9f, 0.2f);
                burst.HandR = V(1.1f, 1.9f, 0.2f);
                burst.ShoulderLUp = 40f;
                burst.ShoulderRUp = 40f;
                var impact = clip.Shape;
                clip.Shape = (t, p) =>
                {
                    p = impact(t, p);
                    // The shaking grows with the swell.
                    if (t < hit)
                    {
                        float shake = Mathf.Sin(t * 260f) * 0.025f * (t / hit);
                        p.Hips += V(shake, 0f, 0f);
                        p.HandL += V(0f, shake, 0f);
                        p.HandR += V(0f, -shake, 0f);
                        p.HeadRoll += shake * 500f;
                    }
                    return p;
                };
                yield return clip.Key(0f, AshenReady()).Key(hit * 0.2f, catchUp, Ease.Snap).Key(hit * 0.55f, open, Ease.InOut).Key(hit * 0.95f, toes, Ease.InOut)
                    .Key(hit, burst, Ease.Snap).Key(1f, burst, Ease.Linear);
            }

            // Struck down before its time: it drops to its knees clutching at the core, and the shell sags in on it.
            {
                var knees = OnKnees(AshenReady());
                knees.Hips = V(0f, -0.54f, 0.06f);
                knees.WaistLean = 30f;
                knees.HeadLean = 30f;
                knees.HandL = V(-0.12f, 1.1f, 0.34f);
                knees.HandR = V(0.12f, 1.04f, 0.36f);
                knees.ElbowL = V(-1f, -1f, 0f);
                knees.ElbowR = V(1f, -1f, 0f);
                var sag = knees.Copy();
                sag.Hips = V(0f, -0.66f, 0.1f);
                sag.WaistLean = 58f;
                sag.HeadLean = 50f;
                sag.HandL = V(-0.1f, 0.6f, 0.4f);
                sag.HandR = V(0.1f, 0.56f, 0.42f);
                yield return new ForgeClip("ashen_death", 0.8f) { SheetMoments = new[] { 0f, 0.15f, 0.3f, 0.45f, 0.6f, 0.75f, 0.9f, 1f } }
                    .Key(0f, AshenReady()).Key(0.4f, knees, Ease.In).Key(1f, sag, Ease.Out);
            }

            // Dormant, as a wick is, and up quicker.
            yield return new ForgeClip("ashen_dormant_loop", 3f, true).From(t =>
            {
                var p = WickLaidOut();
                p.WaistLean = 8f + 2f * Mathf.Sin(t * Mathf.PI * 2f);
                return p;
            });
            yield return new ForgeClip("ashen_rise", kind.RiseSeconds).Key(0f, WickLaidOut()).Key(0.4f, AshenReady(), Ease.Snap).Key(1f, AshenReady(), Ease.Linear);
        }

        // ------------------------------------------------------------------ the gilded soldier

        // Upright and proud, spear grounded in the gold arm, shield across the body. The gold side hangs a little lower.
        private static ForgePose GildedReady() => new ForgePose
        {
            Hips = V(0.02f, -0.03f, 0f),
            HipRoll = -3f,
            HipYaw = 12f,
            WaistYaw = -6f,
            WaistLean = -7f,
            HeadLean = -5f,
            HeadYaw = -6f,
            FootL = V(-0.17f, 0f, 0.16f),
            FootR = V(0.2f, 0f, -0.1f),
            FootLTurn = 2f,
            FootRTurn = 26f,
            HandR = V(0.5f, 1.06f, 0.16f),
            ElbowR = V(1f, -0.3f, -0.6f),
            ShoulderRUp = -8f,
            AimBlade = true,
            Blade = V(0.03f, 1f, 0.04f),
            HandL = V(-0.2f, 1.14f, 0.4f),
            ElbowL = V(-1f, -0.5f, -0.1f),
            ShoulderLForward = 10f,
        };

        // The spear levelled at the hip behind the shield.
        private static ForgePose GildedLevel()
        {
            var p = GildedReady();
            p.Hips = V(0.02f, -0.14f, -0.04f);
            p.HipYaw = 30f;
            p.WaistYaw = 4f;
            p.WaistLean = 4f;
            p.FootL = V(-0.16f, 0f, 0.3f);
            p.FootR = V(0.22f, 0f, -0.24f);
            p.FootRTurn = 44f;
            p.HandR = V(0.4f, 1.1f, -0.1f);
            p.ElbowR = V(0.8f, 0.2f, -1f);
            p.Blade = V(-0.14f, 0.02f, 1f);
            p.HandL = V(-0.16f, 1.2f, 0.46f);
            p.HeadYaw = -30f;
            return p;
        }

        private static ForgePose GildedThrustOut(float reach, float low)
        {
            var p = GildedLevel();
            p.Hips = V(-0.02f, -0.3f - low, 0.34f);
            p.HipYaw = 0f;
            p.HipLean = 14f;
            p.WaistYaw = -36f;
            p.WaistLean = 20f;
            p.FootL = V(-0.16f, 0f, 0.74f);
            p.FootR = V(0.22f, 0f, -0.3f);
            p.FootRTip = 30f;
            p.HandR = V(0.04f, 1.14f - low, 0.84f + reach);
            p.ElbowR = V(1f, -0.3f, 0f);
            p.ShoulderRForward = 32f;
            p.Blade = V(-0.02f, -0.03f, 1f);
            p.HandL = V(-0.4f, 1.1f, 0.2f);
            p.HeadYaw = 30f;
            p.HeadLean = 8f;
            return p;
        }

        private static IEnumerable<ForgeClip> Gilded(EnemyKind kind)
        {
            var kneel = OnKnees(GildedReady());
            // Down on the gold side's knee, held up by the spear.
            kneel.Hips = V(0.04f, -0.5f, 0f);
            kneel.HipYaw = 0f;
            kneel.FootL = V(-0.18f, 0f, 0.28f);
            kneel.FootLTip = 0f;
            kneel.KneeL = V(-0.2f, 0f, 1f);
            kneel.WaistLean = 26f;
            kneel.HeadLean = 30f;
            kneel.HandR = V(0.34f, 1.0f, 0.36f);
            kneel.Blade = V(-0.04f, 1f, 0.08f);
            kneel.HandL = V(-0.3f, 0.6f, 0.3f);
            foreach (var clip in HurtAndStagger("gilded", GildedReady, kneel)) yield return clip;

            // Still as a statue. Only the weight settles, slowly, toward the gold.
            yield return new ForgeClip("gilded_idle_loop", 4f, true).From(t =>
            {
                float a = t * Mathf.PI * 2f;
                float settle = 0.5f - 0.5f * Mathf.Cos(a);
                var p = GildedReady();
                p.Hips += V(0.02f * settle, -0.012f * settle + 0.006f * Mathf.Sin(a * 2f), 0f);
                p.HipRoll -= 2.5f * settle;
                p.WaistRoll = 2f * settle;
                p.WaistLean += 1.5f * Mathf.Sin(a * 2f);
                p.ShoulderRUp -= 4f * settle;
                p.HeadYaw += 5f * Mathf.Sin(a);
                p.HandR += V(0f, -0.02f * settle, 0f);
                return p;
            });

            // He walks the way a statue would: the bare leg steps, the gold leg is brought up after it, and the spear
            // is set down like a staff with every pace.
            yield return new ForgeClip("gilded_walk_loop", 1.15f, true) { Wind = _ => V(0f, 0.3f, -3f) }.From(t =>
            {
                float a = t * Mathf.PI * 2f;
                float left = Mathf.Sin(a), right = -left;
                var p = GildedReady();
                // The left foot strides out; the right is dragged level with it and no farther.
                p.FootL = V(-0.17f, 0.11f * Mathf.Max(0f, Mathf.Cos(a)), 0.1f + 0.26f * left);
                p.FootR = V(0.2f, 0.03f * Mathf.Max(0f, -Mathf.Cos(a)), -0.06f + 0.14f * right);
                p.FootLTip = 18f * Mathf.Max(0f, -left);
                p.FootRTip = 6f;
                p.Hips = V(0.02f + 0.05f * right, -0.05f - 0.035f * Mathf.Max(0f, right), 0.02f);
                p.HipRoll = -3f - 7f * Mathf.Max(0f, right) + 3f * Mathf.Max(0f, left);
                p.HipYaw = 12f + 9f * left;
                p.WaistYaw = -6f - 10f * left;
                p.WaistRoll = 3f * right;
                p.WaistLean = -5f;
                p.HeadYaw = -6f + 4f * left;
                // The spear lifts as the bare leg goes out and is planted as the gold leg comes up.
                p.HandR = V(0.5f, 1.08f + 0.07f * Mathf.Max(0f, left), 0.2f + 0.1f * left);
                p.Blade = V(0.03f, 1f, 0.04f - 0.1f * left);
                p.HandL = V(-0.2f, 1.14f + 0.02f * right, 0.4f);
                return p;
            });

            // The thrust: the spear drawn back level behind the shield, and driven out with the weight of the gold behind it.
            ForgeClip Thrust(string name, float reach, float low, bool second)
            {
                var clip = Attack(kind, name, out float hit, out float done);
                var draw = GildedLevel();
                draw.Hips = V(0.05f, -0.2f, -0.18f);
                draw.HipYaw = 48f;
                draw.WaistYaw = 26f;
                draw.HandR = V(0.5f, 1.14f - low, -0.5f);
                draw.Blade = V(-0.26f, 0.04f, 0.96f);
                draw.HandL = V(-0.1f, 1.24f, 0.56f);
                var held = draw.Copy();
                held.Hips = V(0.07f, -0.27f, -0.27f);
                held.WaistYaw = 38f;
                held.HandR = V(0.54f, 1.1f - low, -0.64f);
                var drive = GildedThrustOut(reach, low);
                var stuck = drive.Copy();
                stuck.Hips += V(0f, -0.06f, 0.08f);
                stuck.WaistYaw = -46f;
                stuck.HandR += V(-0.03f, -0.05f, 0.1f);
                var haul = GildedLevel();
                haul.Hips = V(0.03f, -0.1f, 0f);
                haul.FootL = V(-0.16f, 0f, 0.46f);
                // The second thrust starts where the first ended, and waits.
                var start = second ? GildedThrustOut(0f, 0f) : GildedReady();
                clip.Key(0f, start).Key(hit * (second ? 0.4f : 0.5f), draw, Ease.Out).Key(hit * 0.86f, held, Ease.InOut).Key(hit, drive, Ease.In).Key(done, stuck, Ease.Out);
                if (clip.Seconds - done * clip.Seconds < 0.2f) return clip.Key(1f, stuck, Ease.Linear);
                return clip.Key(Mathf.Lerp(done, 1f, 0.45f), stuck, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.8f), haul, Ease.InOut).Key(1f, GildedReady(), Ease.InOut);
            }
            yield return Thrust("gilded_thrust", 0f, 0f, false);
            yield return Thrust("gilded_thrust_1", 0f, 0f, false);
            yield return Thrust("gilded_thrust_2", 0.12f, 0.12f, true);

            // The sweep: the spear carried out to his left behind the shield, and swung flat through half a circle
            // by the turn of the whole trunk. The gold arm does not bend to do it.
            {
                var clip = Attack(kind, "gilded_sweep", out float hit, out float done);
                var wind = GildedLevel();
                wind.Hips = V(-0.04f, -0.16f, -0.06f);
                wind.HipYaw = -20f;
                wind.WaistYaw = -56f;
                wind.WaistLean = 6f;
                wind.HeadYaw = 50f;
                wind.HandR = V(-0.36f, 1.16f, 0.34f);
                wind.ElbowR = V(0.4f, -0.2f, -1f);
                wind.ShoulderRForward = 30f;
                wind.Blade = V(-0.94f, 0.02f, 0.3f);
                wind.HandL = V(-0.56f, 1.2f, -0.06f);
                var held = wind.Copy();
                held.WaistYaw = -66f;
                held.Blade = V(-0.98f, 0.04f, 0.1f);
                var across = GildedLevel();
                across.Hips = V(0.02f, -0.24f, 0.14f);
                across.HipYaw = 10f;
                across.WaistYaw = 10f;
                across.WaistLean = 12f;
                across.HeadYaw = -10f;
                across.FootL = V(-0.2f, 0f, 0.4f);
                across.HandR = V(0.3f, 1.08f, 0.62f);
                across.ElbowR = V(1f, -0.3f, 0f);
                across.Blade = V(0.3f, -0.03f, 0.95f);
                across.HandL = V(-0.5f, 1.14f, 0.2f);
                var through = across.Copy();
                through.HipYaw = 34f;
                through.WaistYaw = 58f;
                through.HeadYaw = -50f;
                through.HandR = V(0.7f, 1.04f, 0.1f);
                through.Blade = V(0.96f, -0.04f, -0.24f);
                var heavy = through.Copy();
                heavy.Hips = V(0.1f, -0.3f, 0.1f);
                heavy.HipRoll = -10f;
                heavy.WaistRoll = -8f;
                heavy.HandR = V(0.72f, 0.94f, -0.06f);
                heavy.Blade = V(0.9f, -0.2f, -0.4f);
                yield return clip.Key(0f, GildedReady()).Key(hit * 0.5f, wind, Ease.InOut).Key(hit * 0.86f, held, Ease.InOut).Key(hit, across, Ease.In)
                    .Key(done, through, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.4f), heavy, Ease.Out).Key(1f, GildedReady(), Ease.InOut);
            }

            // The red lunge: he sinks behind the shield with the spear couched, holds, and goes with everything.
            {
                var clip = Attack(kind, "gilded_lunge", out float hit, out float done);
                var couch = GildedLevel();
                couch.Hips = V(0.04f, -0.34f, -0.2f);
                couch.HipYaw = 40f;
                couch.HipLean = 10f;
                couch.WaistYaw = 20f;
                couch.WaistLean = 16f;
                couch.FootL = V(-0.16f, 0f, 0.36f);
                couch.FootR = V(0.24f, 0f, -0.4f);
                couch.HandR = V(0.46f, 1.0f, -0.4f);
                couch.Blade = V(-0.2f, 0.06f, 0.98f);
                couch.HandL = V(-0.06f, 1.2f, 0.56f);
                couch.HeadLean = 10f;
                var sunk = couch.Copy();
                sunk.Hips = V(0.06f, -0.42f, -0.3f);
                sunk.WaistYaw = 30f;
                sunk.HandR = V(0.5f, 0.94f, -0.54f);
                var drive = GildedThrustOut(0.2f, 0.14f);
                drive.Hips = V(-0.02f, -0.46f, 0.5f);
                drive.HipLean = 22f;
                drive.WaistLean = 30f;
                drive.FootL = V(-0.16f, 0f, 0.9f);
                drive.FootR = V(0.22f, 0f, -0.44f);
                drive.FootRTip = 50f;
                var spent = drive.Copy();
                spent.Hips = V(0f, -0.52f, 0.54f);
                spent.HipRoll = -8f;
                spent.HeadLean = 24f;
                spent.HandR += V(0f, -0.2f, 0f);
                spent.Blade = V(-0.02f, -0.24f, 0.97f);
                var haul = GildedLevel();
                haul.Hips = V(0.03f, -0.2f, 0.2f);
                haul.FootL = V(-0.16f, 0f, 0.6f);
                yield return clip.Key(0f, GildedReady()).Key(hit * 0.4f, couch, Ease.Out).Key(hit * 0.78f, sunk, Ease.InOut).Key(hit, drive, Ease.In)
                    .Key(done, spent, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), spent, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.85f), haul, Ease.InOut).Key(1f, GildedReady(), Ease.InOut);
            }

            // The planted spear: raised point down in the gold fist as high as he can reach, driven into the stone at
            // his feet, and then it will not come out: he hauls at it twice before it does.
            {
                var clip = Attack(kind, "gilded_plant", out float hit, out float done);
                var raise = GildedReady();
                raise.Hips = V(0f, 0.03f, -0.06f);
                raise.HipYaw = 0f;
                raise.WaistLean = -18f;
                raise.WaistYaw = 10f;
                raise.HeadLean = -16f;
                raise.HandR = V(0.34f, 2.16f, 0.1f);
                raise.ElbowR = V(1f, 0.3f, 0.2f);
                raise.ShoulderRUp = 40f;
                raise.Blade = V(0.04f, -1f, 0.1f);
                raise.HandL = V(-0.56f, 1.3f, 0.2f);
                var top = raise.Copy();
                top.Hips = V(0f, 0.05f, -0.1f);
                top.WaistLean = -24f;
                top.HandR = V(0.36f, 2.22f, 0f);
                var plant = GildedReady();
                plant.Hips = V(0f, -0.44f, 0.12f);
                plant.HipYaw = 0f;
                plant.HipLean = 14f;
                plant.WaistLean = 34f;
                plant.HeadLean = -14f;
                plant.FootL = V(-0.24f, 0f, 0.26f);
                plant.FootR = V(0.24f, 0f, -0.2f);
                plant.HandR = V(0.12f, 1.0f, 0.56f);
                plant.ElbowR = V(1f, 0.2f, -0.2f);
                plant.Blade = V(0f, -1f, 0.06f);
                plant.HandL = V(-0.7f, 1.0f, -0.1f);
                var tug = plant.Copy();
                tug.Hips = V(0f, -0.3f, -0.02f);
                tug.WaistLean = 14f;
                tug.HeadLean = 6f;
                tug.HandR = V(0.12f, 1.1f, 0.54f);
                tug.HandL = V(-0.2f, 1.0f, 0.5f);
                var free = GildedReady();
                free.Hips = V(0f, -0.06f, -0.12f);
                free.WaistLean = -12f;
                free.HandR = V(0.4f, 1.4f, 0.2f);
                free.Blade = V(0.04f, -1f, 0.1f);
                float rest = 1f - done;
                yield return clip.Key(0f, GildedReady()).Key(hit * 0.5f, raise, Ease.InOut).Key(hit * 0.88f, top, Ease.InOut).Key(hit, plant, Ease.In)
                    .Key(done, plant, Ease.Linear).Key(done + rest * 0.3f, tug, Ease.Out).Key(done + rest * 0.45f, plant, Ease.In)
                    .Key(done + rest * 0.65f, tug, Ease.Out).Key(done + rest * 0.85f, free, Ease.Snap).Key(1f, GildedReady(), Ease.InOut);
            }

            // He falls like a thing cast: he does not bend. He tips back past his heels, straight as he stood, and lands flat.
            {
                var teeter = GildedReady();
                teeter.Hips = V(0f, -0.02f, -0.14f);
                teeter.HipYaw = 0f;
                teeter.HipLean = -16f;
                teeter.WaistLean = 0f;
                teeter.WaistYaw = 0f;
                teeter.FootL = V(-0.15f, 0f, 0.06f);
                teeter.FootR = V(0.15f, 0f, 0.06f);
                teeter.FootLTip = -16f;
                teeter.FootRTip = -16f;
                teeter.HandR = V(0.5f, 1.02f, -0.12f);
                teeter.Blade = V(0.03f, 0.96f, -0.26f);
                teeter.HandL = V(-0.22f, 1.1f, 0.12f);
                var half = teeter.Copy();
                half.Hips = V(0f, -0.3f, -0.42f);
                half.HipLean = -48f;
                half.FootLTip = -48f;
                half.FootRTip = -48f;
                half.HandR = V(0.5f, 0.62f, -0.72f);
                half.Blade = V(0.03f, 0.66f, -0.74f);
                half.HandL = V(-0.22f, 0.92f, -0.56f);
                var flat = new ForgePose
                {
                    Hips = V(0f, -0.82f, -0.8f),
                    HipLean = -88f,
                    FootL = V(-0.15f, 0.07f, 0.1f),
                    FootR = V(0.15f, 0.07f, 0.1f),
                    FootLTip = -80f,
                    FootRTip = -80f,
                    KneeL = V(-0.1f, 1f, 0.1f),
                    KneeR = V(0.1f, 1f, 0.1f),
                    HandR = V(0.52f, 0.14f, -1.1f),
                    ElbowR = V(1f, -0.6f, 0f),
                    AimBlade = true,
                    Blade = V(0.03f, 0.05f, -1f),
                    HandL = V(-0.24f, 0.36f, -1.2f),
                    ElbowL = V(-1f, -0.5f, 0f),
                };
                var bounce = flat.Copy();
                bounce.Hips = V(0f, -0.76f, -0.8f);
                bounce.HipLean = -84f;
                yield return new ForgeClip("gilded_death", 0.8f) { SheetMoments = new[] { 0f, 0.15f, 0.3f, 0.45f, 0.6f, 0.75f, 0.9f, 1f } }
                    .Key(0f, GildedReady()).Key(0.22f, teeter, Ease.Out).Key(0.36f, teeter, Ease.Linear).Key(0.6f, half, Ease.In).Key(0.8f, flat, Ease.Linear)
                    .Key(0.9f, bounce, Ease.Out).Key(1f, flat, Ease.In);
            }
        }

        // ------------------------------------------------------------------ the sun-blind priest

        // Face to the sky (that is in the model), leaning back a little, the sun-staff upright in his right hand and
        // the left hand lifted open in praise.
        private static ForgePose PriestReady() => new ForgePose
        {
            Hips = V(0f, -0.02f, -0.02f),
            HipLean = -4f,
            WaistLean = -9f,
            HeadLean = -4f,
            FootL = V(-0.13f, 0f, 0.04f),
            FootR = V(0.14f, 0f, -0.04f),
            HandR = V(0.44f, 1.2f, 0.26f),
            ElbowR = V(1f, -0.6f, -0.3f),
            AimBlade = true,
            Blade = V(0.02f, 1f, 0.03f),
            HandL = V(-0.46f, 1.5f, 0.22f),
            ElbowL = V(-1f, -0.8f, 0f),
            ShoulderLUp = 14f,
            HandLBend = -50f,
        };

        private static IEnumerable<ForgeClip> SunBlind(EnemyKind kind)
        {
            var kneel = OnKnees(PriestReady());
            kneel.WaistLean = 20f;
            kneel.HeadLean = 20f;
            kneel.HandR = V(0.36f, 1.0f, 0.36f);
            kneel.Blade = V(-0.04f, 1f, 0.06f);
            kneel.HandL = V(-0.3f, 0.4f, 0.3f);
            foreach (var clip in HurtAndStagger("sunblind", PriestReady, kneel)) yield return clip;

            // He sways in a slow circle like a man in a trance, turning his blind face across the sky; the lifted hand rises and sinks.
            yield return new ForgeClip("sunblind_idle_loop", 4.2f, true).From(t =>
            {
                float a = t * Mathf.PI * 2f;
                var p = PriestReady();
                p.Hips += V(0.045f * Mathf.Sin(a), 0.01f * Mathf.Sin(a * 2f), 0.035f * Mathf.Cos(a));
                p.HipRoll = 3f * Mathf.Sin(a);
                p.WaistRoll = -6f * Mathf.Sin(a);
                p.WaistLean += 5f * Mathf.Cos(a);
                p.WaistYaw = 8f * Mathf.Sin(a + 0.6f);
                p.HeadYaw = 26f * Mathf.Sin(a + 1.2f);
                p.HeadRoll = 8f * Mathf.Cos(a);
                p.HeadLean += -5f * Mathf.Cos(a);
                p.HandL += V(0.04f * Mathf.Sin(a), 0.12f * Mathf.Sin(a + 0.5f), 0.04f * Mathf.Cos(a));
                p.HandR += V(0.02f * Mathf.Sin(a), 0.02f * Mathf.Cos(a), 0f);
                p.Blade = V(0.02f + 0.05f * Mathf.Sin(a), 1f, 0.03f + 0.05f * Mathf.Cos(a));
                return p;
            });

            // Feeling his way: short shuffling steps, the staff swung out ahead and set down twice a pace like a blind
            // man's stick, the body following it, the free hand groping from side to side in front of him.
            yield return new ForgeClip("sunblind_walk_loop", 1.2f, true) { Wind = _ => V(0f, 0.3f, -2f) }.From(t =>
            {
                float a = t * Mathf.PI * 2f;
                float left = Mathf.Sin(a), right = -left;
                // Twice a pace: out and up, then down and planted.
                float reach = 0.5f - 0.5f * Mathf.Cos(a * 2f);
                float lift = Mathf.Max(0f, Mathf.Sin(a * 2f));
                var p = PriestReady();
                p.Hips = V(0.04f * left, -0.07f + 0.03f * Mathf.Cos(a * 2f), 0.02f + 0.03f * reach);
                p.HipLean = 2f;
                p.WaistLean = -6f + 7f * reach;
                p.WaistRoll = -5f * left;
                p.HipYaw = 8f * left;
                p.WaistYaw = -10f * left;
                p.FootL = V(-0.13f, 0.06f * Mathf.Max(0f, Mathf.Cos(a)), 0.2f * left);
                p.FootR = V(0.14f, 0.06f * Mathf.Max(0f, -Mathf.Cos(a)), 0.2f * right);
                p.FootLTip = 12f * Mathf.Max(0f, -left);
                p.FootRTip = 12f * Mathf.Max(0f, -right);
                p.HeadYaw = 22f * Mathf.Sin(a + 0.4f);
                p.HeadRoll = 8f + 6f * Mathf.Cos(a);
                p.HandR = V(0.4f, 1.14f + 0.18f * lift, 0.3f + 0.34f * reach);
                p.Blade = V(0.02f, 1f, -0.55f + 0.5f * reach);
                p.ShoulderRForward = 16f * reach;
                p.HandL = V(-0.4f + 0.2f * left, 1.2f + 0.08f * Mathf.Sin(a * 2f + 1f), 0.56f + 0.1f * Mathf.Cos(a));
                p.ElbowL = V(-1f, -0.6f, -0.2f);
                p.HandLBend = -60f;
                return p;
            });

            // Searching: he stands where he lost them and sweeps the staff's foot in an arc over the ground, head cocked, listening with his skin.
            yield return new ForgeClip("sunblind_search_loop", 3f, true).From(t =>
            {
                float a = t * Mathf.PI * 2f;
                float sweep = Mathf.Sin(a);
                var p = PriestReady();
                p.Hips = V(0.03f * sweep, -0.12f, 0.04f);
                p.HipLean = 6f;
                p.WaistLean = 8f;
                p.WaistYaw = 34f * sweep;
                p.HipYaw = 10f * sweep;
                p.HeadYaw = -30f * sweep;
                p.HeadRoll = 14f * Mathf.Cos(a);
                p.FootL = V(-0.2f, 0f, 0.1f);
                p.FootR = V(0.2f, 0f, -0.06f);
                p.HandR = V(0.3f + 0.3f * sweep, 1.1f, 0.5f);
                p.Blade = V(-0.5f * sweep, 0.82f, -0.5f);
                p.HandL = V(-0.4f + 0.2f * sweep, 1.1f, 0.5f);
                p.ElbowL = V(-1f, -0.6f, -0.2f);
                p.HandLBend = -60f;
                return p;
            });

            // The ray: the sun on his staff lifted to the sun in the sky with both arms and his whole back, held there
            // while it fills, and brought down like a judgement at where they stood.
            {
                var clip = Attack(kind, "sunblind_ray", out float hit, out float done);
                var lift = PriestReady();
                lift.Hips = V(0f, 0.02f, -0.08f);
                lift.HipLean = -8f;
                lift.WaistLean = -24f;
                lift.HeadLean = -16f;
                lift.HandR = V(0.3f, 2.1f, -0.04f);
                lift.ElbowR = V(1f, 0.2f, 0.3f);
                lift.ShoulderRUp = 36f;
                lift.Blade = V(0f, 1f, -0.1f);
                lift.HandL = V(-0.5f, 2.0f, -0.06f);
                lift.ElbowL = V(-1f, 0.2f, 0.3f);
                lift.ShoulderLUp = 36f;
                var full = lift.Copy();
                full.Hips = V(0f, 0.05f, -0.14f);
                full.WaistLean = -32f;
                full.HandR = V(0.3f, 2.18f, -0.16f);
                full.HandL = V(-0.56f, 2.06f, -0.2f);
                var cast = PriestReady();
                cast.Hips = V(0f, -0.1f, 0.16f);
                cast.HipLean = 8f;
                cast.WaistLean = 22f;
                cast.HeadLean = -26f;
                cast.FootL = V(-0.14f, 0f, 0.42f);
                cast.FootR = V(0.16f, 0f, -0.2f);
                cast.HandR = V(0.2f, 1.36f, 0.7f);
                cast.ElbowR = V(1f, -0.2f, 0f);
                cast.ShoulderRForward = 26f;
                cast.Blade = V(0f, 0.34f, 0.94f);
                cast.HandL = V(-0.5f, 1.0f, 0.5f);
                var spent = cast.Copy();
                spent.Hips = V(0f, -0.14f, 0.18f);
                spent.Blade = V(0f, 0.16f, 0.98f);
                spent.HandR = V(0.2f, 1.26f, 0.74f);
                var impact = clip.Shape;
                clip.Shape = (t, p) =>
                {
                    p = impact(t, p);
                    if (t > hit * 0.45f && t < hit * 0.9f)
                    {
                        float shake = Mathf.Sin(t * 280f) * 0.012f;
                        p.HandR += V(shake, 0f, 0f);
                        p.HandL += V(-shake, 0f, 0f);
                    }
                    return p;
                };
                yield return clip.Key(0f, PriestReady()).Key(hit * 0.45f, lift, Ease.InOut).Key(hit * 0.9f, full, Ease.InOut).Key(hit, cast, Ease.In)
                    .Key(done, spent, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), spent, Ease.Linear).Key(1f, PriestReady(), Ease.InOut);
            }

            // The brand: the staff swung low and level, sun end first, from his right across his front.
            {
                var clip = Attack(kind, "sunblind_brand", out float hit, out float done);
                var wind = PriestReady();
                wind.Hips = V(0.04f, -0.16f, -0.08f);
                wind.HipYaw = 20f;
                wind.WaistYaw = 50f;
                wind.WaistLean = 6f;
                wind.HeadYaw = -40f;
                wind.HandR = V(0.6f, 1.0f, -0.3f);
                wind.ElbowR = V(1f, 0.2f, -0.8f);
                wind.Blade = V(0.86f, -0.16f, -0.5f);
                wind.HandL = V(-0.3f, 1.2f, 0.5f);
                var held = wind.Copy();
                held.WaistYaw = 60f;
                held.Blade = V(0.7f, -0.14f, -0.7f);
                var cut = PriestReady();
                cut.Hips = V(-0.02f, -0.26f, 0.2f);
                cut.HipYaw = -10f;
                cut.WaistYaw = -30f;
                cut.WaistLean = 20f;
                cut.HeadYaw = 20f;
                cut.FootR = V(0.16f, 0f, 0.36f);
                cut.FootL = V(-0.14f, 0f, -0.14f);
                cut.HandR = V(0.1f, 0.96f, 0.6f);
                cut.ElbowR = V(1f, -0.2f, -0.2f);
                cut.ShoulderRForward = 24f;
                cut.Blade = V(-0.2f, -0.2f, 0.95f);
                cut.HandL = V(-0.6f, 1.1f, 0f);
                var through = cut.Copy();
                through.WaistYaw = -58f;
                through.HandR = V(-0.3f, 0.94f, 0.4f);
                through.Blade = V(-0.94f, -0.14f, 0.3f);
                yield return clip.Key(0f, PriestReady()).Key(hit * 0.5f, wind, Ease.InOut).Key(hit * 0.86f, held, Ease.InOut).Key(hit, cut, Ease.In)
                    .Key(done, through, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), through, Ease.Linear).Key(1f, PriestReady(), Ease.InOut);
            }

            // The flare: he folds over the staff, hugging it, and then opens to the sky with it thrust straight up.
            {
                var clip = Attack(kind, "sunblind_flare", out float hit, out float done);
                var fold = PriestReady();
                fold.Hips = V(0f, -0.34f, 0f);
                fold.HipLean = 14f;
                fold.WaistLean = 34f;
                fold.HeadLean = 20f;
                fold.FootL = V(-0.2f, 0f, 0.06f);
                fold.FootR = V(0.2f, 0f, -0.06f);
                fold.HandR = V(0.12f, 1.0f, 0.34f);
                fold.Blade = V(0f, 1f, 0.1f);
                fold.TwoHanded = true;
                fold.GripGap = 0.3f;
                var tight = fold.Copy();
                tight.Hips = V(0f, -0.4f, 0f);
                tight.WaistLean = 42f;
                var open = PriestReady();
                open.Hips = V(0f, 0.04f, -0.08f);
                open.HipLean = -8f;
                open.WaistLean = -26f;
                open.HeadLean = -14f;
                open.FootL = V(-0.24f, 0f, 0.04f);
                open.FootR = V(0.24f, 0f, -0.04f);
                open.HandR = V(0.4f, 2.16f, 0f);
                open.ElbowR = V(1f, 0.2f, 0.2f);
                open.ShoulderRUp = 38f;
                open.Blade = V(0f, 1f, 0f);
                open.HandL = V(-0.9f, 1.7f, 0f);
                open.ElbowL = V(-0.3f, -1f, -0.3f);
                open.ShoulderLUp = 26f;
                var hold = open.Copy();
                hold.WaistLean = -20f;
                hold.HandL = V(-0.84f, 1.6f, 0.06f);
                yield return clip.Key(0f, PriestReady()).Key(hit * 0.5f, fold, Ease.InOut).Key(hit * 0.9f, tight, Ease.InOut).Key(hit, open, Ease.In)
                    .Key(done, hold, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), hold, Ease.Linear).Key(1f, PriestReady(), Ease.InOut);
            }

            // He kneels, face still up, the arms fall open and the staff goes over; then he sits back on his heels and is still.
            {
                var struck = PriestReady();
                struck.Hips = V(0f, -0.04f, -0.12f);
                struck.WaistLean = -22f;
                struck.HandL = V(-0.7f, 1.3f, 0f);
                struck.HandR = V(0.56f, 1.3f, 0.1f);
                struck.Blade = V(0.1f, 1f, -0.2f);
                var knees = OnKnees(PriestReady());
                knees.Hips = V(0f, -0.5f, 0f);
                knees.HipLean = -4f;
                knees.WaistLean = -12f;
                knees.HeadLean = -8f;
                knees.HandL = V(-0.6f, 0.8f, 0.1f);
                knees.HandR = V(0.6f, 0.9f, 0.16f);
                knees.Blade = V(0.3f, 0.9f, 0.2f);
                var open = knees.Copy();
                open.Hips = V(0f, -0.6f, -0.06f);
                open.WaistLean = -18f;
                open.HeadLean = -14f;
                open.HandL = V(-0.6f, 0.34f, 0.1f);
                open.HandR = V(0.62f, 0.3f, 0.14f);
                open.HandLBend = 60f;
                open.Blade = V(0.9f, 0.1f, 0.4f);
                var still = open.Copy();
                still.Hips = V(0f, -0.64f, -0.1f);
                still.WaistLean = -8f;
                still.WaistRoll = 5f;
                still.HeadLean = 4f;
                still.HandL = V(-0.5f, 0.22f, 0.16f);
                still.HandR = V(0.52f, 0.2f, 0.18f);
                still.Blade = V(0.95f, 0.02f, 0.3f);
                yield return new ForgeClip("sunblind_death", 0.8f) { SheetMoments = new[] { 0f, 0.12f, 0.25f, 0.4f, 0.55f, 0.7f, 0.85f, 1f } }
                    .Key(0f, PriestReady()).Key(0.16f, struck, Ease.Snap).Key(0.3f, struck, Ease.Linear).Key(0.55f, knees, Ease.In)
                    .Key(0.8f, open, Ease.Out).Key(1f, still, Ease.InOut);
            }
        }
    }
}

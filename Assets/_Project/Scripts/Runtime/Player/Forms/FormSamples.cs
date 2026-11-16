using System.Collections.Generic;

namespace Clover.Player.Forms
{
    /// <summary>
    /// One spell of each new form with sensible numbers and an element that suits it, on no tree: what the yard's rack
    /// of stones lends the player to try, and what the forms' probe casts. The trees write their own numbers; these
    /// only show what each form is like when its numbers are ordinary.
    /// </summary>
    public static class FormSamples
    {
        // What each sample is like, in the words the yard's cards show over its stone.
        private static readonly Dictionary<string, string> about = new Dictionary<string, string>
        {
            { SpellForms.Blades, "Blades appear over the enemy you face, hang a moment, then fall one after another." },
            { SpellForms.Sword, "One sword as tall as the screen falls where you aim. It leaves a crack in the ground that keeps biting." },
            { SpellForms.Beam, "You brace and throw a thick beam ahead. You can turn slowly while it lasts." },
            { SpellForms.Phalanx, "Blades hover behind your shoulders and fire by themselves at enemies near, or all at once when you land a heavy blow." },
            { SpellForms.Dragon, "A great head forms at your shoulder and lunges ahead, carrying what it meets, and bites at the end." },
            { SpellForms.Nova, "A burst round you after a short breath, with a ring that sweeps outward." },
            { SpellForms.Orb, "A slow orb follows the nearest enemy and bursts on it. Strike the orb yourself and it bursts at once, larger." },
            { SpellForms.Bomb, "You stand rooted and raise a growing sphere, then throw it: a great burst and a burning patch." },
            { SpellForms.Spikes, "Spikes run out along the ground ahead of you, one after another, in three rows." },
            { SpellForms.Field, "A patch of ground ahead grows thorns for a while and bites what stands in it." },
            { SpellForms.Drill, "You become a spinning point and bore through everything ahead, striking each enemy several times. Nothing can touch you on the way." },
            { SpellForms.Minions, "Small helpers rise beside you, run at the nearest enemy and strike it for a few seconds." },
            { SpellForms.Bullets, "A ring of bullets appears round the enemy you face, hangs, then all fly inward together." },
            { SpellForms.Reverse, "A point ahead pulls every enemy near it inward, then bursts." },
            { SpellForms.Parry, "A guard stance for a moment. The first blow that lands on it is turned and answered with a cone of light. If nothing strikes, the mana is lost." },
            { SpellForms.Wall, "A wall rises ahead and blocks the way. It bursts when its time ends or when you strike it with a heavy blow." },
            { SpellForms.Discs, "Discs circle you for a while and cut whatever they pass." },
            { SpellForms.Brand, "A sign is set on the enemy you face. Your next three weapon blows on it each set off a burst." },
            { SpellForms.Rain, "Strikes fall at random over an area ahead for a few seconds, each marked on the ground first." },
            { SpellForms.Clone, "A copy of you stands beside you and repeats every weapon blow a beat late, weaker." },
            { SpellForms.Flash, "You vanish, lines of light cross the enemy you face, and you reappear behind it. A moment later all the cuts land at once." },
            { SpellForms.Ring, "A ring closes round you and the enemy you face. Nothing crosses it, and it bites what touches it." },
            { SpellForms.Pillars, "Pillars burst from the ground under the nearest enemies and lift them for a moment." },
            { SpellForms.Step, "You step through the enemy you face and cut it in passing. If it lands, your next heavy blow soon after is critical." },
        };

        private static readonly Upgrade[] all =
        {
            Make(SpellForms.Blades, "Falling Blades", Elements.Metal, new SpellSpec { Cost = 30f, Count = 5, Delay = 0.7f, Radius = 1.3f, Damage = 16f, Stagger = 14f, Reach = 9f }),
            Make(SpellForms.Sword, "Sword of Day", Elements.Light, new SpellSpec { Cost = 45f, Delay = 1f, Radius = 3.2f, Damage = 70f, Stagger = 80f, Seconds = 4f, LingerDamage = 5f, Reach = 9f }),
            Make(SpellForms.Beam, "Storm Lance", Elements.Lightning, new SpellSpec { Cost = 40f, Reach = 11f, Radius = 0.9f, Seconds = 2.2f, Interval = 0.2f, Damage = 9f, Stagger = 8f }),
            Make(SpellForms.Phalanx, "Rime Wing", Elements.Ice, new SpellSpec { Cost = 35f, Count = 6, Seconds = 8f, Reach = 9f, Damage = 18f, Stagger = 14f, Speed = 12f }),
            Make(SpellForms.Dragon, "Dragon of Embers", Elements.Fire, new SpellSpec { Cost = 40f, Reach = 11f, Radius = 1.3f, Damage = 48f, Stagger = 50f, Speed = 12f }),
            Make(SpellForms.Nova, "Gale Crown", Elements.Wind, new SpellSpec { Cost = 30f, Radius = 4.5f, Delay = 0.45f, Damage = 34f, Stagger = 45f }),
            Make(SpellForms.Orb, "Black Moon", Elements.Dark, new SpellSpec { Cost = 30f, Speed = 3.2f, Seconds = 5f, Radius = 3f, Damage = 44f, Stagger = 40f }),
            Make(SpellForms.Bomb, "Great Blight", Elements.Poison, new SpellSpec { Cost = 50f, Delay = 1.1f, Reach = 7f, Radius = 4f, Damage = 75f, Stagger = 70f, Seconds = 4f, LingerDamage = 5f }),
            Make(SpellForms.Spikes, "Stone Teeth", Elements.Earth, new SpellSpec { Cost = 30f, Reach = 10f, Speed = 16f, Count = 3, Radius = 0.9f, Damage = 26f, Stagger = 30f }),
            Make(SpellForms.Field, "Thorn Bed", Elements.Life, new SpellSpec { Cost = 30f, Reach = 6f, Radius = 3.4f, Seconds = 5f, Interval = 0.5f, LingerDamage = 6f }),
            Make(SpellForms.Drill, "Iron Bore", Elements.Metal, new SpellSpec { Cost = 35f, Reach = 8f, Radius = 1.1f, Count = 4, Damage = 10f, Stagger = 10f, Speed = 14f }),
            Make(SpellForms.Minions, "Seedlings", Elements.Life, new SpellSpec { Cost = 35f, Count = 3, Seconds = 7f, Damage = 7f, Stagger = 6f, Interval = 0.6f }),
            Make(SpellForms.Bullets, "Rain Ring", Elements.Water, new SpellSpec { Cost = 30f, Count = 10, Radius = 3.2f, Delay = 0.6f, Damage = 7f, Stagger = 6f, Reach = 9f }),
            Make(SpellForms.Reverse, "Undertow", Elements.Water, new SpellSpec { Cost = 35f, Reach = 6f, Radius = 5f, Delay = 0.9f, Damage = 40f, Stagger = 45f }),
            Make(SpellForms.Parry, "Mirror of Day", Elements.Light, new SpellSpec { Cost = 20f, Seconds = 1.2f, Reach = 6f, Arc = 100f, Damage = 45f, Stagger = 70f }),
            Make(SpellForms.Wall, "Glacier Wall", Elements.Ice, new SpellSpec { Cost = 30f, Reach = 3.5f, Arc = 6f, Delay = 0.3f, Seconds = 5f, Radius = 3.5f, Damage = 36f, Stagger = 40f }),
            Make(SpellForms.Discs, "Saw Halo", Elements.Metal, new SpellSpec { Cost = 30f, Count = 3, Radius = 2.4f, Seconds = 6f, Interval = 0.35f, Damage = 6f, Stagger = 5f }),
            Make(SpellForms.Brand, "Brand of Day", Elements.Light, new SpellSpec { Cost = 25f, Reach = 9f, Seconds = 8f, Count = 3, Radius = 2.6f, Damage = 22f, Stagger = 22f }),
            Make(SpellForms.Rain, "Ember Rain", Elements.Fire, new SpellSpec { Cost = 40f, Reach = 8f, Radius = 4.5f, Seconds = 4f, Interval = 0.22f, Damage = 14f, Stagger = 10f }),
            Make(SpellForms.Clone, "Second Shadow", Elements.Dark, new SpellSpec { Cost = 40f, Seconds = 10f, Damage = 40f }),
            Make(SpellForms.Flash, "Thousand Cuts", Elements.Lightning, new SpellSpec { Cost = 45f, Count = 6, Interval = 0.07f, Delay = 0.5f, Damage = 11f, Stagger = 12f, Reach = 9f }),
            Make(SpellForms.Ring, "Night Pale", Elements.Dark, new SpellSpec { Cost = 35f, Radius = 5f, Seconds = 6f, LingerDamage = 6f, Reach = 8f }),
            Make(SpellForms.Pillars, "Standing Stones", Elements.Earth, new SpellSpec { Cost = 30f, Count = 3, Reach = 10f, Radius = 1.3f, Damage = 30f, Stagger = 45f, Delay = 0.35f }),
            Make(SpellForms.Step, "Gale Step", Elements.Wind, new SpellSpec { Cost = 20f, Reach = 6f, Damage = 22f, Stagger = 20f, Seconds = 4f }),
        };

        public static IReadOnlyList<Upgrade> All => all;

        private static Upgrade Make(string form, string name, string element, SpellSpec spec) => new Upgrade
        {
            Id = "sample." + form,
            Name = name,
            Element = element,
            Kind = UpgradeKind.Spell,
            Form = form,
            Spell = spec,
            Description = about.TryGetValue(form, out string words) ? words : "",
        };

        /// <summary>The sample spell of a form, or null when the form has none.</summary>
        public static Upgrade Of(string form)
        {
            foreach (var sample in all)
                if (sample.Form == form) return sample;
            return null;
        }
    }
}

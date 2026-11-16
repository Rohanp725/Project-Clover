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
            Description = "",
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

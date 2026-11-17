using System.Collections.Generic;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The solid shapes spells are made of, drawn in code from a few bold wedges each: no curve that a high camera
    /// would lose. Every face is flat (its corners are not shared), and each corner also carries, in its second set
    /// of UVs, the direction smoothed across the faces that meet there, which is what the ink line is pushed out along.
    /// Things that fly point along +Z from the origin; things that stand rise along +Y from the origin.
    /// </summary>
    public static class FormMeshes
    {
        public static Mesh Build(FormMesh which)
        {
            // Water is the one smooth thing among the cut shapes: its wave is a curved skin, not facets.
            if (which == FormMesh.Crest) return Wave();
            var made = new Maker();
            switch (which)
            {
                case FormMesh.Blade:
                    // A throwing blade: a long leaf, a cross-guard, a stub of a tang.
                    made.Bipyramid(new Vector3(0f, 0f, 0.16f), new Vector3(0f, 0f, 1f), 0.24f, Vector3.right * 0.13f, Vector3.up * 0.045f);
                    made.Bipyramid(new Vector3(-0.27f, 0f, 0.2f), new Vector3(0.27f, 0f, 0.2f), 0.5f, Vector3.forward * 0.06f, Vector3.up * 0.06f);
                    made.Bipyramid(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.22f), 0.5f, Vector3.right * 0.045f, Vector3.up * 0.045f);
                    break;
                case FormMesh.Sword:
                    // A great sword: a broad blade with a spine, a wide guard swept toward the point, a grip and a pommel.
                    made.Bipyramid(new Vector3(0f, 0f, 0.2f), new Vector3(0f, 0f, 1f), 0.2f, Vector3.right * 0.085f, Vector3.up * 0.03f);
                    made.Bipyramid(new Vector3(-0.26f, 0f, 0.26f), new Vector3(0f, 0f, 0.17f), 0.7f, Vector3.forward * 0.035f, Vector3.up * 0.035f);
                    made.Bipyramid(new Vector3(0.26f, 0f, 0.26f), new Vector3(0f, 0f, 0.17f), 0.7f, Vector3.forward * 0.035f, Vector3.up * 0.035f);
                    made.Bipyramid(new Vector3(0f, 0f, 0.03f), new Vector3(0f, 0f, 0.21f), 0.5f, Vector3.right * 0.022f, Vector3.up * 0.022f);
                    made.Bipyramid(new Vector3(0f, 0f, -0.03f), new Vector3(0f, 0f, 0.05f), 0.5f, Vector3.right * 0.05f, Vector3.up * 0.05f);
                    break;
                case FormMesh.Shard:
                    made.Bipyramid(Vector3.zero, new Vector3(0f, 0f, 1f), 0.3f, Vector3.right * 0.13f, Vector3.up * 0.13f);
                    break;
                case FormMesh.Spike:
                    // A fang of rock or ice: five sides, its point leaning a little, a splinter at its foot.
                    made.Cone(Vector3.zero, new Vector3(0.08f, 1f, 0.05f), 0.5f, 5, 0f);
                    made.Cone(new Vector3(0.3f, 0f, -0.2f), new Vector3(0.52f, 0.45f, -0.36f), 0.2f, 4, 0.6f);
                    break;
                case FormMesh.Pillar:
                    // A six-sided column with a broken top.
                    made.Prism(0.5f, 0.86f, 6);
                    made.Cone(new Vector3(0f, 0.86f, 0f), new Vector3(0.12f, 1f, -0.1f), 0.5f, 6, 0f);
                    break;
                case FormMesh.Disc:
                    made.Saw(1f, 0.7f, 0.14f, 8);
                    break;
                case FormMesh.Orb:
                    made.Gem(1f);
                    break;
                case FormMesh.Slab:
                    made.Slab();
                    break;
                case FormMesh.DragonSkull:
                    // The upper head: a long snout, a heavy brow, two horns swept back, fangs under the lip.
                    made.Hull(new[]
                    {
                        new Vector3(0f, 0.06f, 1f), new Vector3(-0.2f, 0.02f, 0.7f), new Vector3(0.2f, 0.02f, 0.7f),
                        new Vector3(0f, 0.3f, 0.62f), new Vector3(-0.36f, 0.02f, 0.05f), new Vector3(0.36f, 0.02f, 0.05f),
                        new Vector3(-0.3f, 0.42f, 0.2f), new Vector3(0.3f, 0.42f, 0.2f), new Vector3(0f, 0.5f, -0.1f),
                        new Vector3(0f, 0.05f, -0.2f),
                    }, new[]
                    {
                        0, 3, 1,  0, 2, 3,  0, 1, 2,  1, 3, 6,  2, 7, 3,  3, 7, 8,  3, 8, 6,  1, 6, 4,  2, 5, 7,
                        6, 8, 9,  7, 9, 8,  4, 6, 9,  5, 9, 7,  1, 4, 5,  1, 5, 2,  4, 9, 5,
                    });
                    made.Bipyramid(new Vector3(-0.24f, 0.4f, 0.12f), new Vector3(-0.5f, 0.92f, -0.7f), 0.25f, Vector3.right * 0.09f, new Vector3(0f, 0.06f, 0.07f));
                    made.Bipyramid(new Vector3(0.24f, 0.4f, 0.12f), new Vector3(0.5f, 0.92f, -0.7f), 0.25f, Vector3.right * 0.09f, new Vector3(0f, 0.06f, 0.07f));
                    made.Cone(new Vector3(-0.15f, 0.04f, 0.72f), new Vector3(-0.15f, -0.2f, 0.74f), 0.06f, 4, 0f);
                    made.Cone(new Vector3(0.15f, 0.04f, 0.72f), new Vector3(0.15f, -0.2f, 0.74f), 0.06f, 4, 0f);
                    made.Cone(new Vector3(-0.26f, 0.04f, 0.4f), new Vector3(-0.26f, -0.14f, 0.4f), 0.05f, 4, 0f);
                    made.Cone(new Vector3(0.26f, 0.04f, 0.4f), new Vector3(0.26f, -0.14f, 0.4f), 0.05f, 4, 0f);
                    break;
                case FormMesh.DragonJaw:
                    // The lower jaw, hinged at the origin: a wedge with two fangs standing up at its tip.
                    made.Hull(new[]
                    {
                        new Vector3(0f, 0f, 0.92f), new Vector3(-0.3f, 0f, 0f), new Vector3(0.3f, 0f, 0f), new Vector3(0f, -0.2f, 0.2f),
                    }, new[] { 0, 2, 1,  0, 1, 3,  0, 3, 2,  1, 2, 3 });
                    made.Cone(new Vector3(-0.1f, 0f, 0.74f), new Vector3(-0.1f, 0.18f, 0.76f), 0.05f, 4, 0f);
                    made.Cone(new Vector3(0.1f, 0f, 0.74f), new Vector3(0.1f, 0.18f, 0.76f), 0.05f, 4, 0f);
                    break;
                case FormMesh.DragonSpine:
                    // One joint of the neck: a thick diamond with a fin along its back.
                    made.Bipyramid(new Vector3(0f, 0f, -0.5f), new Vector3(0f, 0f, 0.5f), 0.55f, Vector3.right * 0.34f, Vector3.up * 0.28f);
                    made.Bipyramid(new Vector3(0f, 0.15f, 0.2f), new Vector3(0f, 0.62f, -0.4f), 0.3f, Vector3.right * 0.06f, new Vector3(0f, 0.1f, 0.14f));
                    break;
                case FormMesh.Drill:
                    // A bit: a long point, a wider collar behind it, three flukes.
                    made.Cone(new Vector3(0f, 0f, 0.3f), new Vector3(0f, 0f, 1.5f), 0.34f, 6, 0f);
                    made.Cone(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.75f), 0.62f, 6, 0.5f);
                    for (int i = 0; i < 3; i++)
                    {
                        var turn = Quaternion.Euler(0f, 0f, i * 120f);
                        made.Bipyramid(turn * new Vector3(0.3f, 0f, 0.1f), turn * new Vector3(0.95f, 0f, -0.35f), 0.3f, turn * new Vector3(0f, 0.09f, 0f), new Vector3(0f, 0f, 0.16f));
                    }
                    break;
                case FormMesh.Minion:
                    // A small helper: a body like a flame standing on its point, a head, two horns, one blade held out.
                    made.Bipyramid(new Vector3(0f, 0.05f, 0f), new Vector3(0f, 0.78f, 0f), 0.68f, Vector3.right * 0.26f, Vector3.forward * 0.2f);
                    made.Bipyramid(new Vector3(0f, 0.74f, 0.02f), new Vector3(0f, 1.12f, 0.02f), 0.45f, Vector3.right * 0.2f, Vector3.forward * 0.2f);
                    made.Cone(new Vector3(-0.12f, 1.02f, 0f), new Vector3(-0.3f, 1.38f, -0.1f), 0.07f, 4, 0f);
                    made.Cone(new Vector3(0.12f, 1.02f, 0f), new Vector3(0.3f, 1.38f, -0.1f), 0.07f, 4, 0f);
                    made.Bipyramid(new Vector3(0.24f, 0.56f, 0.05f), new Vector3(0.3f, 0.62f, 0.95f), 0.25f, Vector3.right * 0.035f, Vector3.up * 0.1f);
                    break;
                case FormMesh.Flame:
                    Tongues(made, Vector3.up, Vector3.right, Vector3.forward);
                    break;
                case FormMesh.Fireball:
                    // The same tongues laid back along its flight, behind a hard head.
                    made.Bipyramid(new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 0.45f), 0.4f, Vector3.right * 0.26f, Vector3.up * 0.26f);
                    made.Bipyramid(new Vector3(0.05f, 0.1f, 0.7f), new Vector3(0.12f, 0.3f, 0f), 0.25f, Vector3.right * 0.14f, Vector3.forward * 0.1f);
                    made.Bipyramid(new Vector3(-0.08f, -0.05f, 0.7f), new Vector3(-0.3f, -0.12f, 0.1f), 0.25f, Vector3.up * 0.12f, Vector3.forward * 0.1f);
                    made.Bipyramid(new Vector3(0.1f, -0.08f, 0.7f), new Vector3(0.26f, -0.2f, 0.22f), 0.25f, Vector3.up * 0.1f, Vector3.forward * 0.1f);
                    break;
                case FormMesh.Bolt:
                {
                    // Four hard turns and a fork: never a straight line.
                    var turns = new[] { Vector3.zero, new Vector3(0.14f, 0.05f, 0.27f), new Vector3(-0.12f, -0.05f, 0.5f), new Vector3(0.1f, 0.06f, 0.76f), new Vector3(0f, 0f, 1f) };
                    for (int i = 0; i + 1 < turns.Length; i++)
                        made.Bipyramid(turns[i], turns[i + 1], 0.5f, Vector3.right * 0.045f, Vector3.up * 0.045f);
                    made.Bipyramid(turns[2], new Vector3(-0.34f, 0.04f, 0.72f), 0.3f, new Vector3(0.03f, 0f, 0.03f), Vector3.up * 0.03f);
                    made.Bipyramid(new Vector3(-0.34f, 0.04f, 0.72f), new Vector3(-0.3f, -0.03f, 0.92f), 0.3f, Vector3.right * 0.025f, Vector3.up * 0.025f);
                    break;
                }
                case FormMesh.Crystal:
                    // Three six-sided crystals leaning out of one root, each cut to a point.
                    Crystal(made, Vector3.zero, new Vector3(0.06f, 1f, 0.03f), 0.2f);
                    Crystal(made, new Vector3(0.14f, 0f, 0.08f), new Vector3(0.52f, 0.62f, 0.26f), 0.13f);
                    Crystal(made, new Vector3(-0.12f, 0f, -0.1f), new Vector3(-0.44f, 0.46f, -0.32f), 0.11f);
                    break;
                case FormMesh.Crest:
                {
                    // Six lengths of wave side by side. Each is a long swell climbing from far behind to a sharp
                    // ridge, a steep face under it, and a lip thrown forward and down over its own foot. The ridge
                    // is uneven from one length to the next, so the crest line is ragged.
                    float[] tall = { 0.6f, 0.92f, 0.78f, 1f, 0.84f, 0.7f, 0.56f };
                    float[] lean = { -0.04f, 0.1f, 0f, 0.12f, 0.04f, -0.02f, -0.06f };
                    for (int i = 0; i < 6; i++)
                    {
                        float x0 = -0.5f + i / 6f, x1 = x0 + 1f / 6f;
                        float h0 = tall[i], h1 = tall[i + 1], l0 = lean[i], l1 = lean[i + 1];
                        // Seen from above the wave is a crescent: its middle runs ahead of its ends.
                        float b0 = 0.45f * (1f - 4f * x0 * x0), b1 = 0.45f * (1f - 4f * x1 * x1);
                        made.Hull(new[]
                        {
                            new Vector3(x0, 0f, -0.7f + b0 * 0.4f), new Vector3(x0, 0f, 0.12f + b0), new Vector3(x0, h0, 0.1f + l0 + b0),
                            new Vector3(x1, 0f, -0.7f + b1 * 0.4f), new Vector3(x1, 0f, 0.12f + b1), new Vector3(x1, h1, 0.1f + l1 + b1),
                        }, Prism3);
                        made.Hull(new[]
                        {
                            new Vector3(x0, h0 * 0.72f, 0.1f + b0), new Vector3(x0, h0, 0.1f + l0 + b0), new Vector3(x0, h0 * 0.42f, 0.7f + l0 + b0),
                            new Vector3(x1, h1 * 0.72f, 0.1f + b1), new Vector3(x1, h1, 0.1f + l1 + b1), new Vector3(x1, h1 * 0.42f, 0.7f + l1 + b1),
                        }, Prism3);
                    }
                    break;
                }
                case FormMesh.Foam:
                    // Flecks scattered over a band one metre square: pressed flat on the ground they are the lace a
                    // wave leaves behind it, stood up a little they are the churn at its foot.
                    for (int i = 0; i < 24; i++)
                    {
                        float x = -0.48f + 0.96f * ElementSubstance.Hash(i * 3 + 1);
                        float z = -0.5f + ElementSubstance.Hash(i * 3 + 2);
                        made.Blob(new Vector3(x, 0.02f, z + 0.3f * (1f - 4f * x * x)), 0.03f + 0.05f * ElementSubstance.Hash(i * 3 + 3));
                    }
                    break;
                case FormMesh.Droplet:
                    made.Bipyramid(Vector3.zero, new Vector3(0f, 0f, 1f), 0.72f, Vector3.right * 0.24f, Vector3.up * 0.24f);
                    break;
                case FormMesh.Spout:
                    made.Bipyramid(Vector3.zero, new Vector3(0f, 1f, 0f), 0.72f, Vector3.right * 0.3f, Vector3.forward * 0.3f);
                    made.Bipyramid(new Vector3(0.2f, 0f, 0.1f), new Vector3(0.45f, 0.6f, 0.2f), 0.75f, Vector3.right * 0.13f, Vector3.forward * 0.13f);
                    made.Bipyramid(new Vector3(-0.2f, 0f, -0.1f), new Vector3(-0.42f, 0.45f, -0.25f), 0.75f, Vector3.right * 0.11f, Vector3.forward * 0.11f);
                    break;
                case FormMesh.Rock:
                    // A block broken out of the ground: wide at the foot, its top skewed and uneven.
                    made.Hull(new[]
                    {
                        new Vector3(-0.5f, 0f, -0.4f), new Vector3(0.5f, 0f, -0.45f), new Vector3(0.55f, 0f, 0.4f), new Vector3(-0.45f, 0f, 0.5f),
                        new Vector3(-0.3f, 0.82f, -0.25f), new Vector3(0.26f, 1f, -0.3f), new Vector3(0.36f, 0.68f, 0.25f), new Vector3(-0.24f, 0.9f, 0.3f),
                    }, Box8);
                    made.Hull(new[]
                    {
                        new Vector3(0.35f, 0f, 0.2f), new Vector3(0.8f, 0f, 0.15f), new Vector3(0.78f, 0f, 0.6f), new Vector3(0.4f, 0f, 0.62f),
                        new Vector3(0.5f, 0.42f, 0.3f), new Vector3(0.72f, 0.34f, 0.28f), new Vector3(0.7f, 0.3f, 0.5f), new Vector3(0.5f, 0.38f, 0.52f),
                    }, Box8);
                    break;
                case FormMesh.Pebble:
                    made.Hull(new[]
                    {
                        new Vector3(-0.2f, -0.16f, 0.1f), new Vector3(0.22f, -0.2f, 0.05f), new Vector3(0.25f, -0.12f, 0.8f), new Vector3(-0.18f, -0.2f, 0.9f),
                        new Vector3(-0.14f, 0.2f, 0.2f), new Vector3(0.12f, 0.24f, 0f), new Vector3(0.2f, 0.14f, 0.7f), new Vector3(-0.1f, 0.18f, 1f),
                    }, Box8);
                    break;
                case FormMesh.Leaf:
                    made.Bipyramid(Vector3.zero, new Vector3(0f, 0f, 1f), 0.4f, Vector3.right * 0.28f, Vector3.up * 0.035f);
                    made.Bipyramid(new Vector3(0f, 0f, -0.2f), new Vector3(0f, 0f, 0.1f), 0.5f, Vector3.right * 0.03f, Vector3.up * 0.03f);
                    break;
                case FormMesh.Vine:
                {
                    // A stem that curls as it climbs, thorns standing off every joint, a leaf at its first turn.
                    var joints = new[] { Vector3.zero, new Vector3(0.2f, 0.36f, 0.06f), new Vector3(0.06f, 0.7f, 0.24f), new Vector3(-0.2f, 0.92f, 0.1f), new Vector3(-0.3f, 0.8f, -0.12f) };
                    float[] thick = { 0.11f, 0.09f, 0.07f, 0.05f };
                    for (int i = 0; i + 1 < joints.Length; i++)
                        made.Bipyramid(joints[i], joints[i + 1], 0.4f, Vector3.right * thick[i], Vector3.forward * thick[i]);
                    made.Cone(joints[1], joints[1] + new Vector3(0.24f, 0.1f, -0.14f), 0.05f, 4, 0f);
                    made.Cone(joints[2], joints[2] + new Vector3(0.1f, 0.12f, 0.26f), 0.05f, 4, 0f);
                    made.Cone(joints[2] * 0.5f + joints[1] * 0.5f, joints[2] * 0.5f + joints[1] * 0.5f + new Vector3(-0.24f, 0.06f, 0.08f), 0.045f, 4, 0f);
                    made.Cone(joints[3], joints[3] + new Vector3(-0.1f, 0.2f, 0.16f), 0.04f, 4, 0f);
                    made.Bipyramid(new Vector3(0.2f, 0.36f, 0.06f), new Vector3(0.62f, 0.44f, 0.3f), 0.4f, new Vector3(-0.08f, 0f, 0.14f), Vector3.up * 0.025f);
                    break;
                }
                case FormMesh.Flower:
                    for (int i = 0; i < 5; i++)
                    {
                        var turn = Quaternion.Euler(0f, i * 72f, 0f);
                        made.Bipyramid(turn * new Vector3(0f, 0.04f, 0.08f), turn * new Vector3(0f, 0.16f, 0.5f), 0.6f, turn * Vector3.right * 0.17f, Vector3.up * 0.04f);
                    }
                    made.Blob(new Vector3(0f, 0.1f, 0f), 0.12f);
                    break;
                case FormMesh.Cloud:
                    made.Blob(Vector3.zero, 0.34f);
                    made.Blob(new Vector3(0.3f, -0.04f, 0.08f), 0.26f);
                    made.Blob(new Vector3(-0.28f, -0.06f, -0.06f), 0.28f);
                    made.Blob(new Vector3(0.04f, 0.02f, 0.3f), 0.22f);
                    made.Blob(new Vector3(-0.06f, 0.16f, -0.22f), 0.2f);
                    break;
                case FormMesh.Bubble:
                    made.Blob(new Vector3(0f, 0f, 0.7f), 0.3f);
                    made.Blob(new Vector3(0.12f, 0.1f, 0.22f), 0.16f);
                    made.Blob(new Vector3(-0.1f, -0.06f, 0f), 0.09f);
                    break;
                case FormMesh.Ray:
                    made.Bipyramid(Vector3.zero, new Vector3(0f, 0f, 1f), 0.06f, Vector3.right * 0.05f, Vector3.up * 0.05f);
                    made.Bipyramid(new Vector3(-0.3f, 0f, 0.06f), new Vector3(0.3f, 0f, 0.06f), 0.5f, Vector3.forward * 0.03f, Vector3.up * 0.03f);
                    made.Bipyramid(new Vector3(0f, -0.3f, 0.06f), new Vector3(0f, 0.3f, 0.06f), 0.5f, Vector3.forward * 0.03f, Vector3.right * 0.03f);
                    break;
                case FormMesh.Scarab:
                    // Wing-case, chest and head in a row, two forelegs lifted ahead of it, four legs on the ground.
                    made.Bipyramid(new Vector3(0f, 0.17f, -0.45f), new Vector3(0f, 0.17f, 0.2f), 0.45f, Vector3.right * 0.3f, Vector3.up * 0.17f);
                    made.Bipyramid(new Vector3(0f, 0.16f, 0.1f), new Vector3(0f, 0.16f, 0.42f), 0.5f, Vector3.right * 0.22f, Vector3.up * 0.13f);
                    made.Bipyramid(new Vector3(0f, 0.13f, 0.36f), new Vector3(0f, 0.1f, 0.6f), 0.4f, Vector3.right * 0.13f, Vector3.up * 0.07f);
                    for (int i = -1; i <= 1; i += 2)
                    {
                        made.Bipyramid(new Vector3(0.14f * i, 0.15f, 0.4f), new Vector3(0.3f * i, 0.4f, 0.74f), 0.5f, Vector3.right * 0.04f, Vector3.up * 0.04f);
                        made.Bipyramid(new Vector3(0.2f * i, 0.13f, 0.05f), new Vector3(0.52f * i, 0f, -0.06f), 0.4f, Vector3.forward * 0.035f, Vector3.up * 0.035f);
                        made.Bipyramid(new Vector3(0.22f * i, 0.13f, -0.24f), new Vector3(0.5f * i, 0f, -0.42f), 0.4f, Vector3.forward * 0.035f, Vector3.up * 0.035f);
                    }
                    break;
                case FormMesh.Falcon:
                    // Body, hooked head, a fanned tail, and on each side a wing in two lengths with a long feather behind it and a claw below.
                    made.Bipyramid(new Vector3(0f, 0f, -0.35f), new Vector3(0f, 0f, 0.4f), 0.6f, Vector3.right * 0.12f, Vector3.up * 0.1f);
                    made.Bipyramid(new Vector3(0f, 0.03f, 0.32f), new Vector3(0f, -0.07f, 0.64f), 0.35f, Vector3.right * 0.075f, Vector3.up * 0.075f);
                    made.Bipyramid(new Vector3(0f, 0f, -0.28f), new Vector3(0f, 0.02f, -0.78f), 0.7f, Vector3.right * 0.21f, Vector3.up * 0.025f);
                    for (int i = -1; i <= 1; i += 2)
                    {
                        made.Bipyramid(new Vector3(0.08f * i, 0.02f, 0.1f), new Vector3(0.6f * i, 0.17f, 0.02f), 0.5f, Vector3.forward * 0.23f, Vector3.up * 0.03f);
                        made.Bipyramid(new Vector3(0.5f * i, 0.15f, 0.05f), new Vector3(1.05f * i, 0.02f, -0.3f), 0.4f, Vector3.forward * 0.17f, Vector3.up * 0.025f);
                        made.Bipyramid(new Vector3(0.7f * i, 0.1f, -0.05f), new Vector3(0.9f * i, 0.02f, -0.52f), 0.4f, Vector3.right * 0.08f, Vector3.up * 0.02f);
                        made.Bipyramid(new Vector3(0.35f * i, 0.1f, -0.05f), new Vector3(0.45f * i, 0.03f, -0.4f), 0.4f, Vector3.right * 0.08f, Vector3.up * 0.02f);
                        made.Bipyramid(new Vector3(0.07f * i, -0.05f, 0f), new Vector3(0.09f * i, -0.3f, 0.08f), 0.4f, Vector3.right * 0.03f, Vector3.forward * 0.03f);
                    }
                    break;
                case FormMesh.Gust:
                {
                    // A swept curve, thick in its middle and gone to nothing at both ends, with a curl at its head.
                    var bend = new[] { Vector3.zero, new Vector3(0.16f, 0f, 0.3f), new Vector3(0.2f, 0f, 0.62f), new Vector3(0.06f, 0f, 0.9f), new Vector3(-0.12f, 0f, 1f), new Vector3(-0.2f, 0f, 0.86f) };
                    float[] wide = { 0.03f, 0.06f, 0.06f, 0.045f, 0.03f };
                    for (int i = 0; i + 1 < bend.Length; i++)
                        made.Bipyramid(bend[i], bend[i + 1], 0.5f, Vector3.right * wide[i], Vector3.up * 0.02f);
                    break;
                }
            }
            return made.Finish("Form " + which);
        }

        // The wave's shape seen from its side, as (ahead, up): a long back climbing to the crest, the lip thrown
        // forward and drooping, the hollow face under it, and the foot running out in front.
        private static readonly Vector2[] WaveSide =
        {
            new Vector2(-0.95f, 0f), new Vector2(-0.62f, 0.14f), new Vector2(-0.32f, 0.46f), new Vector2(-0.06f, 0.84f), new Vector2(0.2f, 1f),
            new Vector2(0.48f, 0.93f), new Vector2(0.68f, 0.68f), new Vector2(0.52f, 0.64f), new Vector2(0.32f, 0.52f), new Vector2(0.24f, 0.27f),
            new Vector2(0.36f, 0.08f), new Vector2(0.62f, 0f),
        };

        /// <summary>
        /// A breaking wave one metre wide across X, rolling toward +Z: one smooth skin swept across its width, tall in
        /// the middle and sinking to nothing at both ends, its middle running ahead of its ends. It is painted as
        /// water is: deep at the foot, green where the light comes through the top, foam along the crest and lip and
        /// where the foot runs out. The spell shader reads the paint.
        /// </summary>
        private static Mesh Wave()
        {
            const int across = 30, perSpan = 4;
            int round = (WaveSide.Length - 1) * perSpan + 1;
            var side = new Vector2[round];
            for (int i = 0; i < round; i++)
            {
                int span = Mathf.Min(i / perSpan, WaveSide.Length - 2);
                float t = (i - span * perSpan) / (float)perSpan;
                Vector2 a = WaveSide[Mathf.Max(span - 1, 0)], b = WaveSide[span], c = WaveSide[span + 1], d = WaveSide[Mathf.Min(span + 2, WaveSide.Length - 1)];
                side[i] = 0.5f * (2f * b + (c - a) * t + (2f * a - 5f * b + 4f * c - d) * (t * t) + (3f * b - a - 3f * c + d) * (t * t * t));
                side[i].y = Mathf.Max(0f, side[i].y);
            }
            // Where along the side the crest's foam begins and ends, and where the foot's begins.
            int crestFrom = 5 * perSpan + 2, crestTo = 7 * perSpan, footFrom = 10 * perSpan - 1;

            var points = new List<Vector3>();
            var paint = new List<Color>();
            var faces = new List<int>();
            for (int column = 0; column <= across; column++)
            {
                float x = -0.5f + column / (float)across;
                float edge = Mathf.Abs(x) * 2f;
                float tall = Mathf.Max(0.03f, 1f - edge * edge * edge * edge) * (0.88f + 0.12f * Mathf.Sin(x * 19f + 1.3f));
                float ahead = 0.45f * (1f - 4f * x * x);
                float lean = 0.07f * Mathf.Sin(x * 27f) + 0.05f * Mathf.Sin(x * 11f + 2f);
                // The foam's edge wanders from one stretch of the wave to the next.
                int ragged = Mathf.RoundToInt(2.2f * Mathf.Sin(x * 41f) + 1.4f * Mathf.Sin(x * 13f + 0.7f));
                for (int i = 0; i < round; i++)
                {
                    float up = side[i].y;
                    points.Add(new Vector3(x, up * tall, side[i].x * (0.35f + 0.65f * tall) + ahead + lean * up));
                    Color water = Color.Lerp(new Color(0.22f, 0.36f, 0.7f), new Color(0.7f, 1.2f, 1.02f), Mathf.Clamp01(up * 1.25f));
                    if (up > 0.6f) water = Color.Lerp(water, new Color(0.95f, 1.95f, 1.35f), (up - 0.6f) / 0.4f);
                    // The hollow under the lip is in the wave's own shade.
                    if (i > crestTo && i < footFrom) water *= 0.8f;
                    bool foam = (i >= crestFrom - ragged / 2 && i <= crestTo + Mathf.Abs(ragged) / 2) || i >= footFrom - Mathf.Max(0, ragged) || edge > 0.97f;
                    water.a = foam ? 0f : 1f;
                    paint.Add(water);
                }
            }
            for (int column = 0; column < across; column++)
                for (int i = 0; i < round; i++)
                {
                    int a = column * round + i, b = column * round + (i + 1) % round, c = a + round, d = b + round;
                    faces.Add(a); faces.Add(b); faces.Add(c);
                    faces.Add(b); faces.Add(d); faces.Add(c);
                }
            // The two ends, closed, so the ink line has a back to it.
            for (int end = 0; end < 2; end++)
            {
                int first = end * across * round, middle = points.Count;
                Vector3 centre = Vector3.zero;
                for (int i = 0; i < round; i++) centre += points[first + i];
                points.Add(centre / round);
                paint.Add(new Color(1f, 1f, 1f, 0f));
                for (int i = 0; i < round; i++)
                {
                    faces.Add(middle);
                    faces.Add(end == 1 ? first + i : first + (i + 1) % round);
                    faces.Add(end == 1 ? first + (i + 1) % round : first + i);
                }
            }
            var mesh = new Mesh { name = "Form Crest" };
            mesh.SetVertices(points);
            mesh.SetColors(paint);
            mesh.SetTriangles(faces, 0);
            mesh.RecalculateNormals();
            var normals = new List<Vector3>();
            mesh.GetNormals(normals);
            mesh.SetUVs(1, normals);
            mesh.RecalculateBounds();
            return mesh;
        }

        // A three-sided bar and a six-faced block, as faces between corners: the first half of the corners is one end.
        private static readonly int[] Prism3 = { 0, 1, 2,  3, 5, 4,  0, 3, 4,  0, 4, 1,  1, 4, 5,  1, 5, 2,  2, 5, 3,  2, 3, 0 };
        private static readonly int[] Box8 = { 0, 1, 2,  0, 2, 3,  4, 5, 6,  4, 6, 7,  0, 1, 5,  0, 5, 4,  1, 2, 6,  1, 6, 5,  2, 3, 7,  2, 7, 6,  3, 0, 4,  3, 4, 7 };

        // Three tongues of flame along an axis: a tall one that leans and two short ones at its foot.
        private static void Tongues(Maker made, Vector3 up, Vector3 right, Vector3 forward)
        {
            made.Bipyramid(Vector3.zero, up + right * 0.14f + forward * 0.05f, 0.3f, right * 0.3f, forward * 0.22f);
            made.Bipyramid(right * -0.2f + forward * 0.1f, up * 0.62f + right * -0.36f + forward * 0.16f, 0.3f, right * 0.17f, forward * 0.14f);
            made.Bipyramid(right * 0.18f + forward * -0.14f, up * 0.48f + right * 0.3f + forward * -0.26f, 0.3f, right * 0.14f, forward * 0.13f);
        }

        // One crystal: a six-sided shaft from its foot, cut to a point over its last quarter.
        private static void Crystal(Maker made, Vector3 foot, Vector3 point, float radius)
        {
            Vector3 neck = Vector3.Lerp(foot, point, 0.72f);
            made.Cone(neck, point, radius, 6, 0f);
            made.Cone(neck, foot, radius * 0.8f, 6, 0f);
        }

        /// <summary>A flat square on the ground, one metre across, centred, facing up.</summary>
        public static Mesh Square() => Flat("Form Mark", new Vector3(-0.5f, 0f, -0.5f), new Vector3(-0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, -0.5f),
            new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f));

        /// <summary>A flat strip on the ground running one metre along +Z from the origin, one metre wide; u runs along it.</summary>
        public static Mesh Strip() => Flat("Form Strip", new Vector3(-0.5f, 0f, 0f), new Vector3(-0.5f, 0f, 1f), new Vector3(0.5f, 0f, 1f), new Vector3(0.5f, 0f, 0f),
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f));

        private static Mesh Flat(string name, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector2 ua, Vector2 ub, Vector2 uc, Vector2 ud)
        {
            var mesh = new Mesh { name = name };
            mesh.vertices = new[] { a, b, c, d };
            mesh.uv = new[] { ua, ub, uc, ud };
            mesh.colors = new[] { Color.white, Color.white, Color.white, Color.white };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateBounds();
            return mesh;
        }

        // Collects triangles piece by piece. Every piece is convex, so each of its faces is turned to look away from
        // the piece's own middle: nobody has to get the winding right by hand.
        private class Maker
        {
            private readonly List<Vector3> corners = new List<Vector3>();

            private void Piece(List<Vector3> triangles)
            {
                Vector3 middle = Vector3.zero;
                foreach (var point in triangles) middle += point;
                middle /= Mathf.Max(1, triangles.Count);
                for (int i = 0; i + 2 < triangles.Count; i += 3)
                {
                    Vector3 a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
                    Vector3 normal = Vector3.Cross(b - a, c - a);
                    if (normal.sqrMagnitude < 1e-10f) continue;
                    bool outward = Vector3.Dot(normal, (a + b + c) / 3f - middle) >= 0f;
                    corners.Add(a);
                    corners.Add(outward ? b : c);
                    corners.Add(outward ? c : b);
                }
            }

            /// <summary>Two points joined through a ring of four corners part-way between them.</summary>
            public void Bipyramid(Vector3 from, Vector3 to, float at, Vector3 side, Vector3 up)
            {
                Vector3 middle = Vector3.Lerp(from, to, at);
                var ring = new[] { middle + side, middle + up, middle - side, middle - up };
                var triangles = new List<Vector3>();
                for (int i = 0; i < 4; i++)
                {
                    triangles.Add(from); triangles.Add(ring[i]); triangles.Add(ring[(i + 1) % 4]);
                    triangles.Add(to); triangles.Add(ring[i]); triangles.Add(ring[(i + 1) % 4]);
                }
                Piece(triangles);
            }

            /// <summary>A pyramid: a ring of corners round a foot, drawn to a point. The ring lies square to the line from foot to point.</summary>
            public void Cone(Vector3 foot, Vector3 point, float radius, int sides, float turn)
            {
                Vector3 axis = (point - foot).normalized;
                Vector3 across = Vector3.Cross(axis, Mathf.Abs(axis.y) > 0.9f ? Vector3.forward : Vector3.up).normalized;
                Vector3 other = Vector3.Cross(axis, across);
                var triangles = new List<Vector3>();
                for (int i = 0; i < sides; i++)
                {
                    float a = (i + turn) * Mathf.PI * 2f / sides, b = (i + 1 + turn) * Mathf.PI * 2f / sides;
                    Vector3 first = foot + (across * Mathf.Cos(a) + other * Mathf.Sin(a)) * radius;
                    Vector3 second = foot + (across * Mathf.Cos(b) + other * Mathf.Sin(b)) * radius;
                    triangles.Add(point); triangles.Add(first); triangles.Add(second);
                    triangles.Add(foot); triangles.Add(first); triangles.Add(second);
                }
                Piece(triangles);
            }

            /// <summary>An upright column standing on the origin.</summary>
            public void Prism(float radius, float height, int sides)
            {
                var triangles = new List<Vector3>();
                Vector3 top = Vector3.up * height;
                for (int i = 0; i < sides; i++)
                {
                    float a = i * Mathf.PI * 2f / sides, b = (i + 1) * Mathf.PI * 2f / sides;
                    Vector3 first = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius, second = new Vector3(Mathf.Cos(b), 0f, Mathf.Sin(b)) * radius;
                    triangles.Add(first); triangles.Add(second); triangles.Add(second + top);
                    triangles.Add(first); triangles.Add(second + top); triangles.Add(first + top);
                    triangles.Add(Vector3.zero); triangles.Add(first); triangles.Add(second);
                }
                Piece(triangles);
            }

            /// <summary>A flat wheel of teeth lying in the ground plane, thick at the hub and sharp at the rim.</summary>
            public void Saw(float outer, float inner, float thick, int teeth)
            {
                // Not convex as a whole, so each tooth is its own piece: hub top, hub bottom, and the two corners of the tooth.
                for (int i = 0; i < teeth; i++)
                {
                    float a = i * Mathf.PI * 2f / teeth, b = (i + 1) * Mathf.PI * 2f / teeth, tip = a + (b - a) * 0.22f;
                    Vector3 low = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * inner, next = new Vector3(Mathf.Cos(b), 0f, Mathf.Sin(b)) * inner;
                    Vector3 point = new Vector3(Mathf.Cos(tip), 0f, Mathf.Sin(tip)) * outer;
                    Vector3 top = Vector3.up * thick, bottom = Vector3.down * thick;
                    var triangles = new List<Vector3>
                    {
                        top, low, point,  top, point, next,  bottom, low, point,  bottom, point, next,
                    };
                    Piece(triangles);
                }
            }

            /// <summary>A cut stone: twenty flat faces.</summary>
            public void Gem(float radius)
            {
                float t = (1f + Mathf.Sqrt(5f)) * 0.5f;
                var points = new[]
                {
                    new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                    new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                    new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1),
                };
                var faces = new[]
                {
                    0, 11, 5,  0, 5, 1,  0, 1, 7,  0, 7, 10,  0, 10, 11,  1, 5, 9,  5, 11, 4,  11, 10, 2,  10, 7, 6,  7, 1, 8,
                    3, 9, 4,  3, 4, 2,  3, 2, 6,  3, 6, 8,  3, 8, 9,  4, 9, 5,  2, 4, 11,  6, 2, 10,  8, 6, 7,  9, 8, 1,
                };
                var triangles = new List<Vector3>();
                foreach (int index in faces) triangles.Add(points[index].normalized * radius);
                Piece(triangles);
            }

            /// <summary>A slab of wall, one metre wide and one tall, standing on the origin across X: a broken top, thick at the foot.</summary>
            public void Slab()
            {
                var front = new[] { new Vector3(-0.5f, 0f, 0f), new Vector3(0.5f, 0f, 0f), new Vector3(0.5f, 0.72f, 0f), new Vector3(0.12f, 1f, 0f), new Vector3(-0.5f, 0.84f, 0f) };
                var triangles = new List<Vector3>();
                Vector3 thick = Vector3.forward * 0.2f, thin = Vector3.forward * 0.1f;
                for (int i = 0; i < front.Length; i++)
                {
                    Vector3 a = front[i], b = front[(i + 1) % front.Length];
                    Vector3 depthA = a.y < 0.01f ? thick : thin, depthB = b.y < 0.01f ? thick : thin;
                    triangles.Add(a - depthA); triangles.Add(b - depthB); triangles.Add(b + depthB);
                    triangles.Add(a - depthA); triangles.Add(b + depthB); triangles.Add(a + depthA);
                }
                // The two faces, each a fan pushed out to a ridge in the middle so the slab is not a board.
                Vector3 ridge = new Vector3(0f, 0.5f, 0f);
                for (int i = 0; i < front.Length; i++)
                {
                    Vector3 a = front[i], b = front[(i + 1) % front.Length];
                    Vector3 depthA = a.y < 0.01f ? thick : thin, depthB = b.y < 0.01f ? thick : thin;
                    triangles.Add(ridge + Vector3.forward * 0.26f); triangles.Add(a + depthA); triangles.Add(b + depthB);
                    triangles.Add(ridge - Vector3.forward * 0.26f); triangles.Add(a - depthA); triangles.Add(b - depthB);
                }
                Piece(triangles);
            }

            /// <summary>A rough ball: two rings of five corners between a top and a bottom.</summary>
            public void Blob(Vector3 centre, float radius)
            {
                var upper = new Vector3[5];
                var lower = new Vector3[5];
                for (int i = 0; i < 5; i++)
                {
                    float a = i * 72f * Mathf.Deg2Rad, b = (i * 72f + 36f) * Mathf.Deg2Rad;
                    upper[i] = centre + new Vector3(Mathf.Cos(a) * 0.88f, 0.45f, Mathf.Sin(a) * 0.88f) * radius;
                    lower[i] = centre + new Vector3(Mathf.Cos(b) * 0.88f, -0.45f, Mathf.Sin(b) * 0.88f) * radius;
                }
                Vector3 top = centre + Vector3.up * radius, bottom = centre - Vector3.up * radius;
                var triangles = new List<Vector3>();
                for (int i = 0; i < 5; i++)
                {
                    int next = (i + 1) % 5;
                    triangles.Add(top); triangles.Add(upper[i]); triangles.Add(upper[next]);
                    triangles.Add(bottom); triangles.Add(lower[i]); triangles.Add(lower[next]);
                    triangles.Add(upper[i]); triangles.Add(lower[i]); triangles.Add(upper[next]);
                    triangles.Add(upper[next]); triangles.Add(lower[i]); triangles.Add(lower[next]);
                }
                Piece(triangles);
            }

            /// <summary>A convex shape given as corners and the faces between them.</summary>
            public void Hull(Vector3[] points, int[] faces)
            {
                var triangles = new List<Vector3>();
                foreach (int index in faces) triangles.Add(points[index]);
                Piece(triangles);
            }

            public Mesh Finish(string name)
            {
                int count = corners.Count;
                var normals = new Vector3[count];
                var smooth = new List<Vector3>(count);
                var indices = new int[count];
                var summed = new Dictionary<Vector3Int, Vector3>();
                for (int i = 0; i < count; i += 3)
                {
                    Vector3 normal = Vector3.Cross(corners[i + 1] - corners[i], corners[i + 2] - corners[i]).normalized;
                    for (int k = 0; k < 3; k++)
                    {
                        normals[i + k] = normal;
                        indices[i + k] = i + k;
                        var key = Key(corners[i + k]);
                        summed.TryGetValue(key, out Vector3 sum);
                        summed[key] = sum + normal;
                    }
                }
                for (int i = 0; i < count; i++)
                {
                    Vector3 sum = summed[Key(corners[i])];
                    smooth.Add(sum.sqrMagnitude > 1e-6f ? sum.normalized : normals[i]);
                }
                var mesh = new Mesh { name = name };
                mesh.SetVertices(corners);
                // Unpainted: the spell shader reads white as "the element's own colour, no foam".
                var paint = new Color[count];
                for (int i = 0; i < count; i++) paint[i] = Color.white;
                mesh.colors = paint;
                mesh.normals = normals;
                mesh.SetUVs(1, smooth);
                mesh.triangles = indices;
                mesh.RecalculateBounds();
                return mesh;
            }

            private static Vector3Int Key(Vector3 point) =>
                new Vector3Int(Mathf.RoundToInt(point.x * 2000f), Mathf.RoundToInt(point.y * 2000f), Mathf.RoundToInt(point.z * 2000f));
        }
    }
}

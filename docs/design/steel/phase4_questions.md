# Phase 4: questions for the owner

Choices the design left open while the trees were reshaped into spell branches (tasks 4.1 onward, branch `phase4`).
Each was built with the default given here; every one is easy to change. Added to as each tree is written.

## Tasks 4.1 to 4.3 (the schema, Fire, Lightning)

1. **Is an element's base spell free with its element?** The base spells (Flame Wave, Thunderclap, and the rest) are
   now a branch's Learn mark. Default: given with the element, never bought, so its Empower, Shape and Master can be
   bought at once. Alternatives: the Learn costs the usual 4 Shards (a new element then has no spell until bought); or
   it is free but its branch waits on the root.
2. **Are the Katana-bound marks refunded when they move to the Katana's Way?** Burning Crescent and Delayed Cuts leave
   the Fire and Lightning trees for the Way, which is paid in Memories. Default: a save keeps them under the same ids,
   with no refund, waiting for the Way (`RetiredMarks.Moving`). Alternatives: refund their 3 Shards now and give the
   Way's marks new ids; or refund in Shards when the Way arrives and leave the Way mark unbought.
3. **Fire keeps one keystone, Lightning two.** The cut list says each tree keeps two keystones beside Devotion, but
   Fire's passive table has only White Heat. Default: as the tables have it (Fire one, Lightning two; the chart test
   allows one or two). Alternatives: give Fire a second keystone, or let every tree have exactly two.
4. **Where the branches sit on the chart.** The pattern has three boughs with two stalks each. Default: Fire's middle
   bough holds Heat with Uraeus and Meteor Leap, Flame Wave grows from the root on the right bough; Lightning's Single
   Stroke grows from Ring of Drums' Learn and Ring of Drums from Stored Charge, so every line passes clear. Alternatives:
   any other placement on the same pattern (the pattern is proved; the placement is free).
5. **A Master that does nothing yet.** Seven Flashes' Master (cast within 0.3 s of a perfect dodge, no stand-still)
   needs a cast window and a change to the form. Default: the mark exists, and its details say it does nothing yet.
   Alternatives: hide it until built; or give it a stand-in (a perfect dodge gives mana while Seven Flashes is slotted).
6. **A saved Hand with no shapes.** Shapes are chosen at a rest fire and kept for the run. Default: applying a Hand
   that says nothing of shapes (one kept before any Shape was bought) keeps the shapes already chosen. Alternative: a
   Hand always sets the shapes, and one with none puts every spell back to its first shape.

## Task 4.x: Wind

1. **The three built majors the passive table leaves out.** Wind's passive table keeps four majors (Rising Cut, Passing
   Cut, Turned Aside, Against the Wall); the cut list does not name Rushing Wake, Empty Air or Hammer Down. Default: all
   three retired and refunded (3 Shards each), so the tree is the design's 34 marks. Alternatives: keep Hammer Down (the
   heavy blow that drives an airborne enemy down, the end of the Updraft juggle) in one of the two empty places, at 35
   marks; or fold its slam into Rising Cut.
2. **A shape that does nothing yet.** Bag of Winds' Kept Wind (the bag kept 20 s, opened when you choose) needs the bag
   itself, which the data cannot say yet: for now each perfect guard or dodge lets a wind out at once, so keeping changes
   nothing. Default: the shape exists and its details say it does nothing yet (as Seven Flashes' Master, question 5).
   Alternatives: hide the Shape mark until the bag is built; or give Kept Wind a stand-in (its cones break harder).

## Ice

1. **The two built majors the passive table leaves out.** Ice's passive table keeps four majors (Cold Read, Flying
   Shards, Brittle, Splinter Ward); the cut list does not name Gathering Frost or Clean Break. Default: both retired and
   refunded (3 Shards each), so the tree is the design's 34 marks. Alternatives: keep Clean Break (a shatter ends what
   has under a fifth of its health) in one of the two empty places on the right bough, at 35 marks; or fold it into All
   at Once.
2. **Sent Sliding takes a new id.** The design says Sent Sliding replaces Unmoved. Default: Unmoved (`ice.unmoved`) is
   retired and refunded, and Sent Sliding is a new keystone, `ice.sliding`, because the two do different things.
   Alternative: Sent Sliding keeps `ice.unmoved`, so a save that held Unmoved holds Sent Sliding without a refund.
3. **Stand-ins that are stronger than their timing.** Until the cast windows exist, Winter Wall's Master freezes with
   every burst (not only a heavy blow timed to an enemy's strike), and Rime Guard's Master freezes everything within
   3 m on any perfect guard, once in nine seconds (not only in the fog's first second). Default: as written, with
   **Not built yet** lines. Alternatives: give them nothing until the windows are built (as Seven Flashes' Master); or
   keep them and lengthen their limits.
4. **Cracked Wall on a clock.** The shape bursts 1.5 s after an enemy first strikes the wall, which needs a wall that
   knows it was struck. Default: it bursts by itself 3 s after it rises. Alternatives: a shape that does nothing yet,
   as Kept Wind (Wind, question 2); or the burst at once when an enemy first comes within its reach.

## Water

1. **Catch the Rain and Drawn From the Drowned.** The design says Drawn From the Drowned replaces Catch the Rain (a
   perfect guard fills part of a flask; a Soaked death does now). Default: Catch the Rain (`water.catch`) is retired and
   refunded, and Drawn From the Drowned is a new major, `water.drowned`, because the two answer different moments.
   Alternative: Drawn From the Drowned keeps `water.catch`, so a save that held Catch the Rain holds it without a refund.
2. **The three built majors the passive table leaves out.** Water's table keeps five majors; Flowing Guard, Unbroken
   Chain and Spilled Water are not in it and not in the cut list. Default: all three retired and refunded (3 Shards each),
   so the tree is the design's 34 marks; Spilled Water's ring on drinking lives on in Wellspring's The Flask Breaks
   (in the ledger, not built). Alternative: keep Spilled Water in one of the two empty places until The Flask Breaks is built.
3. **One keystone.** The passive table gives Water only Deep Well; the cut list says each tree keeps two. Default: one
   keystone (the chart tests allow one or two), Spring Tide and River Without End retired. Alternative: keep Spring Tide
   aimed at the larger spell and the surge alone, as the cut list offers, in the empty place at the top of the trunk.
4. **Whirlpool's size.** The roster says "as built: ... 4 m across", but the built whirlpool is 4 m in radius (8 across).
   Default: as built, 4 m in radius, and Maelstrom 2.5 m in radius. Alternative: 2 m in radius as the words say, and
   Maelstrom 1.25 m.
5. **Poured Blade's price.** The roster says 30 mana (Empower 25), under the shared rule's 35 to 50 for a larger spell.
   Default: the roster's 30 (the tests allow 30 to 60). Alternative: 35, Empower 30, like Ice's Winter Wall.
6. **Stand-ins that are stronger than their timing.** Until held presses and cast windows exist, Blowhole's Master
   doubles every answer (it comes in the instant of the blow as built) and ends its own clock once in twenty seconds, and
   The Sea Comes In makes the dodge half again as far and the clocks 30% faster everywhere, not only in your water.
   Default: as written, with **Not built yet** lines. Alternatives: give them nothing until the pieces are built; or halve
   the numbers.
7. **Breaking Swell throws "down".** "Throwing everything in it down 3 m off". Default: thrown 3 m away and knocked down
   for 0.6 s (King Tide's time). Alternative: thrown only, nothing knocked down.

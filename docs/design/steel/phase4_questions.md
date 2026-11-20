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

## Life

1. **Two marks called Briar Patch.** Life's base spell is a mark now, and Overgrowth's mix major
   (`overgrowth.briarpatch`) had the same name. Default: the mix mark is renamed Thorns Take Root (its id kept), so
   the base spell keeps its kit's name. Alternative: another name for the mix mark, or the base spell's mark named
   apart from its kit.
2. **The built majors the passive table leaves out.** Thin Blood (replaced by The Garden Holds You, as the table says)
   and Shade are not in the table. Default: both retired and refunded (3 Shards each), and The Garden Holds You is a new
   major, `life.garden`, so the tree is the design's 34 marks. Alternative: keep Shade (mending near what you grew) in
   one of the two empty places, now that Elder Tree mends inside its own ring.
3. **One keystone.** The passive table gives Life only Wildwood; the cut list says each tree keeps two. Default: one
   keystone (the chart tests allow one or two), Heartwood, Paid in Blood and Shared Roots retired. Alternative: keep
   Paid in Blood aimed at the surge alone, as the cut list offers, in the empty place at the top of the trunk.
4. **Shapes that cannot set their Learn aside yet.** Golden Sickle keeps the garden standing, Feed the Ground withers
   nothing, and Strangler and Dodder come in place of the sapling; a shape cannot yet switch off its Learn's effects.
   Default: as written, the Learn's burst, withering and saplings still happen beside the shape, with **Not built yet**
   lines. Alternatives: give those shapes nothing until the piece is built (as Kept Wind); or build that piece (a shape
   naming the Learn effects it sets aside) before Life is played.
5. **Drawn From the Wood's window.** The design gives 1.5 s of green blood for each grown thing withered, 10 s at most;
   nothing yet counts what a burst of the garden spent. Default: 6 s, given only when cast within 4 m of something you
   grew. Alternatives: 4.5 s (three things' worth) or 10 s, given always.
6. **Stand-ins that are stronger than their timing.** Until the windows and counts exist, Sapling's Master raises a
   small Elder Tree on any perfect guard once in 15 s (not on the third sapling within 3 s), and Drawn From the Wood's
   Master waives every health price for a second after any blow lands on you (Dark's prices too, while it is slotted).
   Default: as written, with **Not built yet** lines. Alternatives: give them nothing until the pieces are built; or
   lengthen their limits.
7. **Where Elder Tree stands.** The roster does not say; the built tree was planted 2.5 m ahead. Default: at your feet,
   so its ring of Seeded ground and the roots lie together (the ring is laid round the player). Alternative: 2.5 m ahead,
   with the ring moved there once a hazard can be laid at a spell's own point.
8. **The Garden Holds You before its footing exists.** The player's flinch is kept by hand in `PlayerUpgrades`
   (`PlayerDefence.Unflinching`). Default: wounds take a fifth less within 4 m of something grown, no change to
   `PlayerUpgrades`. Alternative: a one-line hook there that keeps the player unflinching near the garden (no throw, pull
   or knock-down guard yet).
9. **How long burrs ride.** Burrs give no time. Default: 6 seconds (a tuft every 1.5 s, four tufts), six tufts at most.
   Alternative: as long as Seeded lasts (8 s).

## Earth

1. **Three keystones in the table, two kept.** Earth's passive table gives Immovable, The Slow Hand and The Closed Ring;
   the cut list says each tree keeps two beside its Devotion, and the chart tests allow one or two. Default: Immovable
   and The Slow Hand kept (they change how you fight), The Closed Ring retired and refunded (6 Shards), so the tree is
   33 marks. Alternatives: keep The Closed Ring and cut one of the other two; or keep it as a major (3 Shards) in the
   empty place at the top of the trunk.
2. **Mountain Stance's Empower has no shorter wait.** The roster gives it "3 s; 25 stagger a blow turned", but every
   Empower is "stronger, shorter cooldown" by the shared rule, and the chart tests ask for one. Default: it waits 9
   seconds, not 10, as well. Alternative: keep 10 seconds and let the test allow an Empower that only lengthens the stance.
3. **Cairn's "plus 10 more".** Each stone lands "for 12 damage and 15 stagger plus 10 more". Default: 10 more damage
   once for the heap on each shaken enemy. Alternatives: 10 more stagger a stone; or nothing beyond 12 and 15 a stone.
4. **The Slow Hand is the ring's cheapest keystone.** It grows from Heavy Hand on the middle bough, so it is reached for
   11 Shards (Immovable, grown from Mountain Stance's Learn, for 15). Default: as placed; two keystones still cost more
   than a run. Alternative: grow The Slow Hand from Cairn's or Under the Mound's Learn (15 Shards).
5. **Stand-ins that are stronger than their timing.** Until the moments exist, Under the Mound's Master gives a perfect
   dodge's reward and a stronger next blow on every cast; Quake Step's Master ends its wait on any stamp that lands, once
   in 18 s; Mountain Stance's Master holds the attacker of any blow turned and ends its wait once in 20 s; Stone Rise's and
   Cairn's Masters ask only that you are Rooted (three seconds without a dodge), not still for a second; Stone Remembers
   strikes as if into stone on any heavy blow, once in 3 s. Default: as written, with **Not built yet** lines.
   Alternatives: give them nothing until the pieces are built; or lengthen their limits.
6. **Standing Stones before stones stand.** The design re-aims it (two of your stones stand at once, and an enemy
   driven into one is hurt and breaks it); nothing yet stands on the field. Default: it keeps its built pillar on what a
   spell strikes, with its built words, and the design's in its details. Alternative: the design's words now, as
   Stone Remembers has, with the pillar as the stand-in.

## Metal

1. **Three keystones in the table, two kept.** Metal's passive table gives Edge Guard, Master's Answer and All Blades
   Home; the cut list says each tree keeps two beside its Devotion. Default: Edge Guard and Master's Answer kept (both
   answer the perfect guard, Metal's own timing), All Blades Home retired and refunded (6 Shards): its blades that never
   fade need standing blades nothing has yet, and To the Blade and Grave of Swords now call blades home. Alternatives:
   keep All Blades Home and cut Master's Answer (the one that changes the weapon's skill, which the Way may own now); or
   keep it as a major (3 Shards) in the empty place at the top of the trunk.
2. **Four Empowers with no shorter wait in the roster.** Flung Blade's, Circle of Steel's, Returning Edge's and Poured
   Bronze's Empowers give only stronger numbers, but every Empower is "stronger, shorter cooldown" and the chart tests ask
   for one. Default: each waits a second less (5, 9, 8 and 11 seconds), as Earth's Mountain Stance. Alternative: keep
   their waits and let the test allow an Empower that only strengthens.
3. **Returning Edge before a turning blade exists.** No form flies out and back to the hand yet. Default: a lane of
   spikes broken open along the ground for the outward pass (12, 10, a notch), and each enemy it struck cut again a
   second later for the way home. Alternatives: a returning projectile from the mark alone (it would not count as the
   spell striking, so its shapes and the charged blade would not answer it); or keep the built lunging head until the
   form is built.
4. **Shapes that reach more than the design's enemies.** Poured Bronze's Set and Red-Hot crust every enemy the pour's
   eight metres reached, not only the notched ones poured into (the shape answers after the pour has spent the notches);
   The Golden Chair answers with a 90-degree cone at the attacker and binds everything the cone strikes. Default: as
   written, with **Not built yet** lines. Alternatives: give those shapes nothing until the pieces are built; or narrow
   them (Set and Red-Hot answering only a heavy blow's or a weapon blow's own enemy within two seconds of the pour).
5. **Stand-ins that are stronger than their timing.** Until the moments exist, Flung Blade's Master ends its wait on
   every perfect guard (as Thunderclap's does); Returning Edge's Master on any perfect guard once in 9 s; Bat It Home
   throws a blade out and back on any perfect guard once in 2 s; Circle of Steel's Master gives its mana and reset for any
   blow turned once in 20 s; Grave of Swords' Master gives 20 mana for any heavy blow on an enemy near the cast, once a
   cast. Default: as written. Alternatives: give them nothing until the pieces are built; or lengthen their limits.
6. **The Guard Holds before a guard that cannot break.** Default: your guard costs half the stamina (so it breaks far
   later), you take 15% less, and each blow held on it notches and sparks the attacker. Alternative: no stamina change
   until the unbreakable guard is built.
7. **Two readings of the roster.** The Nail's "a pool of 3 m": default 3 m across (1.5 m round). The Tripods' "four
   blades on each, cutting twice a second": default three helpers that cut for 6 s. Alternatives: a pool 3 m round; four
   cuts each (2 s), then the tripod is spent.

## Light

1. **Three keystones in the table, two kept.** Light's passive table gives Unblemished, Two Suns and Condemned; the cut
   list says each tree keeps two beside its Devotion. Default: Unblemished and Condemned kept (the clean fight and the
   judgement on the marked, Light's own substance), Two Suns retired and refunded (6 Shards): its price, spells costing a
   quarter more, is broken by cooldowns (only Daybreak pays mana now), and a figure of light casting your spells again
   is close to Halo's mirrors. Alternatives: keep Two Suns and cut Condemned (its finish-at-any-balance is not built
   either); or keep Two Suns as a major (3 Shards) in the empty place at the top of the trunk, with a new price (spells
   wait a fifth longer).
2. **Three Empowers with no shorter wait in the roster.** Ring of Spears', Standing Light's and Reckoning's Empowers
   give only stronger numbers. Default: each waits a second less (9, 11 and 11 seconds), as Metal's. Alternative: keep
   their waits and let the test allow an Empower that only strengthens.
3. **Daybreak's Empower has no price in the roster.** It gives "10 damage a ring", and the shared rule asks a larger
   spell's Empower for a lower price. Default: 40 mana, not 45, and the 10 a ring left for later (set here it would be
   set on Red Before the Sun's and High Noon's own numbers too). Alternative: 45 mana kept, and the 10 a ring only.
4. **Stand-ins that are stronger or wider than their timing.** Until the moments exist: Glare's Master holds and Marks on
   every perfect dodge once in 6 s (Dazzle as built, a little longer), whether or not Glare is cast; Step Into Light's
   Master ends its wait on any step that cuts, once in 16 s; Ring of Spears' Master answers any weapon blow while the
   spears hang, not only a critical one; Standing Light's burst comes on the three perfect guards after each lamp,
   anywhere, and its Master resets every Light spell on any perfect guard once in 12 s; Daybreak's Master gives a ward
   on every cast. Default: as written, with **Not built yet** lines. Alternatives: give them nothing until the pieces
   are built; or lengthen their limits.
5. **Reckoning's count before a damage count exists.** Default: 8 damage for each weapon blow landed on the sealed enemy
   in its six seconds, and 40 stagger, so quick weapons fill it as heavy ones would by damage. Alternatives: 12 a blow
   with heavy blows counted twice; or the built brand (each of six blows sets off its own burst) until the count is built.
6. **The Drawn Line before a line on the floor exists.** Default: the spears still drive in, then stand five seconds in
   the floor in their ring round the enemy, Marking and jarring what is inside. Alternative: the wall form (Red Before the Sun's) laid six
   metres across your front (nothing crosses it, and a heavy blow bursts it), nearer the picture but blocking where the
   design holds.

## Dark

1. **The Rise stays with the gift for now.** The design moves the Rise to Dark's root and leaves the gift the finisher
   that buys it back. As built, `PlayerUpgrades.Rise` asks for the gift's id, its cast is the gift's (RiseForm ends the
   Rise when that mark is not held), and RiseForm's buy-back is part of the same form: moving it is three lines in two
   files, not data or one hook. Default: the gift, A Life for a Life, keeps giving the Rise and its buy-back as built,
   and the root only curses (a NEEDS, `rise-on-root`). Alternative: allow the hook now (Rise asks for the root, the cast
   names the root, RiseForm's buy-back asks for the gift), after main's PlayerUpgrades work tonight.
2. **Pawned Heart's Empower has no shorter wait in the roster.** It gives 70% and a ring of 12 seconds; the shared rule
   asks an Empower for a shorter wait or a lower price, and the heart is paid in health. Default: it waits 36 seconds,
   not 40, beside the roster's numbers. Alternative: keep 40 and let the test allow an Empower that only strengthens a
   spell paid in health.
3. **Blood Price before a cast on cooldown exists.** Default: it keeps its built rule (the mana a spell lacks paid in
   health, grey), which touches only duo spells and another element's larger spell now. Alternative: hold the mark out
   of the tree (an empty place) until the cast on cooldown is built.
4. **Stand-ins that are wider than their timing.** Until the moments exist: Grasp's Master answers any blow of yours on
   the attacker within a second of being struck; Devouring Maw's Master breaks what every bitten enemy began and resets
   once in 16 s; Gone to Ash's Master answers any blow scattered, once in 20 s; Second Shadow's Master gives a ward on
   every cast; Pawned Heart's Master heals 10 on any perfect dodge, once in 3 s; Debts Called's Master and its Learn's
   step strengthen every spell for the second after the cast, not only the burst. Default: as written, with **Not built
   yet** lines. Alternatives: give them nothing until the pieces are built; or lengthen their limits.
5. **Pawned Heart's shapes before the heart can be set elsewhere.** Default: both still set the heart behind you;
   Buried Heart lays a ring of ash that Curses for the heart's 10 s, Hung From the Tree keeps the enemy faced Cursed for
   them. Alternative: leave both shapes doing nothing beyond the plain heart until `heart-places` is built.
6. **Devouring Maw's jaw no longer Curses.** The built jaw Cursed what it struck, so every bite would find a Curse to
   spend; the roster's "on a Cursed enemy it spends the Curse" asks that it was Cursed before. Default: the jaw lays no
   Curse. Alternative: it Curses after the spend, so a second bite in the same fight feeds.

## Poison

1. **Three keystones in the table, two kept.** Poison's passive table gives Slow Death, Volatile and Plague Bearer; the
   cut list says each tree keeps two beside its Devotion. Default: Slow Death and Volatile kept (both change the beat,
   Poison's own timing, and PlayerUpgrades already keeps their rules), Plague Bearer retired and refunded (6 Shards):
   its aura of doses overlaps Miasma's Censer and the Devotion's fighting in the mist. Alternatives: keep Plague Bearer
   and cut Volatile or Slow Death; or keep it as a major (3 Shards) in the empty place at the top of the trunk.
2. **Four Empowers with no shorter wait in the roster, and One by One's with no price.** Five Needles', Coiled
   Strike's, Blister's and Share Alike's Empowers give only stronger numbers or reach. Default: each waits a second less
   (8, 9, 9 and 8 seconds), as Metal's and Light's. One by One's gives "7 damage a dose": default 45 mana, not 50, with
   the 7 a dose left for later (it is a number of the Learn's own burst, which an Empower cannot reach yet).
   Alternatives: keep their waits and let the test allow an Empower that only strengthens; 50 mana kept for One by One.
3. **The beat Masters test each enemy as it is struck.** A cast cannot be tested against the beat yet (a cast has no
   enemy of its own). Default: Miasma's, One by One's and Share Alike's Masters answer each enemy struck within a fifth
   of a second of its own bite, not the cast made on the beat of any enemy near. Alternatives: give them nothing until
   the cast can be tested; or test the enemy faced at the cast.
4. **Stand-ins wider than their timing.** Until the moments exist: Coiled Strike's Master ends its wait on any perfect
   dodge, once in 10 s; Shed Skin blinds the enemies near where you stood on every cast, not only a let-go in the
   perfect window; Tasted Every Herb heals 2 a second and adds a dose to every lay in every fight, not only in a cloud.
   Default: as written, with **Not built yet** lines. Alternatives: give them nothing until the pieces are built; or
   halve the Devotion's healing until clouds can be stood in.
5. **Five Needles as one fan.** No form throws five separate needles yet. Default: one narrow cone with the five
   needles' worth (15 damage, 50 stagger, five doses), and the first enemy struck pinned, once a cast. Alternative: the
   bullets form (five things closing on the enemy faced from a ring), which strikes five times but from all sides.
6. **Three readings of the roster.** Centipede's Run's "a bite on each": default a strike on each, 14 damage and 15
   stagger (one fang's), two doses. Shed Tail's "every 0.5 s for 3 doses": default three doses at each lash. Low Mist's
   sheet: default a row of six patches, each 2.5 m across. Alternatives: a bite of the poison on each; three doses over
   the tail's whole time; one long patch.
7. **Fever's new healing is not a mark.** The design raises Fever's healing to 40% of poison's and doubles its beat
   now the root heals. Default: Fever unchanged here (it is the kit's surge, not part of the tree); the root's tenth is
   built. Alternative: change the kit's Fever numbers in the same pass as the other surges.

# Blade lines on the weapon prefabs

What the swing looks follow on each weapon: the line the weapon builder measured off the model (`BladeLineFit`), against the model notes' line (`BladeLines.Defaults`). Points are in the weapon's own space (grip at the origin, business end along +Z).

| Carry | Part | Kind | Points on the prefab | Notes' points | Base off (m) | Tip off (m) |
|---|---|---|---|---|---|---|
| gsword | edge | Edge | (0.00, -0.25, 0.13) (0.00, -0.25, 1.20) (0.00, 0.13, 1.58) | (0.00, -0.17, 0.14) (0.00, -0.07, 1.50) | 0.08 | 0.22 |
| scythe | edge | Edge | (-0.01, 0.11, 0.66) (-0.01, -0.56, 0.47) (0.01, -0.89, -0.03) | (0.00, -0.02, 0.78) (0.00, -0.55, 0.66) (0.00, -0.92, -0.02) | 0.18 | 0.03 |
| spear | leaf | Point | (0.00, 0.00, 1.55) (0.00, 0.00, 2.04) | (0.00, 0.00, 1.45) (0.00, 0.00, 2.00) | 0.09 | 0.04 |
| sunspear | leaf | Point | (0.00, 0.00, 1.41) (0.00, 0.00, 2.04) | (0.00, 0.00, 1.50) (0.00, 0.00, 2.00) | 0.09 | 0.04 |
| hammer | head | Head | (0.00, 0.02, 0.90) (0.00, 0.02, 1.26) | (0.00, -0.08, 0.88) (0.00, -0.08, 1.27) | 0.10 | 0.10 |
| knife | edge | Edge | (0.00, -0.06, 0.08) (0.00, -0.06, 0.31) (0.00, 0.00, 0.39) | (0.00, -0.05, 0.08) (0.00, -0.01, 0.37) | 0.01 | 0.02 |
| bow | upper limb | Stave | (0.04, -0.03, 0.06) (-0.02, 0.30, 1.19) | (0.00, 0.00, 0.07) (0.00, 0.13, 0.76) (0.00, 0.31, 1.18) | 0.04 | 0.03 |
| bow | lower limb | Stave | (0.04, -0.03, -0.07) (-0.04, 0.03, -0.31) (-0.02, 0.30, -0.64) | (0.00, 0.00, -0.07) (0.00, 0.19, -0.50) (0.00, 0.31, -0.63) | 0.04 | 0.03 |
| arrow | head | Point | (0.00, 0.00, 0.55) (0.00, 0.00, 1.10) | (0.00, 0.00, 0.55) (0.00, 0.00, 1.10) | 0.00 | 0.00 |
| sickle | edge | Edge | (0.00, -0.04, 0.10) (0.00, -0.23, 0.08) (0.00, -0.34, -0.16) | (0.00, -0.02, 0.20) (0.00, -0.25, 0.15) (0.00, -0.34, -0.16) | 0.10 | 0.00 |
| weight | weight | Head | (0.00, 0.00, 0.00) (0.00, 0.00, 0.22) | (0.00, 0.00, 0.00) (0.00, 0.00, 0.22) | 0.00 | 0.00 |
| katana | edge | Edge | (0.00, -0.02, 0.13) (0.00, 0.04, 0.99) | (0.00, -0.02, 0.13) (0.00, -0.01, 0.99) | 0.01 | 0.06 |
| fan | rim | Rim | (0.01, 0.00, 0.09) (0.01, 0.00, 0.41) | (0.00, 0.00, 0.00) (0.00, 0.00, 0.41) | 0.10 | 0.01 |

Bare hands and the Armament sweep with the body: fist R (forearm.R to hand.R and 0.09 m on), fist L (forearm.L to hand.L and 0.09 m on), foot R (shin.R to foot.R and 0.12 m on), foot L (shin.L to foot.L and 0.12 m on).

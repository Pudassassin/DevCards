## Patch Notes
<details>
<summary>Public Beta 4-0 [V0.4.0]</summary>

### Implemented [Booster Pack] mechanic:

- added **\[Vintage Gears]** redraw and pick 3 vanilla cards of uncommon or lower rarity.
- added **\[Veteran's Friend]** redraw and pick 1 guaranteed rare vanilla card.
- added **\[Supply Drop]** do nothing now, next round's card pick you get to pick +3 more cards of Uncommon or lower rarity.
- updated **\[Glyph CAD Module]** to give 1 Glyph card of your choice.
- added **\[Pure Canvas]** -- a \[Shuffle] that excludes Glyph, Spell and Magick cards from appearing.

### Additions:

- added **[Replication Glyph]** -- more gun projectiles! MOAR SPELL PROJECTILE!!
- added **[Mystic Missile]** -- a passive spell card that enchants gun-fired bullets with explosive arcane energy, scales with glyphs and additional copies.

### Other changes:

#### Buffed [Aracane Sun]

- stronger start + ramp up rate
- update beam and sun visual codes
- fixed delayed activation at battle start

#### Rebalance [Flak Cannon]

- severely limiting primary gun firerate, bursts and #projectiles gains
- now only some shrapnels carry effects while the rest is only carrying basic bullet stats

#### Buffed [Anti-Bullet Magick]

- affecting larger area and lasting longer at base level

#### Scaled up [Tiberium Bullets]

- inflicts more life drains proportional to gun's damage
- also incurs more self life drains on pick up

#### Misc.

- Reworked how **\[Arc of Bullets]** and **\[Parallel Bullets]** spread the bullets and fixed the issue with burst-fire guns

</details>

<details>
<summary>Public Beta 3-0 [V0.3.1]</summary>

- added \[Bullets.rar] trim down bullet spams (projectile counts) in exchange for more damage per bullets
- added \[Guardians Charm] boosts block card draw chance and 'block cards draw more block cards' (no longer the card pack's default mechanic)
- added \[Lifeforce Duorblity] orb spell of mobile Heal & DMG zone
- added \[Lifeforce Blast!] orb spell with explosive Heal/Damage and then boost/hinder healings afterward
- added \[Arcane Sun] spell that passively deal ramping DPS + "Damage Amp" debuff
- added \[Portal Magick] Unique Magick, "Now you're thinking with Portals"

- buffed \[Orb-literation] increased base radius and amount of Max HP culls on impact
- Reworked \[Parallel Bullets] make it so it properly arranges in parallel and scales with gun spread & proj. counts

- Spells passively boost glyphs' draw chances
- Spells compatible with controller
- Improved/fixed issues with card draw rarity/weight adjustment
- Block-based ability cooldowns start at 0.5s at battle start; should be available when grace period is over.

</details>

<details>
<summary>Public Beta 2-0 [v0.2.0]</summary>

- added \[Orb-Literation] and dependencies: map destruction, Max HP culls on impact
- added \[Tiberium Bullet] and dependencies: caustic HP removal bullet modifier
- added \[Arc of Bullets] evenly spread bullets in arc
- added \[Parallel Bullets] neatly arranged and focused bullets

- added \[Divination Glyph] velocity/trajectory. speed to both Spell and Bullets
- added \[Influence Glyph] spell range/AoE upgrade
- added \[Geometric Glyph] bounces to both Spell and Bullets
- added \[Potency Glyph] spell power, add raw damage to Bullets

- reworked \[Size Normalizer] to utilze patch instead of MonoBehavior; works instantly and reliably

- rebalanced all of the initial release cards
- all block ability cooldowns start at 2.0s at battle starts

Under the hood:
- Implemented Hollow Life mechanic to handle temp HP caps incurred by **\[Orb-Literation]** and possibly future cards

- **\[Chompy Bullet]** and any future bullet modifiers only add one instance of the effect to each bullet, they will calculate the effect on the fly

- Disabled redundancy system that iterate and resolve unique and/or mutually exclusive cards (it will be other mods' faults that violates the backbones of the system)
</details>

<details>
<summary>Public Beta 1-2 [v0.1.13]</summary>

- Under the hood reworks of healing and damage multipliers.

- **\[Tactical Scanner]** now properly modify **healings** and **damages** taken and ignore all **direct health changes**.

- **\[Hollow Life]** changes:
  - rarity changed to **Rare**
  - Max HP gains changed from **x2.5** to **3x**
  - now giving **-15% healing effects**; reducing healing and regeneration

- Reduced the delay caused by card conflict resolver at the start of each round.
</details>

#### Public Beta 1-1 \[v0.1.9]
- Patched the logic behind the monobehavior that manages and prevents card conflicts, to execute from the main mod class instead of from each players!
- **\[Shield Battery]** sneak peak

#### Public Beta 1-0 \[v0.1.0]
- It all begins. Starting out with 5 wacky cards and 3 minor all-around passive cards

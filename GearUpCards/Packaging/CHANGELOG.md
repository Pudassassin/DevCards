## Patch Notes
### Public Beta 3-0 \[V0.3.1]
- added \[Bullets.rar] trim down bullet spams (projectile counts) in exchange for more damage per bullets
- added \[Guardians Charm] boosts block card draw chance and 'block cards draw more block cards' (no longer the card pack's default mechanic)
- added \[Lifeforce Duorblity] orb spell of mobile Heal & DMG zone
- added \[Lifeforce Blast!] orb spell with explosive Heal/Damage and then boost/hinder healings afterward
- added \[Arcane Sun] spell that passively deal ramping DPS + "Damage Amp" debuff
- added \[Portal Magick] Unique Magick, "Now you're thinking with Portals"

- buffed \[Orb-literation] increased base radius and amount of Max HP culls on impact
- Reworked \[Parallel Bullets] make it so it properly arranges in parallel and scales with gun spread & proj. counts

- Spells now passively boost glyphs' draw chances
- Spells now compatible with controller
- improved/fixed issues with card draw rarity/weight adjustment

#### Public Beta 2-0 \[v0.2.0]
- added \[Orb-Literation] and dependencies: map destruction, Max HP culls on impact
- added \[Tiberium Bullet] and dependencies: caustic HP removal bullet modifier
- added \[Arc of Bullets] evenly spread bullets in arc
- added \[Parallel Bullets] neatly arranged and focused bullets

- added \[Divination Glyph] velocity/trajectory. speed to both Spell and Bullets
- added \[Influence Glyph] spell range/AoE upgrade
- added \[Geometric Glyph] bounces to both Spell and Bullets
- added \[Potency Glyph] spell power, add raw damage to Bullets

- reworked [Size Normalizer] to utilze patch instead of MonoBehavior; works instantly and reliably

- rebalanced all of the initial release cards

- rebalanced [Tactical Scanner]
  - AoE radius: 12 (+1.5 per stack)
  - Damage/Heal Amp: 50% per stack
  - Duration: 6 seconds (+1 per stack)
  - Cooldown: 9 seconds (-0.5 per stack)

- rebalanced [Chompy Bullet]
  - ~15% > ~20% HP Culling; per bullet-- at default gun firerate
  - -25% > -15% DMG, ATKSPD and Reload SPD
  - Stackable with diminishing return

- completed [Shield Battery]
  - 2 Empowered shot capacity
  - no longer give an extra block
  - +0.5s block cooldown
  - -25% attack speed

- all block ability cooldowns start at 2.0s at battle starts

Under the hood:
- Implemented HollowLifeEffect mono to handle temp HP caps incurred by \[Orb-Literation] and possibly future cards

- Chompy Bullet now only add one instance of the effect to each bullet, they will calculate the effect on the fly

- Disabled redundancy system that iterate and resolve unique and/or mutually exclusive cards (it will be other mods'
  faults that breach allowMultiple/black/whitelisting system)

#### Public Beta 1-2 \[v0.1.13]
- Under the hood reworks of healing and damage multipliers.

- **\[Tactical Scanner]** now properly modify **healings** and **damages** taken and ignore all **direct health changes**.

- **\[Hollow Life]** changes:
  - rarity changed to **Rare**
  - Max HP gains changed from **x2.5** to **3x**
  - now giving **-15% healing effects**; reducing healing and regeneration

- Reduced the delay caused by card conflict resolver at the start of each round.

#### Public Beta 1-1 \[v0.1.9]
- Patched the logic behind the monobehavior that manages and prevents card conflicts, to execute from the main mod class instead of from each players!
- **\[Shield Battery]** sneak peak

#### Public Beta 1-0 \[v0.1.0]
- It all begins. Starting out with 5 wacky cards and 3 minor all-around passive cards

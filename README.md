# GearUP Cards [0.2.0] 
Public Release 0-2-0, build #141

Adding in game-changing mechanics into the modded ROUNDS fray! Bought to you by Pudassassin

## **Cards**

### **Rare Cards**
#### \[Anti-Bullet Magick]
- **Unique Magick** - *Spell caster*

- *Blocking casts the spell that deletes all nearby bullets in flight and also in guns' magazines, including **yours!***

- All players affected suffer **additional 3.5s reload time** on top of their own guns' reload time.

- Caster suffer less penalty from above.

- \[Magick Fragment] reduce cooldown and lessen reload penalty on self.

- \[Potency Glyph] increase zoning duration and force-reload time added.

- \[Influence Glyph] increase effect radius.

#### \[Hollow Life]
Health Passive

- *Puffs up your total Max HP; you no longer heal as effective and to full health.
**(Stack multiplicatively)***

- Max HP is **multiplied by 3**, but also gain **-25% HP Cap** to the current health and **-15% to healing effects**. (Heals and Regens)

**Tip: Look for the cards that benefit from having larger Max HP pool, or having you to stay low on health, but definitely not \[Pristine Perserverance]!*

### **Uncommon Cards**
#### \[Chompy Bullet]
**Passive Bullet Modifier**

- *Bullets deal bonus damage based on target's current health. Effect varies with gun's firerate.\**

- This will make your gun slightly harder to shoot but will do wonder against the hearty tankers!

*The effect will spread and dilute out the more bullet you can shoot out per second

#### \[Tactical Scanner]
**Active Block** - *Gear equipments*

- *Blocking scans nearby players' power: Scanned enemies take more damage. Scanned friendlies heal for more.*

- Also displays a quick summary of the scanned players' combat stats.

- Ability is improved for additional copies of the card

#### \[Size Normalizer]
**Unique Size Modification**

- *Set your final player size to default, retaining the current mass and gravity.*

- For when you are going **too BEEG** or **too smol** to your liking, moderately boost to health and movement speed.

#### \[Arc of Bullets]
**Unique Gun Spread Modification**

- *Add and evenly spread bullets in an arc formation.*

- also provides +4 projectiles, +60 degree spread and -35% damage penalty

#### \[Parallel Bullets]
**Unique Gun Spread Modification**

- *Add and convert bullets spread into a parallel formation. Width scales with projectile counts.*

- the bullets fired will fan out initally before arranging into uniformed trajectory

- also provides +4 projectiles, initial spread and -65% damage penalty

#### \[Tiberium Bullets]
**Passive Bullet Modifier** - *Tiberium Prophecy*

- *Bullets inflict "Tiberium poisoning" on hit, causing burst of HP losses and 'chronic' HP loss, last until respawn (not just one of the revives).*

- +100% of bullet's damage dealt over 4 second burst **as HP Removal** on hit

- inflict +5% bullet's damage, +0.05% target's HP and +0.35 **chronic flat HP Loss per second** on each bullet hit

#### \[Orb-literation!]
**Active Orb Spell** - *Orb sage*

- *Blocking fires the orb that obliterates part of the map and reduce players' max HP.*

- ALL PLAYERS in the impact area suffer -10% max HP (-15% on direct hit), **last until respawn**

- \[Magick Fragment] reduce cooldown and burst-casing delay.

- \[Potency Glyph] increase percentage of MAX HP cullings.

- \[Influence Glyph] increase effect radius.

- \[Geometry Glyph] allowing the orb to bounce +1 time per stack.

- \[Divination Glyph] increase overall velocity of the orb.

- *Addional copies -* increase number of orbs cast. (2 orbs on 2 copies, every 2 additional copies give +1 orb)

### **Common Cards**
#### \[Medical Parts]
**Health Passive** - *Material*

- *Some hearty doodads to help you survive longer.*

- simply giving you +50% Health and 2 HP/s regen.

*\* Material cards can be obtained normally or as result of having conflicting cards together.*

#### \[Gun Parts]
**Gun Passive** - *Material*

- *Attachments and enhancements to up your gun games.*

- simply giving +20% Damage, +20% ATK SPD and +3 to Max Ammo to your gun

#### \[Magick Fragments]
**Spell Passive** - *Glyph*

- *This mysterious glyph hasten your spellcasting, but at what cost?*

- on its own, it gives *-25%* and *-0.1s* Block Cooldown at the cost of -25% Health.

#### \[Divination Glyph]
**Spell Passive** - *Glyph*

- *Your Bullets and Spells reach a little further AND quicker!*

- on its own, it gives +25% Bullet (velocity) Speed and +15% Projectile (simulation) Speed.

#### \[Geometry Glyph]
**Spell Passive** - *Glyph*

- *"Simple Geometry!"*

- on its own, it gives +3 Bullet Bounces and +0.15s Reload Time

#### \[Influence Glyph]
**Spell Passive** - *Glyph*

- *Improve your Spells' range and effect area.*

- only available when you pick at least 1 'Spell' card-- it does nothing without one!

## Note from the modder
There will be more cards planned to expand in each category, here the second beta release give more contents and introducing 'Orb Spell', as well as bug fixes, rebalancing the first beta cards and framework updates

## Patch Notes
#### Public Beta 2-0 \[v0.2.0]
- added [Orb-Literation] and dependencies: map destruction, Max HP culls on impact
- added [Tiberium Bullet] and dependencies: caustic HP removal bullet modifier
- added [Arc of Bullets]: evenly spread bullets in arc
- added [Parallel Bullets]: neatly arranged and focused bullets

- added [Divination Glyph]: velocity/trajectory. speed to both Spell and Bullets
- added [Influence Glyph]: spell range/AoE upgrade
- added [Geometric Glyph]: bounces to both Spell and Bullets
- added [Potency Glyph]: spell power, add raw damage to Bullets

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
- Implemented HollowLifeEffect mono to handle temp HP caps incurred by [Orb-Literation] and possibly future cards

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

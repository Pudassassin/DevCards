# GearUP Cards [0.1.13] 
Public Release 0-1-2, build #85

Adding in game-changing mechanics into the modded ROUNDS fray! Bought to you by Pudassassin

## Cards

### Rare Cards
#### \[Anti-Bullet Magick]
Unique Magick - *Spell caster*

- *Blocking casts the spell that deletes all nearby bullets in flight and also in guns' magazines, including **yours!***

  - \[\+] Deletes Bullets
  - \[+/-] +3s Forced Reload
  - \[--] +1.5s Block Cooldown
  - \[--] 15s *Spell* Cooldown

1. Anti-bullet zone stays effective for 1 second where you cast or where your empowered shot landed.
2. Affected players are forced to reload +3s on top of their normal reload time. Only apply at the moment spell is cast.
3. \[-1.5s] cooldown per \[Magick Fragments] card you have, down to 7s minimum.

***You can only have one Unique Magick in posession!***
- The extras you obtain are converted to \[Magick Fragments] or potentially lost!

#### \[Hollow Life]
Health Passive

- *Puffs up your total Max HP; you no longer heal as effective and to full health.
(Stack multiplicatively)*

  - \[+] x3 Max HP
  - \[--] -25% Health Cap
  - \[--] -15% Healing Effects

1. Affected all sources of health gain: Heal, Life Steal, Regeneration.
2. Any health gained above the cap incurred by this card is quickly removed.

**Tip: Look for the cards that benefit from having larger Max HP, or lower current HP.*

### Uncommon Cards
#### \[Chompy Bullet]
Bullet Passive

- *Bullets deal bonus damage based on target's current health. Effect varies with gun's firerate.\**

  - \[\+] +15% HP Culling\*
  - \[--] -25% Damage
  - \[--] -25% Attack Speed
  - \[--] -25% Reload Speed

1. The value shown is the potential bonus damage per shot of the default gun with one stack of this card [~ 3 bullet per second]
2. The more bullets you fire per second, the weaker this card's effect becomes (higher ATK SPD, projectile counts and/or burst counts)

#### \[Tactical Scanner]
Block Active - *Gear equipments*

- *Blocking scans nearby players' power: Scanned enemies take more damage. Scanned friendlies heal for more.*

  - \[+] +20% Scan Amplification *(per stack)*
  - \[+] 7s Duration *(+1s per extra stack)*
  - \[--] +0.5 Block Cooldown
  - \[--] 9s *Gear* Cooldown *(-1s per extra stack)*

1. Increase all source of heal gains/losses by a flat amp. percentage, no feedback loop!
2. Also show the latest stats of the scanned player:
   - HP/Max HP (green)
   - Health delta over the last second (after amps) (white)
   - DMG: xA\[B] where A is total bullet they shoot per volley, B is base damage for each bullet
   - ATK/s: shots(or volley of bullets) fired per second
   - RLD: reload time in seconds
   - Blocks: block counts on each trigger
   - BlkCD: block cooldown in seconds

#### \[Size Normalizer]
Unique Size Modification

- *Set your final player size much closer to default where it started.*

  - \[+] +50% Health
  - \[+] +25% Movement Speed
  - \[+/-] Near normal size
  - \[+/-] Cannot GO BEEG
  - \[+/-] Cannot go smol

***You can only have one Unique Mods of each type in posession!***
- The extra Size Mod cards you obtain are converted to \[Medical Parts] or potentially lost!
- The following cards from other mods are also consider as Size Mod:
  - \[Size Difference]
  - \[Size Matter]

### Common Cards
#### \[Medical Parts]
Health Passive - *Material*

- *Some hearty doodads to help you survive longer.*

  - \[+] +50% Health
  - \[+] +2 HP Regeneration/second

*\* Material cards can be obtained normally or as result of having conflicting cards together.*

#### \[Gun Parts]
Gun Passive - *Material*

- *Attachments and enhancements to up your gun games.*

  - \[+] +25% Damage
  - \[+] +25% Attack Speed
  - \[+] +3 Max Ammo

#### \[Magick Fragments]
Spell Passive - *Material*

- *This mysterious glyph hasten your spellcasting, but at what cost?*

  - \[+] -30% Block Cooldown
  - \[--] -25% Health
  - \[+] Faster *Spell* Cooldown*

\* *The bonus varies from each spells and Magicks*

## Note from the modder
There will be more cards planned to expand in each category, this first beta release serves as a pavement and test run on my first ever modding experience, ever.

## Patch Notes
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

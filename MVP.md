
TACTICAL HEX GAME — MVP SPEC

# GOAL

Create a small turn-based tactics game inspired by Advance Wars:
- Hex grid movement
- Two unit types (Assault, Sniper)
    - Assault Direct Attack (adjacent grid) only
    - Sniper Indirect Attach (non adjacent grid in range only)
- Turn-based Direct/Indirect combat (with counterattacks where applicable)
- Simple AI
- Clear, minimal UI
- Fully playable from start to finish

## CORE GAME LOOP

1. Player selects/clicks a unit (movement range highlighted)
2. Player selects/clicks a tile to move to (clicking outside range deselects unit)
- Clicking same unit again selects no movement
3. Action menu displayed - Context sensitive - Player chooses action 
- Attack 
    - If available, 
    - Indirect attack only available if unit has not moved and target(s) in range
- End (unit becomes inactive)
    - E.g. unit moved but no direct targets in range
    - E.g. unit remains stationary but no indirect targets in range
- Cancel (revert move if menu displayed after a move)
    - Unit remains active
4. Process any attack/death as necessary
5. Repeat for all player units
6. AI takes turn (if still has units)
7. Repeat until win/loss condition met

## GRID SYSTEM

- Hex grid
- Size: 6x6 or 8x8 or 10x10
- Tile types:
- Plain
  
### Requirements:

- Tile selection (click/tap)
- Highlight movement range
- Highlight attack range (only during preview)

## UNITS

Per side:
- 1 × Assault
- 1 × Sniper

## UNIT TYPES

### ASSAULT

- Range: 1 (adjacent direct)
- Movement: 3 tiles
- BaseDamage: 40
- MaxHP: 100
- Can move and attack

### SNIPER

- Range: 2–3 (indirect)
- Movement: 2 tiles
- BaseDamage: 60
- MaxHP: 100
- Cannot attack adjacent
- Cannot move and attack

## HEALTH SYSTEM

- Internal HP: 0–100
- All units start at 100 HP

Display HP:
displayHP = ceil(currentHP / 10)

Examples:
- 100 → 10
- 76 → 8
- 21 → 3

## DAMAGE SYSTEM

damage = round(baseDamage × (currentHP / 100))

Cover:
- If target is in cover:

damage = damage - 1
minimum damage = 1

## COMBAT FLOW

1. Attacker deals damage
2. If defender survives:
- Check counterattack applicable (direct attacks only)
3. If defender can counter:
- Defender deals damage back

## COUNTERATTACK RULES

Defender counterattacks if:
- Still alive after attack
- Attacker is within defender’s range
- Defender is allowed to attack

No counterattack
- Defender dies
- Attacker out of range
- Sniper attacked from adjacent tile

## UNIT SELECTION

- Click unit to select
- Only selectable if:
- Player-owned
- Has not acted
- Clicking empty space:
- Deselects unit (if no action committed)

## MOVEMENT SYSTEM

- Movement uses range (BFS-style search)
- Cannot move through units
- Cannot end on occupied tiles

## MOVEMENT FLOW

1. Select unit → show movement tiles only
2. Click valid tile:
- Unit moves (preview only until confirmed on Action menu)
- Action menu appears
- Attackable enemies shown from preview position

## ACTION MENU

Options (context-sensitive):
- Move (if not moved yet)
- Attack (if valid targets exist)
- End (always available)
- Cancel (only during movement preview)

Behaviour:

- Move → preview → menu
- Attack → select target → unit ends
- End → unit ends
- Cancel → revert to original position

## TARGETING SYSTEM

- Highlight all valid enemy targets
- Player selects one target
- Works for all unit types

## TURN SYSTEM

- Player controls all units
- Each unit acts once per turn
- After all player units act:
→ AI turn begins

## AI (MVP)

For each enemy unit:
1. Check if any targets in attack range at turn start
- Attack if in range
- Move toward nearest player unit if none in range

## VISUALS (MVP)

Team:
- Player = Blue
- Enemy = Red

Unit Types:
- Assault = Cube
- Sniper = Capsule

## VISUAL FEEDBACK

- Selected unit highlight
- Movement tiles highlighted
- Attackable enemies highlighted (only after movement preview)
- HP displayed above units

## INPUT RULES

- Click unit → select
- Click tile → preview move
- Click enemy → attack
- Click elsewhere → deselect (if not committed)

## GAME STATES

- Idle
- UnitSelected
- PreviewMove
- ActionMenu
- EnemyTurn
- GameOver

## WIN / LOSS

Win:
- All enemy units defeated

Loss:
- All player units defeated

## END SCREEN

- Display Victory or Defeat
- Restart option

## OUT OF SCOPE (DO NOT BUILD)

However these are loosely in order to priority to build next

- Enemy threat preview (click enemy to see their attack range)
- Base/HQ grid (and assoicated win/loss processing)
- Multiplayer (AI only)
- Terrain Types (movement/combat changes)
- Vehicles
- Fog of war 
- Terrain Textures
- Animated Unit models
- Upgrades or progression
- Inventory
- Large maps
- Story
- Front End / UI (other than action menu and win/loss/restart screen)

## MVP COMPLETION CHECKLIST

[ ] Unit selection works
[ ] Movement range works
[ ] Movement preview + cancel works
[ ] Action menu works
[ ] Combat works
[ ] Counterattack works
[ ] Multiple targets selectable
[ ] Turns alternate correctly
[ ] AI takes actions
[ ] Units die correctly
[ ] HP displays correctly
[ ] Game ends (win/loss screen)
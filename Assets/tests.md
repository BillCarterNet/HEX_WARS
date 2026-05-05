# HEX WARS – Manual Test Cases

This document defines the core interaction tests for unit selection, movement, cancel, and end turn.

Run through these after any gameplay/system changes.

---

## 🧪 TEST GROUP 1 — Selection

### 1. Select Unit
**Steps:**
- Click a tile containing a unit

**Expected:**
- Tile turns yellow
- Movement range (green tiles) appears
- No action menu appears

---

### 2. Click Same Unit Again (Open Action Menu)
**Steps:**
- Click a unit
- Click the same unit again

**Expected:**
- Movement preview ends
- Action menu is shown (log output for now)
- Unit does NOT move

---

---

## 🧪 TEST GROUP 2 — Movement

### 1. Move to Valid Tile
**Steps:**
- Click a unit
- Click a green tile

**Expected:**
- Unit moves to the selected tile
- Green tiles disappear
- Selected tile remains yellow
- Action menu is shown

---

### 2. Move Then End Turn
**Steps:**
- Click a unit
- Move to a green tile
- Press `E`

**Expected:**
- "END TURN" is logged
- Unit is deselected
- All highlights are cleared

---

---

## 🧪 TEST GROUP 3 — Cancel

### 1. Cancel Ignored During Movement Preview
**Steps:**
- Click a unit (movement preview only)
- Press `C`

**Expected:**
- Nothing happens
- Unit remains selected
- Yellow highlight remains
- Green movement tiles remain
- No action menu is triggered

---

### 2. Cancel From Action Menu (No Movement)
**Steps:**
- Click a unit
- Click the same unit again (open action menu)
- Press `C`

**Expected:**
- Unit is deselected
- Yellow highlight is cleared
- No green tiles remain
- Action menu closes

---

### 3. Cancel After Move
**Steps:**
- Click a unit
- Move to a green tile
- Press `C`

**Expected:**
- Unit returns to original tile
- Movement range (green tiles) reappears
- Unit remains selected
- No action menu is shown

---

---

## 🧪 TEST GROUP 4 — Switching Units

### 1. Switch Units
**Steps:**
- Click Unit A
- Click Unit B

**Expected:**
- Unit A is deselected
- Unit B is selected
- Movement range for Unit B is shown

---

---

## 🧪 TEST GROUP 5 — Edge Cases

### 1. Click Empty Tile (No Selection)
**Steps:**
- Click an empty tile when no unit is selected

**Expected:**
- Nothing happens
- No highlights appear

---

### 2. Attempt Move to Occupied Tile
**Steps:**
- Select a unit
- Attempt to move to a tile with another unit

**Expected:**
- Unit does NOT move
- No errors occur

---

---

## 🧪 TEST GROUP 6 — State Consistency

### 1. No Multiple Yellow Tiles
**Steps:**
- Select a unit
- Deselect (via Cancel or End)
- Select another unit

**Expected:**
- Only ONE tile is yellow at any time

---

### 2. No Stale Highlights
**Steps:**
- Perform several actions (move, cancel, end turn)

**Expected:**
- No leftover green or yellow tiles remain incorrectly highlighted

---

---

## 🧪 NOTES

- Action Menu is currently represented via Debug.Log output
- `C` and `E` inputs should ONLY function when the action menu is active
- Movement preview is a planning state and should not trigger actions
- These tests should be expanded when:
  - Attack logic is implemented
  - Turn system is introduced
  - UI menu is added
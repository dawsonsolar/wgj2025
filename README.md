# Ice 'Em

A turn-based 2D physics game where teams of penguins fling themselves and use items to knock opponents off the ice or reduce their health to zero. Made for Winter Game Jam 2025.

---

## How to Play

**Selecting and flinging a penguin**
- Click a penguin on your team to select it. An aim line appears and tracks your cursor.
- Move your cursor to set the direction and power (further away = faster).
- Left-click to confirm and launch.
- Right-click cancels the selection.

**Using an item**
- Slide a penguin over an item pickup on the ice to collect it.
- Select that penguin and press **E** to switch between Fling mode and Item mode. The aim line turns yellow in Item mode.
- Left-click to confirm and throw the item in the aimed direction.
- The item is consumed after use. Each penguin holds one item at a time.

**Winning**
- Eliminate all enemy penguins by knocking them into the water or dealing enough damage to reduce their health to zero.
- The game ends immediately when one team has no penguins remaining.

---

## Features

### Turn System
Turns alternate between the player team and the enemy team. Every penguin on the active team must take an action before the turn passes. If a penguin dies before it has moved, the turn advances automatically. The enemy turn can be force-skipped during testing with **Space**.

### Enemy AI
Enemy penguins automatically find and target the nearest player penguin each turn. The AI checks the full path for walls and kill zones before committing to a direction, then sweeps outward through angles to find the clearest launch route. Levels can contain **AIPathing** waypoints (tagged `AIGap`) to help the AI navigate around obstacles and corridors it cannot reach directly.

### Health System
Each penguin has a health bar that appears above them when they take damage and fades after a short time. Penguins are eliminated when their health reaches zero. Penguins on the active team are immune to collision damage from their own teammates during their turn.

### Items
Items are placed in the level as pickups. A player penguin that slides over a pickup collects it and carries it for the rest of the game. Each penguin holds one item at a time.

**Rock**
- Deals contact damage to any penguin it hits directly.
- Slides to a stop and despawns after a few seconds.
- Cracks Thin Ice zones it passes through.

**Bomb**
- Deals light contact damage when it bounces off a penguin.
- Explodes after its fuse timer runs out or as soon as it comes to a stop.
- The explosion damages all penguins caught in the blast radius.
- Cracks or breaks Thin Ice zones within the explosion radius.

### Thin Ice
Certain areas of the ice can be marked as Thin Ice. These zones are invisible or subtly marked at the start of a round and react to destructive impacts.

- **First hit** — the ice cracks and the cracks are revealed.
- **Second hit** — the ice breaks. The ice is replaced with water and a kill zone activates over that area.
- Any penguin that slides into a broken Thin Ice zone is eliminated instantly, similarly to water.
- Bomb explosions crack or break Thin Ice within their radius. Rocks crack Thin Ice on contact.

### Levels
The game includes five playable levels, each with a different ice arena layout, platform arrangement, and placement of item pickups and Thin Ice zones. Difficulty increases across levels with more complex layouts and additional hazards.

---

## Scripts

| Script | Purpose |
|---|---|
| `PlayerFlinger2D` | Core penguin controller — handles selection, aiming, launching, item holding, and turn handoff |
| `PlayerClickSelector2D` | Reads mouse input to select penguins and confirm launches |
| `EnemyAI` | Controls enemy penguin behaviour each turn — pathfinding, gap navigation, angle sweep |
| `TurnManager` | Manages turn order, team switching, win and loss conditions |
| `Stats` | Tracks health and damage values, handles death and health bar display |
| `ThrowableItem` | Projectile logic for items — contact damage, fuse timer, explosion, thin ice interaction |
| `ItemPickup` | Collectible placed in the scene — gives the item to the first player penguin that touches it |
| `ThinIce` | Breakable ice zone — manages crack and break states, activates kill zone on break |
| `ExplosionEffect` | Short-lived trigger spawned by bomb explosions to notify nearby Thin Ice zones |
| `KillZone2D` | Instantly eliminates any penguin that enters the trigger area |
| `PenguinSpriteController` | Updates the penguin sprite based on movement direction |
| `HealthBarUI` | World-space health bar that follows a penguin and fades after taking damage |
| `GameUIController` | Displays turn announcements and win/loss screen |
| `PauseMenu` | Handles in-game pause, resume, and returning to the title screen |

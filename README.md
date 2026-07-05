# Torpedo

https://github.com/user-attachments/assets/61b5fd6c-d22f-40bf-add1-6f6740428b9e

Adds four torpedo variants to Nuclear Option (cloned from vanilla anti-ship and cruise missiles). Fire them at range like any other weapon — they drop to the water, and swim toward the target before detonating on impact or proximity to a ship.

## What it adds

### **SCT-350 'Mako'** — Fast, super-cavitating interceptor. For both anti-ship and counter-torpedo.
<img width="1669" height="1080" alt="Torp1" src="https://github.com/user-attachments/assets/a6380ed6-f1d0-4356-9e8f-3eaea4fff394" />

### **Type-88 'Lemon'** — Light, compact, cheap, and agile. Deceptively lethal, Ideal choice for saturation or ambush.
<img width="1669" height="1080" alt="Torp2" src="https://github.com/user-attachments/assets/56eb866c-9ec3-4474-8929-a6c4f275a5f2" />

### **HT-200 'Hammerhead'** — Heavy kinetic penetrator. Low velocity, but carries 1350kg payload to split a carrier in half.
<img width="1671" height="1080" alt="Torp3" src="https://github.com/user-attachments/assets/7a101654-e4e8-4f64-a5bc-d5ea6f5e0883" />

### **NT-2 'Megalodon' (20kt)** — Nuclear-tipped strategic asset. Erasing enemy ship and its escorts in a single delivery.
<img width="1669" height="1080" alt="Torp4" src="https://github.com/user-attachments/assets/ec922472-cc92-4bc9-b3bb-382ad8e05a03" />

Every vanilla mount configuration for each source missile (single, double, internal bay, external, etc.) is automatically available for its torpedo counterpart — no manual per-aircraft setup needed.

## How it behaves

Fire it like a normal bomb. It transitions into torpedo mode once it touches the water: dives to its designated depth, holds a steady course, and detonates only when it actually hits (or near) a ship. Run one into land and it'll detonate.

Speed is deliberately far below the source missile's aerial top speed (see the table below) — burn time is extended to match, so it still has the range to reach distant targets despite cruising much slower underwater.

| Torpedo | Source | Top speed | Explosive yield |
|---|---|---|---|
| SCT-350 'Mako'   | AShM-300 | 359 km/h | 600kg |
| Type-88 'Lemon' | AGM-99 | 257 km/h | 650kg |
| HT-200 'Hammerhead' | ALCM-450 | 204 km/h | 1350kg |
| NT-2 'Megalodon' (20kt) | ALND-4 | 194 km/h | 20kt |

## Requirements

- BepInEx 5.4.23.5
- No dependency on other mods — the mod clones and modifies vanilla assets directly at runtime.
- [Surface Loadout](https://github.com/SonPamungkas/surface-loadout) if you want naval or ground unit to fire the torpedo that you want.

### Argus Lemon
<img width="1427" height="862" alt="Argus Lemon" src="https://github.com/user-attachments/assets/c536c087-edc2-455b-b8c8-91e0a5ec9581" />

## Installation

Drop `Torpedo.dll` into your `BepInEx/plugins` folder.

## Known limitations
- Torpedoes are generally undetectable once it dives, but can still be stopped by explosion on the surface or an obstacle in its path.
- Torpedoes target whatever the underlying missile's seeker would normally target (they don't yet filter to naval-only targets).

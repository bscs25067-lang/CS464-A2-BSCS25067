# CS464 Assignment 02 · ⚠️[Your full name] · ⚠️[Roll no]

- **Video:** [Google Drive link](https://drive.google.com/file/d/16xG5hBgWxGf_hubeqNxkgeDx_N7aAKmV/view?usp=sharing)
- **Input:** Legacy Input (`Input.GetAxis`, `GetKeyDown(Space)`), Input Handling set to Both
- **Ground check:** `Physics.CheckSphere` at the player's feet, with Ground layer as mask.
- **Camera:** smoothing blend = 1 − e^(−k·dt) with k = 10, in `LateUpdate`, Look-ahead of the player's velocity is  0.3 s
- **Juice:** Squash on landing via a coroutine with quadratic ease-out (1 − (1 − p)²). It starts only on landing after atleast 0.1 s in the air.

## Tuning

| Setting | Value | Why this number |
|---|---|---|
| Speed | 4 m/s | Makes the player feel grounded. |
| Jump height | 1.5 m | Enoguh to clear the obstacles but not make it super floaty and easy. |
| Time to apex | 0.55 s | v0 / g = 5.43 / 9.81. It follows from the jump height. |
| Fall multiplier | 2× | This gives a snappy landing and the same apex. |
| Coyote time | 0.2 s | At 4 m/s that is 0.8 m past the edge, enough to forgive the player for late input |
| Jump buffer | 0.2 s | A press up to 0.2 s before landing still jumps, which is about 0.8m of fall at the end. |
| Camera smoothing | 10 (sharpness) | Steady-state lag is speed ÷ k = 5 / 10 = 0.5 m, so the camera follows closely without feeling too snappy. |
| Look-ahead | 0.3 s | 0.3 × 4 m/s = 1.2 m of look-ahead, so the view shows enough of what is ahead. |

## Four answers

1. **The jump, in numbers.** With h = 1.5 m and g = 9.81, the launch speed is v0 = √(2·9.81·1.5) = **5.43 m/s**. The impulse is mass × v0, so `jumpForce` is 5.43 on a 1 kg body. Time to apex is 5.43 / 9.81 = **0.55 s**. With fall multiplier 2, the fall time is √(2·1.5 / (9.81·2)) = **0.39 s**. In the air for 0.94 s at 5 m/s, one jump carries me about **4.7 m** on flat ground. Widest gap between obstacles is roughly 2-3m.

2. **Update and FixedUpdate.** `Update` runs once per frame. It reads the keys, calculates camera-relative `_moveInput`, set the coyote and buffer timestamps, queues jump, and detects landings to start the squash. `FixedUpdate` runs every 0.02 s. checks if player is ground via `CheckSphere`, sets `rb.linearVelocity` from `_moveInput` (maintaining y velocity), applies the queued jump once, and adds extra fall gravity. If I swapped them, `GetKeyDown` would be true for only one frame, and at 144 fps about two frames in three have no `FixedUpdate`, so most jump presses would be lost. Setting velocity in `Update` would tie physics to the frame rate and make the movement jittery.

3. **Vector3.Dot.** My script doesn't use it, but I can use `Vector3.Dot(_rb.linearVelocity, Vector3.up)` to tell whether I'm rising or falling. A positive result means moving up, negative means falling, and 0 means at the apex or on the ground.

4. **The juice.** A squash on landing + particle effect on jump: `Body` is scaled to y = 0.7 (x and z = 1/√0.7 ≈ 1.2 to keep the volume), then eases back to 1 with a quadratic ease-out, 1 − (1 − p)². The duration is **0.25 s, which is 15 frames at 60 fps**. Ease-out makes the squash hit instantly on impact and then relax. 0.25 s is long enough to see (more than about 6 frames) but short enough to finish before another jump. It runs in a coroutine, starts only after spending 0.1 s in the air so small hops don't trigger the animation, and ends by setting the scale to exactly (1, 1, 1). The particle effect is played the moment the player jumps.

## Levels

| Level | What the controller must do there | What I changed for this jump |
|---|---|---|
| Level01 | A Mario like level where the player enters a big open area like a garden and needs to jump over pillars and rocks to reach the tower. | Nothing. |
| Level02 | Among us inspired level where the player has to find the right room to reach the goal, going different routes rewards point. | Nothing. |
| Level03 | A small passage leads to a big cave like opening with a broken bridge and lava beaneath forcing the player to jump and avoid dying to reach the end. | Nothing. |
| Level04 | Inspired by Mirrors Edge, Make way through multiple buildings and paths to reach the tall building with the helipad. | Nothing. |
| Level05 | A dark souls like map where the player has to traverse multiple different kinds of obstacles and paths just to reach the goal right next to the spawn blocked by a wall. | Nothing. |
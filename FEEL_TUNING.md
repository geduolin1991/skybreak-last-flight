# 1.12 campaign presentation and combat tuning

Player experience targets: understand why each chapter matters, see objective consequences in the world, read boss openings, and feel aircraft destruction without pausing control.

| Weak moment | Change | Budget / constraint | Evidence to collect |
| --- | --- | --- | --- |
| Abrupt chapter and result screens | Authored prologue, bridges, three outcome-dependent endings and individual pilot codas | Manual next, optional auto, skip; mission clock frozen | Native sequence traversal and touch layout checks |
| Aircraft disappear instantly | Existing visual falls, rolls and trails small embers | 9 slots desktop / 5 touch; no new model clone or collision | Pool return and destruction stress checks |
| Damage arrives during boss final words | Remove hostile beams/rounds, retreat remaining air enemies and maintain exit protection | Only after the boss is destroyed | Injected late-damage regression |
| Boss becomes a flat health bar after modules break | Short core exposure after a completed volley | 1.4–1.8 seconds; damage ×1.65; existing arrival times preserved | Runtime window check and three natural campaigns |
| Chapters share an unchanging backdrop | District trains and buses, weather recovery, powered orbital joints | Static geometry batched; moving geometry excluded; fixed actor counts | Restored-city and orbital screenshots, frame samples |
| Music cuts at state changes | Two pairs of synchronized sources, equal-power 1.3-second crossfade | Retire old pair on completion; narration ducks both | Ship selection, audio reset and boss transition checks |

Avoid extra full-screen flashes, global hit-stop on ordinary kills and unbounded physical debris. Settings retain camera-shake control and three browser quality choices. Phone measurements use emulation unless explicitly labeled as physical device data.

## 1.13 frame identity and civilian counterplay

| Weak moment | Change | Budget / constraint | Validation |
| --- | --- | --- | --- |
| Three overdrives share a silhouette and attacks | Three Blender frames, scoped articulated meshes, blade / siege / prism arsenals | 16.7k–26.4k triangles; 30–45 renderers; same hit core | Actual game-camera captures, damage and alternate-mode checks |
| Transformation hides the model in light | Mechanical unfolding, synchronized recoil, lower bloom and short local ring | 12 reusable stroke lines; no repeated full-screen flash | Deployment/reversion, reset, pause and mobile playback |
| Saved civilians rarely alter the boss encounter | Conditional 5/6/7-second shield bypass during phase two | Once per boss; prerequisites and expiry enforced | Earned/missing objective cases, damage and cleanup checks |
| New weapon identity is hard to hear | Six brief mechanical cues, seven tactical events in three languages | Existing voice ducking; SFX peak below -4 dBFS | New-take ASR/signal review plus live audio continuity |

Retain the 75/95/110-second boss arrivals, readable telegraphs, three outcomes, mobile portrait support and anchored scenery fixes.


## 1.14 destruction and enemy equipment

| Weak moment | Change | Budget / constraint | Validation |
| --- | --- | --- | --- |
| Every kill reads as the same flash | Immediate air breakup or smoking, rolling descent to the background; water spray/ripples, ground fire/scorch, vacuum dissipation | Reuse 9 wrecks / 5 on touch and 10 surface impacts; kill reward and collision end immediately | All three chapter surfaces, scrolling alignment, single reward, pause and cleanup |
| Pickups look like abstract shapes | Four Blender equipment models with distinct silhouette, symbol and colour | 940–2,188 imported triangles; same reward and collection range | Real mesh import and all four rewards, browser legibility |
| Tough enemies only increase health | Two heavy types resist damage, open physical cooling shutters after firing and permanently lose armor at low hull | One threat readout; damage ×0.70 protected, ×1.70 cooling, ×1.12 broken | Damage and hatch direction checks, carrier launch, natural campaigns |
| Enemies lack construction detail | Rebuild nine airframes with canopy, engine, weapon, vent and wing assemblies | 2,780–12,972 imported triangles / 5–11 renderers | Actual game camera, three browser quality settings and mass kills |

No ordinary-kill full-screen flash or rigidbody debris was added. Impact locations use the same authored shoreline footprints as the ocean renderer; this is cosmetic surface classification, not building destruction physics. New audio cues are original procedural synthesis. Physical phones still require separate performance testing.

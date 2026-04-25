# Story–Dialogue integration status

**Date:** 2026-04-23  
**Scope:** Pixel Crushers Dialogue System for Unity v2.2.69 ↔ narrative/mission systems (`Assets/_Project/` only; no edits under `Assets/Plugins/Pixel Crushers/`).

## Completed

| Deliverable | Location |
|-------------|----------|
| Dialogue runtime bridge | `Assets/_Project/Scripts/Story/DialogueSystemBridge.cs` |
| Quest `SendMessage` hook | `Assets/_Project/Scripts/Story/DialogueQuestBroadcastHook.cs` |
| Mission ↔ quest sync (gameplay authoritative) | `Assets/_Project/Scripts/Story/MissionDialogueSync.cs` |
| Radio comms from `MatchEvents` (queued barks) | `Assets/_Project/Scripts/Story/RadioCommsManager.cs` |
| Readable → Dialogue UI | `Assets/_Project/Scripts/Story/ReadableDialogueAdapter.cs` + `ReadableObject.cs` |
| Mission completion event for sync | `Assets/_Project/Scripts/Story/MissionManager.cs` |
| Prototype DB generator (Editor) | `Assets/_Project/Scripts/Editor/Story/PrototypeDialogueDatabaseBuilder.cs` → `Assets/_Project/Data/Dialogues/PrototypeDialogueDB.asset` |

### Behaviour notes

- **DialogueSystemBridge** runs at `DefaultExecutionOrder(-1000)`, assigns `initialDatabase` when possible, optionally instantiates a serialized Dialogue Manager prefab, and wires `DialogueQuestBroadcastHook` to `MissionDialogueSync.NotifyQuestStateChanged`.
- **MissionDialogueSync** maps quest titles to missions via inspector **links**; if no link exists, **`mission.ID`** is used as the quest title when pushing gameplay → Dialogue System. Incoming quest changes: **Success** before `MissionManager` agrees is **reverted to Active** with a warning; **Active** adds the mission to `MissionManager`.
- **RadioCommsManager** listens to `MatchEvents.OnMatchStart`, `OnExtractionStart`, `OnPlayerSpotted`, and `OnAIDeath`; it plays **`Radio_Barks`** for match events and schedules **`NPC_Ambient_Chatter`** lines during low-threat windows (patrol/chill), with randomized intervals and a combat cooldown to avoid overlap with firefights.
- **ReadableDialogueAdapter** sets Lua `readable_title` / `readable_body` and starts **`Readable_Document_Template`**.
- **PrototypeDialogueDB** now includes **`NPC_Ambient_Chatter`** with multiple non-story ambient lines intended for background patrol chatter.

## Verification

- **Unity batch compile passed** after closing the editor lock (`Unity.exe -batchmode -nographics -quit -projectPath ...`). Log scan found no `error`, `Exception`, `failed`, or `Aborting batchmode` entries.
- **PrototypeDialogueDB.asset** is created on first Editor load if missing (`InitializeOnLoadMethod`), or via menu **Extraction Shooter → Story → Regenerate Prototype Dialogue Database**.

## Scene / setup checklist (manual)

1. Add **Dialogue Manager** to the scene (or assign **dialogueManagerPrefab** on `DialogueSystemBridge`).
2. Add **`DialogueSystemBridge`**: assign **PrototypeDialogueDB** (or rely on generator + assign after first import).
3. Add **`MissionDialogueSync`**: configure **QuestMissionLink** rows (or ensure quest titles match `MissionObjective.ID`).
4. Add **`RadioCommsManager`**: assign `DialogueSystemBridge` (or leave empty for auto-find).
5. Add **`ReadableDialogueAdapter`**: assign bridge; ensure a **Standard Dialogue UI** (or your UI) is present per Pixel Crushers docs.

## Blockers / follow-ups

- **Unity Safe Mode / Editor open:** if the Editor reports compile errors on open, inspect Console errors top-down; batchmode passed, so check for Editor-only/import-time issues.
- **Bark UX:** consider sequencing by sequencer / subtitle callbacks instead of fixed `postBarkPauseSeconds` if overlap or cut-offs occur.
- **MissionDialogueSync subscription:** if `MissionManager` awakens after `MissionDialogueSync`, `OnEnable` may miss subscription; consider `Start()` re-bind or execution order if missions never sync from gameplay.

## Next session focus

1. Open in Unity, inspect any Safe Mode Console errors, then play-test conversations and quest sync.
2. Tune **Radio_Barks** dialogue entries (conditions on Lua `reason` or separate bark conversations per event if needed).
3. Optional: `MatchEvents` hook for document read / lore pickup if design wants radio reactions.

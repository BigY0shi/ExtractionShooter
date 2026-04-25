#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace ExtractionShooter.Story.Editor
{
    /// <summary>
    /// Builds the prototype Dialogue System database under <c>Assets/_Project/Data/Dialogues/</c>.
    /// Runs once on domain reload if the asset is missing; can also be regenerated from the menu.
    /// </summary>
    public static class PrototypeDialogueDatabaseBuilder
    {
        public const string AssetPath = "Assets/_Project/Data/Dialogues/PrototypeDialogueDB.asset";

        [InitializeOnLoadMethod]
        private static void EnsureDatabaseExists()
        {
            if (File.Exists(AssetPath)) return;
            BuildAndSave();
        }

        [MenuItem("Extraction Shooter/Story/Regenerate Prototype Dialogue Database")]
        public static void BuildAndSaveMenu() => BuildAndSave();

        public static DialogueDatabase BuildAndSave()
        {
            var folder = Path.GetDirectoryName(AssetPath);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var db = ScriptableObject.CreateInstance<DialogueDatabase>();
            db.name = "PrototypeDialogueDB";
            db.version = "2.2.69-prototype";
            db.author = "Extraction Shooter / Story";
            db.description = "Prototype Vorograd-themed dialogue, radio barks, and quest wiring for MissionObjective sync.";
            db.ResetEmphasisSettings();

            const int idPlayer = 1;
            const int idHQ = 2;
            const int idNpc = 3;

            db.actors.Add(MakeActor(idPlayer, "Player", "Contractor on the ground in Vorograd.", isPlayer: true));
            db.actors.Add(MakeActor(idHQ, "CommandHQ", "Forward tactical net — radio handler.", isPlayer: false));
            db.actors.Add(MakeActor(idNpc, "GenericNPC", "Local contact or survivor.", isPlayer: false));

            const int questId = 10;
            db.items.Add(MakeQuestItem(questId, "Vorograd_Raid_Objective",
                "Secure intel and extract before the cordon tightens.",
                "Objective complete. Good work — exfil is live.",
                ""));

            int convoId = 100;
            db.conversations.Add(MakeBriefingConversation(++convoId, idPlayer, idHQ));
            db.conversations.Add(MakeNpcConversation(++convoId, idPlayer, idNpc));
            db.conversations.Add(MakeGenericNpcConversation(++convoId, idPlayer, idNpc));
            db.conversations.Add(MakeRadioBarksConversation(++convoId, idHQ, idPlayer));
            db.conversations.Add(MakeNpcAmbientConversation(++convoId, idNpc, idPlayer));
            db.conversations.Add(MakeReadableTemplateConversation(++convoId, idPlayer, idNpc));

            if (File.Exists(AssetPath))
                AssetDatabase.DeleteAsset(AssetPath);

            AssetDatabase.CreateAsset(db, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[PrototypeDialogueDatabaseBuilder] Wrote {AssetPath}");
            return db;
        }

        private static Actor MakeActor(int id, string name, string description, bool isPlayer)
        {
            var a = new Actor { id = id, fields = new List<Field>() };
            Field.SetValue(a.fields, "Name", name, FieldType.Text);
            Field.SetValue(a.fields, "Description", description, FieldType.Text);
            Field.SetValue(a.fields, "IsPlayer", isPlayer ? "True" : "False", FieldType.Boolean);
            return a;
        }

        private static Item MakeQuestItem(int id, string title, string description, string success, string failure)
        {
            var item = new Item { id = id, fields = new List<Field>() };
            Field.SetValue(item.fields, "Name", title, FieldType.Text);
            Field.SetValue(item.fields, "Description", description, FieldType.Text);
            Field.SetValue(item.fields, "Success Description", success, FieldType.Text);
            Field.SetValue(item.fields, "Failure Description", failure, FieldType.Text);
            Field.SetValue(item.fields, "State", QuestLog.UnassignedStateString, FieldType.Text);
            Field.SetValue(item.fields, "Is Item", "False", FieldType.Boolean);
            Field.SetValue(item.fields, "Track", "True", FieldType.Boolean);
            Field.SetValue(item.fields, "Trackable", "True", FieldType.Boolean);
            return item;
        }

        private static Conversation MakeBriefingConversation(int id, int actorId, int conversantId)
        {
            var c = BaseConversation(id, "HQ_Mission_Briefing", actorId, conversantId,
                "Radio briefing: raid parameters for Vorograd industrial sector.");
            AddLinearLines(c, actorId, conversantId,
                "Command, this is actual. Inbound for grid seven-niner, over.",
                "Copy, actual. Picture is fluid — militia checkpoints east, drone sweep every six mikes. Primary: recover the casefile from the old rail office. Secondary: stay quiet until exfil lights green. Clock is eating daylight — do not miss the window.");
            return c;
        }

        private static Conversation MakeNpcConversation(int id, int actorId, int conversantId)
        {
            var c = BaseConversation(id, "NPC_Contact_Welcome", actorId, conversantId,
                "Face-to-face with a nervous local.");
            AddLinearLines(c, conversantId, actorId,
                "You are not city sanitation. Who sent you?",
                "Same people who pay for heat in winter. I need passage to the depot catwalks.",
                "Then you want Viktor. He owes me — go around the blue containers, knock twice. And do not shoot the dogs; they are louder than alarms.");
            return c;
        }

        private static Conversation MakeGenericNpcConversation(int id, int actorId, int conversantId)
        {
            var c = BaseConversation(id, "NPC_Generic_Supplies", actorId, conversantId,
                "Generic survivor — barter tone.");
            AddLinearLines(c, conversantId, actorId,
                "Stranger, hands where I can see them.",
                "Easy. I am leaving with my kit and no drama. You selling filters or fairy tales?",
                "Filters are real. Fairy tales are cheaper. Pick one.");
            return c;
        }

        private static Conversation MakeRadioBarksConversation(int id, int speakerId, int listenerId)
        {
            var c = BaseConversation(id, "Radio_Barks", speakerId, listenerId,
                "Short radio lines for barks (speaker = CommandHQ).");
            var root = Entry(c.id, 0, speakerId, listenerId, isRoot: true, "START", string.Empty, string.Empty);
            c.dialogueEntries.Add(root);
            c.dialogueEntries.Add(Entry(c.id, 1, speakerId, listenerId, false, "RB1", string.Empty,
                "All callsigns — thermal spike grid four. Stay off main roads thirty seconds."));
            c.dialogueEntries.Add(Entry(c.id, 2, speakerId, listenerId, false, "RB2", string.Empty,
                "Exfil beacon is staged. Ghost your movement; eyes are up."));
            c.dialogueEntries.Add(Entry(c.id, 3, speakerId, listenerId, false, "RB3", string.Empty,
                "Contact left — assume armed escort. Weapons tight, identification loose."));
            root.outgoingLinks.Add(Link(c.id, 0, 1));
            root.outgoingLinks.Add(Link(c.id, 0, 2));
            root.outgoingLinks.Add(Link(c.id, 0, 3));
            return c;
        }

        private static Conversation MakeNpcAmbientConversation(int id, int speakerId, int listenerId)
        {
            var c = BaseConversation(id, "NPC_Ambient_Chatter", speakerId, listenerId,
                "Non-story ambient NPC chatter for patrol/chill moments.");
            var root = Entry(c.id, 0, speakerId, listenerId, isRoot: true, "START", string.Empty, string.Empty);
            c.dialogueEntries.Add(root);
            c.dialogueEntries.Add(Entry(c.id, 1, speakerId, listenerId, false, "AC1", string.Empty,
                "Quiet street. Too quiet. Keep your ears open."));
            c.dialogueEntries.Add(Entry(c.id, 2, speakerId, listenerId, false, "AC2", string.Empty,
                "If the generators keep humming, we get one more warm night."));
            c.dialogueEntries.Add(Entry(c.id, 3, speakerId, listenerId, false, "AC3", string.Empty,
                "Patrol says docks are clear. Means trouble moved somewhere else."));
            c.dialogueEntries.Add(Entry(c.id, 4, speakerId, listenerId, false, "AC4", string.Empty,
                "No shooting for ten minutes. That has to be a city record."));
            c.dialogueEntries.Add(Entry(c.id, 5, speakerId, listenerId, false, "AC5", string.Empty,
                "Watch the rooftops. Snipers get bored before they get hungry."));
            c.dialogueEntries.Add(Entry(c.id, 6, speakerId, listenerId, false, "AC6", string.Empty,
                "Someone was brewing coffee in the rail yard. Smelled almost normal."));
            c.dialogueEntries.Add(Entry(c.id, 7, speakerId, listenerId, false, "AC7", string.Empty,
                "Keep your safety on unless you mean it. Ammo does not grow back."));
            c.dialogueEntries.Add(Entry(c.id, 8, speakerId, listenerId, false, "AC8", string.Empty,
                "Chill while you can. Vorograd never stays quiet for long."));

            for (int i = 1; i <= 8; i++)
                root.outgoingLinks.Add(Link(c.id, 0, i));
            return c;
        }

        private static Conversation MakeReadableTemplateConversation(int id, int actorId, int conversantId)
        {
            var c = BaseConversation(id, "Readable_Document_Template", actorId, conversantId,
                "ReadableDialogueAdapter sets Lua Variables readable_title / readable_body before starting this conversation.");
            AddLinearLines(c, actorId, conversantId,
                "[lua((Variable['readable_title'] or 'Document') .. \"\\n\\n\" .. (Variable['readable_body'] or '')))]",
                "Copy that.");
            return c;
        }

        private static Conversation BaseConversation(int id, string title, int actorId, int conversantId, string description)
        {
            var c = new Conversation
            {
                id = id,
                fields = new List<Field>(),
                dialogueEntries = new List<DialogueEntry>()
            };
            Field.SetValue(c.fields, "Title", title, FieldType.Text);
            Field.SetValue(c.fields, "Description", description, FieldType.Text);
            Field.SetValue(c.fields, "Actor", actorId.ToString(), FieldType.Actor);
            Field.SetValue(c.fields, "Conversant", conversantId.ToString(), FieldType.Actor);
            return c;
        }

        /// <summary>Alternating lines: even index spoken by <paramref name="aSpeaker"/>, odd by the other participant.</summary>
        private static void AddLinearLines(Conversation c, int aSpeaker, int aListener, params string[] lines)
        {
            var root = Entry(c.id, 0, aSpeaker, aListener, isRoot: true, "START", string.Empty, string.Empty);
            c.dialogueEntries.Add(root);
            int prev = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                int sp = (i % 2 == 0) ? aSpeaker : aListener;
                int ls = (i % 2 == 0) ? aListener : aSpeaker;
                int nid = i + 1;
                var e = Entry(c.id, nid, sp, ls, false, "L" + nid, string.Empty, lines[i]);
                c.dialogueEntries[prev].outgoingLinks.Add(Link(c.id, prev, nid));
                c.dialogueEntries.Add(e);
                prev = nid;
            }
        }

        private static DialogueEntry Entry(int conversationId, int id, int actor, int conversant, bool isRoot, string title, string menu, string dialogue)
        {
            var e = new DialogueEntry
            {
                id = id,
                conversationID = conversationId,
                isRoot = isRoot,
                fields = new List<Field>(),
                outgoingLinks = new List<Link>()
            };
            Field.SetValue(e.fields, "Title", title, FieldType.Text);
            Field.SetValue(e.fields, "Actor", actor.ToString(), FieldType.Actor);
            Field.SetValue(e.fields, "Conversant", conversant.ToString(), FieldType.Actor);
            Field.SetValue(e.fields, "Menu Text", menu, FieldType.Text);
            Field.SetValue(e.fields, "Dialogue Text", dialogue, FieldType.Text);
            Field.SetValue(e.fields, "Sequence", "None()", FieldType.Text);
            return e;
        }

        private static Link Link(int convoId, int origin, int dest) => new Link
        {
            originConversationID = convoId,
            originDialogueID = origin,
            destinationConversationID = convoId,
            destinationDialogueID = dest,
            isConnector = false
        };
    }
}
#endif

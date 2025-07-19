EXTERNAL StartQuest(questId)
EXTERNAL AdvanceQuest(questId)
EXTERNAL FinishQuest(questId)
EXTERNAL MoveNPCSequence(npcName, sequence)
EXTERNAL StartCombat(enemyTag, postCombatKnot)


// quest names
VAR CollectCoinsQuestId = "CollectCoinsQuest"
VAR KillEnemiesQuestId = "KillEnemiesQuest"

//quest states
VAR CollectCoinsQuestState = "REQUIREMENTS_NOT_MET"
VAR KillEnemiesQuestState = "REQUIREMENTS_NOT_MET"


INCLUDE collect_coins_start_npc.ink
INCLUDE collect_coins_end_npc.ink
INCLUDE Lamp.ink
INCLUDE kill_enemies_end_npc.ink
INCLUDE kill_enemies_start_npc.ink
INCLUDE DeadMan.ink
INCLUDE KnightOverworldTest.ink
EXTERNAL StartQuest(questId)
EXTERNAL AdvanceQuest(questId)
EXTERNAL FinishQuest(questId)


// quest names
VAR CollectCoinsQuestId = "CollectCoinsQuest"

//quest states
VAR CollectCoinsQuestState = "REQUIREMENTS_NOT_MET"


INCLUDE collect_coins_start_npc.ink
INCLUDE collect_coins_end_npc.ink
INCLUDE Lamp.ink

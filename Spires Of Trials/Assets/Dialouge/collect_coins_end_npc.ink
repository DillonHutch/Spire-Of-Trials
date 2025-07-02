=== collectCoinsFinished ===
{CollectCoinsQuestState:
- "FINISHED": -> finished
- else: -> default

}

= finished
Thank you!
-> END

= default
Hm? What do you want?
* [Nothing, I guess.]
-> END

* {CollectCoinsQuestState == "CAN_FINISH" }[Give Coins]
~ FinishQuest(CollectCoinsQuestId)
    oh? These are for me? Thank you!
-> END
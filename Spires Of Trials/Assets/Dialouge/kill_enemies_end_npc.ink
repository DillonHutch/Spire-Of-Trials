=== killEnemiesFinished ===
{KillEnemiesQuestState:
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

* {KillEnemiesQuestState == "CAN_FINISH" }[I killed them all.]
~ FinishQuest(KillEnemiesQuestId)
    Awesome!
-> END
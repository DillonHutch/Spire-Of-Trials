=== collectCoinsFinished ===
{CollectCoinsQuestState:
    - "FINISHED": -> finished
    - else: -> default
}

= finished
(He doesn't say a word. You just hear what sounds like coins clinking in his mouth.) #speaker:Dr. Green #portrait:ms_yellow_neutral #layout:right #audio:animalcrossinglow #object:false
-> END

= default
I NEED THOSE COINS NOW!!! #speaker:Dr. Green #portrait:ms_yellow_neutral #layout:right #audio:animalcrossinglow #object:false

PLEASE TELL ME YOU HAVE THEM! #speaker:Dr. Green #portrait:ms_yellow_neutral #layout:right #audio:animalcrossinglow #object:false

* [Nope]
THEN STOP WASTING MY TIME!

Those poor babies, all alone out there... #speaker:Dr. Green #portrait:ms_yellow_sad #layout:right #audio:animalcrossinglow #object:false #speed:0.06
-> END

* {CollectCoinsQuestState == "CAN_FINISH"} [Give Coins]
    ~ FinishQuest(CollectCoinsQuestId)
    AHAHAHAHAHAHAHAH #speaker:Dr. Green #portrait:ms_yellow_happy #layout:right #audio:animalcrossinglow #object:false #speed:0.01

    HAHAHAHAHAHAHAHAH

    AHAHAHAHAHAHAHAHAHAHAHAH

    YESSSSSSS

    COOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO #speed:0.001

    OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO

    OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO

    OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO

    OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO

    OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO

    OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOINS

    Thank you, brave adventurer! #speed:0.04

    What? My reaction is completely normal. #speaker:Dr. Green #portrait:ms_yellow_neutral #layout:right #audio:animalcrossinglow #object:false

    Can't a soldier have his own passions in life?
-> END

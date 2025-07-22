=== collectCoinsFinished ===
{CollectCoinsQuestState:
    - "FINISHED": -> finished
    - else: -> default
}

= finished
(He doesn't say a word. You just hear what sounds like coins clinking in his mouth.) #speaker:Dr. Green #portrait:ms_yellow_neutral #layout:right #audio:animalcrossinglow #object:false
-> END

= default
I told my pig headed friend to not leave a hole in the bag! #speaker:Dr. Green #portrait:ms_yellow_neutral #layout:right #audio:animalcrossinglow #object:false

Please tell me you have the coins?! #speaker:Dr. Green #portrait:ms_yellow_neutral #layout:right #audio:animalcrossinglow #object:false

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

    OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOINS

    Thank you, brave adventurer! #speed:0.04

    What? My reaction is completely normal. #speaker:Dr. Green #portrait:ms_yellow_neutral #layout:right #audio:animalcrossinglow #object:false

    Can't a soldier have his own passions in life?
-> END


=== collectCoinsCutscene ===


Oh gods what do we do... #speaker:Dr. Green #portrait:dr_green_sad #layout:left #audio:animalcrossing #object:false

Our boss will <b><color=\#FF1E35>kill</color></b> us if I don’t bring those coins back. #speaker:Dr. Green #portrait:ms_yellow_sad #layout:right #audio:animalcrossingmid #object:false

I would do it, but these bushes are in the way. #speaker:Dr. Green #portrait:dr_green_sad #layout:left #audio:animalcrossing #object:false

Hey you over there! Can you help us out? #speaker:Dr. Green #portrait:ms_yellow_neutral #layout:right #audio:animalcrossingmid #object:false



-> END






=== collectCoinsStart ===  
{CollectCoinsQuestState :
    - "REQUIREMENTS_NOT_MET": -> requirementsNotMet
    - "CAN_START": -> canStart
    - "IN_PROGRESS": -> inProgress
    - "CAN_FINISH": -> canFinish
    - "FINISHED": -> finished
    - else: -> END
}

= requirementsNotMet
Come back when you are a higher level! #speaker:Dr. Green #portrait:dr_green_neutral #layout:left #audio:animalcrossing #object:false 
-> END

= canStart

~ FocusCam("CoinLook")

We dropped some coins and need to get them back. #speaker:Dr. Green #portrait:dr_green_sad #layout:left #audio:animalcrossing #object:false

~ResetCamera()

Could you see if you can try and get them? 

* [Yes]
    ~ StartQuest("CollectCoinsQuest")
    ~ MoveNPCSequence("QuestGuardStart","up:1, down:1")
    ~ MoveNPCSequence("QuestGuardEnd","up:1, down:1")
    Great! We just need to get past these bushes. #speaker:Dr. Green #portrait:dr_green_happy #layout:left #audio:animalcrossing #object:false

    If only you had the power to <b><color=\#FFFF00>GO BACK</color></b> in <b><color=\#C71585>TIME</color></b>... #speaker:Dr. Green #portrait:dr_green_neutral #layout:left #audio:animalcrossing #object:false

* [No]
    Oh, okay then. I didn't really feel like we had that connection anyway. #speaker:Dr. Green #portrait:dr_green_sad #layout:left #audio:animalcrossing #object:false

 - -> END

= inProgress
How is collecting going? #speaker:Dr. Green #portrait:dr_green_neutral #layout:left #audio:animalcrossing #object:false

Did you figure out how to <b><color=\#FFFF00>GO BACK</color></b> in <b><color=\#C71585>TIME</color></b>?
-> END

= canFinish
You got the coins! Go hand them over to my boss over there. #speaker:Dr. Green #portrait:dr_green_happy #layout:left #audio:animalcrossing #object:false
-> END

= finished
May the gods look upon thee with favor! #speaker:Dr. Green #portrait:dr_green_happy #layout:left #audio:animalcrossing #object:false

...Or just thanks, I guess.
-> END

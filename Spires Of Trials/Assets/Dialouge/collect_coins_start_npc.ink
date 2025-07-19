
=== collectCoinsCutscene ===


Oh gods what do I do... #speaker:Dr. Green #portrait:dr_green_sad #layout:left #audio:animalcrossing #object:false

My boss will <b><color=\#FF1E35>kill</color></b> me if I don’t bring them back. #speaker:Dr. Green #portrait:dr_green_sad #layout:left #audio:animalcrossing #object:false

I would do it, but these bushes are in the way.

Hey you over there! Can you help me out?


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

I dropped some coins and need to get them back. #speaker:Dr. Green #portrait:dr_green_sad #layout:left #audio:animalcrossing #object:false


Could you see if you can try and get them? #speaker:Dr. Green #portrait:dr_green_neutral #layout:left #audio:animalcrossing #object:false

* [Yes]
    ~ StartQuest("CollectCoinsQuest")
    ~ MoveNPCSequence("QuestGuardStart","right:3,up:1,left:3,down:1")
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

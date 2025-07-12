=== killEnemiesStart ===  
{KillEnemiesQuestState :
    - "REQUIREMENTS_NOT_MET": -> requirementsNotMet
    - "CAN_START": -> canStart
    - "IN_PROGRESS": -> inProgress
    - "CAN_FINISH": -> canFinish
    - "FINISHED": -> finished
    - else: -> END
}

= requirementsNotMet
I'm hiding from my boss. #speaker:Dr. Green #portrait:dr_green_sad #layout:left #audio:animalcrossing #object:false

He has a weird affinity for coins.

Please come back once you've helped him.
-> END

= canStart
Oh, will you please help me? #speaker:Dr. Green #portrait:dr_green_neutral #layout:left #audio:animalcrossing #object:false

You see, I’m a pacifist.

I don’t want to kill anything...

But I need to return home.

I took on too much. I just want to do the right thing, you know?

Could you help me out and...

... #speed:0.5 #frequency:1

<b><color=\#FF1E35>take care</color></b> of the enemies for me? #speed:0.04 #frequency:2

If you do, it would mean the world to me.

* [Yes]
    ~ StartQuest("KillEnemiesQuest")
    Thank you, kind friend. #speaker:Dr. Green #portrait:dr_green_happy #layout:left #audio:animalcrossing #object:false

* [No]
    A fellow pacifist. Who am I to judge? #speaker:Dr. Green #portrait:dr_green_happy #layout:left #audio:animalcrossing #object:false

- -> END

= inProgress
Thank you for the help... #speaker:Dr. Green #portrait:dr_green_happy #layout:left #audio:animalcrossing #object:false

I’ve never met someone so kind!
-> END

= canFinish
You know, the other soldiers pick on me... #speaker:Dr. Green #portrait:dr_green_sad #layout:left #audio:animalcrossing #object:false

But you didn’t. You helped me. #speaker:Dr. Green #portrait:dr_green_happy #layout:left #audio:animalcrossing #object:false

You used your precious <b><color=\#C71585>TIME</color></b> to help me out.

That takes someone who really cares...

Thank you... #speed:0.1
~ FinishQuest(KillEnemiesQuestId)
-> END

= finished
You’ve given me hope in this world again. Thank you. #speaker:Dr. Green #portrait:dr_green_happy #layout:left #audio:animalcrossing #object:false #speed:0.04
-> END

=== killEnemiesStart ===  
{KillEnemiesQuestState :
    - "REQUIREMENTS_NOT_MET": -> requirementsNotMet
    - "CAN_START": -> canStart
    - "IN_PROGRESS": -> inProgress
    - "CAN_FINISH": -> canFinish
    - "FINISHED":  -> finished
    - else: -> END
    }
    

    
 = requirementsNotMet
I'm hiding from my boss.

He has a weird affinity for coins.

Please due come back when you have helped him. 
 
 
 
 -> END
 
 = canStart
 oh, will you please help me? #speaker:Dr. Green #portrait:dr_green_neutral #layout:left #audio:animalcrossing #object:false
 
 You see I am the worst soldier to exist.
 
 I don't want to kill anything...
 
 But I need to return home.
 
 Could you kill all the enemies?
 
 If you do ill give you quite the reward.

* [Yes]
    ~ StartQuest("KillEnemiesQuest")
    Thank you kind friend.  #speaker:Dr. Green #portrait:dr_green_happy #layout:left  #audio:animalcrossing #object:false
    
* [No]
    A fellow pacifist, who am I to judge.   #speaker:Dr. Green #portrait:dr_green_sad #layout:left  #audio:animalcrossing #object:false

- -> END
 
 = inProgress
 Thank you for the help... #speaker:Dr. Green #portrait:dr_green_neutral #layout:left  #audio:animalcrossing #object:false
 
 Gods I am so pathetic.
 -> END
 
 = canFinish
 You killed them all, you are a true hero!. #speaker:Dr. Green #portrait:dr_green_happy #layout:left  #audio:animalcrossing #object:false
 ~ FinishQuest(KillEnemiesQuestId)
 -> END
 
 = finished
 Thanks for killing them all! #speaker:Dr. Green #portrait:dr_green_happy #layout:left  #audio:animalcrossing #object:false
 -> END
    
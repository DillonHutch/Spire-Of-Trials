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
 Help my friends at the beginning first then come talk to me! #speaker:Dr. Green #portrait:dr_green_neutral #layout:left #audio:animalcrossing #object:false
 -> END
 
 = canStart
Kill all enemies and then come and report back to me! #speaker:Dr. Green #portrait:dr_green_neutral #layout:left  #audio:animalcrossing #object:false

* [Yes]
    ~ StartQuest("KillEnemiesQuest")
    Thats the spirit! #speaker:Dr. Green #portrait:dr_green_happy #layout:left  #audio:animalcrossing #object:false
    
* [No]
    You are a disgrace.  #speaker:Dr. Green #portrait:dr_green_sad #layout:left  #audio:animalcrossing #object:false

- -> END
 
 = inProgress
 How the killing going? #speaker:Dr. Green #portrait:dr_green_neutral #layout:left  #audio:animalcrossing #object:false
 -> END
 
 = canFinish
 You killed them all, you are a true hero!. #speaker:Dr. Green #portrait:dr_green_happy #layout:left  #audio:animalcrossing #object:false
 ~ FinishQuest(KillEnemiesQuestId)
 -> END
 
 = finished
 Thanks for killing them all! #speaker:Dr. Green #portrait:dr_green_happy #layout:left  #audio:animalcrossing #object:false
 -> END
    
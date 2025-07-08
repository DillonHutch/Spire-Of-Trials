=== collectCoinsStart ===  
{CollectCoinsQuestState :
    - "REQUIREMENTS_NOT_MET": -> requirementsNotMet
    - "CAN_START": -> canStart
    - "IN_PROGRESS": -> inProgress
    - "CAN_FINISH": -> canFinish
    - "FINISHED":  -> finished
    - else: -> END
    }
    

    
 = requirementsNotMet
 Come Back when you are a higher level! #speaker:Dr. Green #portrait:dr_green_neutral #layout:left #audio:animalcrossing #object:false
 -> END
 
 = canStart
Will you collect 5 [wave]coins[/wave] and bring them to my friend over there? #speaker:Dr. Green #portrait:dr_green_neutral #layout:left  #audio:animalcrossing #object:false

* [Yes]
    ~ StartQuest("CollectCoinsQuest")
    Great! #speaker:Dr. Green #portrait:dr_green_happy #layout:left  #audio:animalcrossing #object:false
    
* [No]
    Oh, ok then. Come back if you change your mind.  #speaker:Dr. Green #portrait:dr_green_sad #layout:left  #audio:animalcrossing #object:false

- -> END
 
 = inProgress
 How is collecting going? #speaker:Dr. Green #portrait:dr_green_neutral #layout:left  #audio:animalcrossing #object:false
 -> END
 
 = canFinish
 You got all the coins? Great go turn them in to my friend. #speaker:Dr. Green #portrait:dr_green_happy #layout:left  #audio:animalcrossing #object:false
 -> END
 
 = finished
 Thanks for getting those pal! #speaker:Dr. Green #portrait:dr_green_happy #layout:left  #audio:animalcrossing #object:false
 -> END
    
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
 Come Back when you are a higher level!
 -> END
 
 = canStart
Will you collect 5 coins and bring them to my friend over there?

* [Yes]
    ~ StartQuest("CollectCoinsQuest")
    Great!
    
* [No]
    Oh, ok then. Come back if you change your mind. 

- -> END
 
 = inProgress
 How is collecting going?
 -> END
 
 = canFinish
 You got all the coins? Great go turn them in to my friend.
 -> END
 
 = finished
 Thanks for getting those pal!
 -> END
    
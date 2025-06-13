INCLUDE globals.ink

I'm not like the other skeletons. #speaker:Dr. Green #portrait:dr_green_neutral #layout:left

-> main

=== main ===
I don't have the <b><color=\#FF1E35>guts</color></b> to hurt anyone! #portrait:dr_green_happy

So you need some help down here?  #portrait:dr_green_neutral
+ [Yes]
    Well then let me help you! #portrait:dr_green_happy

        -> help
+[YES!!!!!!!]
    Wow so enthusiastic! #portrait:dr_green_happy

        -> help
+[No]
    Oh..... okay then. #portrait:dr_green_sad


->END

=== help ===
Watch out for my fellow skeletons!

If you run into them they will fight you along with their friend!

You'll want to block their with your sheild.

Then make sure you are in the same spot where they are...

and hit them with the correct attack. 

They should only need four hits to go down. 

So did you get all that?
+[Yes]
    Wounderful!!! You forget feel free to come back and talk to me!
    -> DONE
    
+[No]
    Okay from the top then!
    
    -> help


->END
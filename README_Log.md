# SE_ShipMagicScript

/*
        #############################################################################################################
        #                                   Space Engineers Ship Magic Script                                       #
        #                   Made by Paulo Magalhães aka Skaer aka MangaYuurei aka Ghost of Magellan                 #
        #############################################################################################################                   
        #                                                   Purpose                                                 #
        #              Create assistance scripts for Space Engineers ships and learn C# in the process              #   
        #   One script runs in every ship. User only requires to configure the "Main" according to the need even    #
        #   without programming knowledge. Just follow the instructions and keep the "Vital" section of the main    #
        #   undisturbed and you'll be just fine. If you're a programmer, this might be interesting for you:         #
        #   https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts         #
        #                                                                                                           #
        #############################################################################################################

        #############################################################################################################
        #                            Log, problem observation and implemented solution                              #
        #############################################################################################################
        #   v01.01 - Problem: Hard to understand when and which cargo containers are full. Drill inventories        #
        #       literally blow up when you try to drill materials over their capacity. User inventory is composed   #
        #       of as many interfaces as there are inventory types.                                                 #
        #       Solution: create algorithm to check every cargo container (including tool inventories like drills,  #
        #       welders and grinders), verify their current and total volume and calculate a percentage of the      #
        #       ocupied space. Printed it to a LCD screen in the ship                                               #
        #   v01.02 - Problem: Tool and general cargo inventories were initally combined but this caused problems    #
        #       observing critical volumes on the tools, so they still went BOOM.                                   #
        #       Solution: Created a single method that returned a list of values read from the intended inventory   #
        #       type which we can select from a group of optional parametres. Reads all by default                  #
        #   v01.03 - Problem: materials of less importance take up too much space. Unloading it all by hand takes   #
        #       up too much time and is inexcusably boring. Died because I ran out of oxygen while mining and I     #                                               
        #       couldn't remote control the ship because I turned off the antenna in favor of a beacon to save      #
        #       energy. Which reminds me that I have no idea about my fuel and energy reserves.                     #
        #       Solution: Added sorters and expulsion blocks which, once activated, expell stone and/or iron ore    #
        #       which are easy to find and process leaving space for more important and rare types of ore. Status   #
        #       of the sorters are printed along with the other data in the LCD. Added method that verifies ice     #
        #       reserves in the O2/H2 generator. Changed the cargo verification method to verify every type of      #
        #       inventory currently in existance in SE, including reactors to know fuel reserves. Added method to   #
        #       verify current and total energy reserves in the batteries. Tried to make another method to calculate#
        #       expected longevity of that energy but power comsumption conditions are too irregular for a correct  #
        #       reading. Its useful, but not precise enough to take lightly in a life or death situation            #
        #       Added a method to  that verifies player presence in the cockpit; if present, turn on beacon and     #
        #       change batteries mode to Auto: if not, turn off beacon and turn on antenna to allow remote control, #
        #       and change batteries to recharge mode to allow solar panels to charge energy reserves.              #
        #   v01.04 - Problem: shifting materials from the drill ship to the station inventories is sheer mental     #
        #       torture and clicking hell. To save up boredom I either have to create bigger cargo containers and   #
        #       be forced to increase ship size, or keep ship size by adding smaller cargo containers to fill the   #
        #       gaps between components for greater size efficiency but increase the number of inventories to clean #
        #       by hand.                                                                                            #
        #       Solution. Added a method that drains every item of acertain category (in this case, ores) and places#
        #       them in the containers of another ship (Warning: i could not find a way to specify which ship grid  #
        #       to send the mats to, since SE libraries and documentation only identify THIS SHIP or NOT THIS SHIP  #
        #       so if there are other ships connected to the station, its possible they'll end up with the drained  #
        #       items. Use with care). Like this, it doesn't matter what kind of cargos you place, you'll never have#
        #       to look at them anyway. Added tool indicator to tell you if one of your tools went boom.            #
        #   v02.00 - Problem: Finished process with the drill ship and went to add welder functionalities. Unlike   #
        #       drill ship where only % capacity matters, on the welder I need to know what types of materials I    #
        #       have and their stock.                                                                               #
        #       Solution: remade getCargoCount() to not only return % capacity but also a count of every material   #
        #       of a certain type and print it into a LCD                                                           #
        #   v02.01 - Problem: In the drill I had to click the hell out of every inventory to clear everything of    #
        #       ores. Here I have the opposite problem; I have to fill up the welder ship inventories with a variety#
        #       of mats and try to fit as many as possible.                                                         #
        #       Solution: added a dictionary of required parts returned by desiredMats() and methods that search    #
        #       for the availability of these mats, calculate the volume vs available space in the welder ship and  #
        #       transfers them according to size (large items to large cargo, small items to small cargo or to large#
        #       cargo if small is full). Added several intermediate methods that clean, convert or simplify data    #
        #   v02.02 - Problem: current transfer type transfers mats to the first available container with space to   #
        #       accept materials. My OCD tingles with barely repressed rage everytime I see the disorganized        #
        #       containers.                                                                                         #
        #       Solution: Added optional parametre to transferItems methods to search destination containers for    #
        #       items of the same type or subtype and take preference in transfering them there if possible and if  #
        #       not to any other available container. Note that this option is heavier as it has more cycles        #
        #   v02.03 - Problem: SE updates made many library functions redundant or no longer functional.             #
        #       Solution: remake the damned thing and pray it doesn't happen often.                                 #
        #   v03.00 - Problem: Assemblies keep getting clogged with iron and can't pull other variety of materials   #
        #       to build more complex components. Refineries pull the first type of ore available and, since I'm    #
        #       a binge miner, they spend hours refining the same type of material.                                 #
        #       Solution: Assemblies and refineries have 2 inventory types, (0) for the raw materials and (1) for   #
        #       processed materials. Created methods that search both, clear up (1) whenever its over a certain %   #
        #       to assure production can never stop for lack of space where to put processed mats and other methods #
        #       to verify mats in (0) and assure they always keep to certain numbers of raw materials so production #
        #       never stops for lack of a certain raw material because (0) is filled to capacity by another raw     #
        #       material that we have in excess. (Work In Progress)                                                 #
        #   v03.01 - Problem: Using the dictionaries was a nice first attempt, but it's starting to show its        #
        #       limitations.                                                                                        #
        #       Solution: convert everything into classes that contain specific info, separate into types and       #
        #       easily accessible by all parts of the code without needing convoluted conversion methods or         #
        #       less efficient ways to access and use the data. Plus, it's easier to navigate.                      #
        #       (Work in Progress. Styding MDK and utility classes used in Space Engineers)                         #
        #	(This rework led me to take out the transfer, manage facilities and current listing functions)	          #
        #   v04.00 - Future                                                                		                      #
        #       Problem: Large ships with 225 or 289 drills consume too much CPU and GPU processing power.          #
        #       Solution: Old man Magellan is going to have to learn how to mod SE, otherwise he'll never build a   #
        #       megastructure without buying a new PC or transforming his old one into molten magma. Create a 5x5   #
        #       Drill that uses only one animation and thus consumes 1/25th of the current processing. 300 drills   #
        #       turn into 225/25=9 and 289/25=11.59 mega drills (can actually increase size of Planet Cracker class #
        #       drill ship further)                                                                                 #
        #############################################################################################################
        */

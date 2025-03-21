# Reus 2 Planet Surveyor
This desktop app reads the save files for completed Reus 2 planets and produces a spreadsheet of stats. It is written in C# and uses the .NET framework.
(Not affiliated officially with Reus 2 or its developer and publisher.) Tested for Reus 2 v1.4.3 and up.
![image](https://github.com/user-attachments/assets/835d8993-bad3-4cb7-a1a1-2f0c0b3048c8)
## Usage Instructions
0. (If building from source code) Build the project. Make sure the output location contains named "Glossaries". The CSV files within that folder are used by the application to identify and name game objects.
1. Locate your Reus 2 profile using the **\[Find Profile\]** button. The text box to the right of the button will have green text if the profile folder looks correct, and red if it does not.
2. Click **\[Decode Planets\]** to read the planet files. If the **Write Decoded** box is checked, the program will also write the decoded save files into `/Decoded`. (TODO: The Table will also update to show basic information on each read planet.)
3. Once the planets have been read, the **\[Export XLSX\]** button will become ready. Clicking it will produce an Excel workbook (XLSX) file in `/Output`.
## To Do/WIP Features
* Biotica Stat Table: Account for biotica unlocked with Evolve
* Planet Summary Table: Scenario and difficulty information
* Summary tables for Cities
* Stat table for Spirits
  * Table for spirits v. buildings, eras
* Stat table for Luxuries
* Stat table for Eras
* Misc. stat table
 * Biotica as pets
 * Biotica used as inspiration for original characters
* Error logging to log exceptions by planet/object
* Guardrails for missing information
* Background workers
* Table UI for planet information
* Option to read only a selection of planets
* UI decoration
## Potential Features
* Pre-read summary `.mgs` files.
* Option to select which save point (complete, left, etc.) to reach from each planet
* Stat table for Micros and (pre-Micro Kingdom) placed Emblems

# Reus 2 Planet Surveyor
This fan-made desktop app reads the save files for completed Reus 2 planets and produces a spreadsheet of stats. It is written in C# and uses the .NET framework.
(Not affiliated officially with Reus 2 or its developer and publisher.) Tested for Reus 2 v1.3.0 (Atlantic Forest) and up.

![UI Pic v3](https://github.com/user-attachments/assets/50b12e7c-2fac-4db8-b55a-aaf8e7417e6d)
## Usage Instructions
0. (If building from source code) Build the project. Make sure the output location contains named "Glossaries". The CSV files within that folder are used by the application to identify and name game objects.
1. Locate your Reus 2 profile using the **\[Find Profile\]** button. The text box to the right of the button will have green text if the profile folder looks correct, and red if it does not. The table will populate with the planets it has found in `/profile_<#>/sessions`.
2. The checkboxes in the `Read?` column mark a planet to be read and processed. Only planets that have a completed game session (having a `auto_complete.deux` save file in the planet's folder) can be selected. The `Check All` and `Check None` buttons can be used to clear or check the boxes for all the available planets.
3. Click **\[Decode Planets\]** to read the planet files. If the **Write Decoded** box is checked, the program will also write the decoded save files into `/Decoded`. As the program reads planets, it will fill out the table with information such as the planet's founding spirit, the mini map of the biomes, and the giants.
4. Once the planets have been read, the **\[Export XLSX\]** button will become ready. Clicking it will produce an Excel workbook (XLSX) file in `/Output`.
## Output
This program outputs an Excel workbook (.xlsx) file. It has the following sheets:
1. **Planets** - A ledger summarizing each completed planet in the profile. It includes starting information (giants, starting spirit, etc.), era performance, summary information for cities (charcters chosen, total prosperity scores, etc.), summary information for biotica (amount and unique types placed, etc.) and biome information.
2. **Cities** - A listing of each city present on the planets. For each, there are prosperity stats, stats on biotica in borders, and buildings.
3. **Spirits** - A sheet grouping city stats by leader.
4. **Biotica** - Usage stats for each bioticum type.
5. **BioticaVsChar** - A table showing how many of each bioticum were placed in the borders of each spirit's cities. 
## To Do/WIP Features
* Biotica Stat Table: Account for biotica unlocked with Evolve
* Planet Summary Table: Scenario and difficulty information
* Stat table for Spirits
  * Table for spirits v. buildings, eras
* Stat table for Luxuries
* Stat table for Eras
* Misc. stat table
 * Biotica as pets
 * Biotica used as inspiration for original characters
## Potential Features
* Stat table for Micros and (pre-Micro Kingdom) placed Emblems

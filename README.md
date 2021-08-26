## Contract Hiring Hubs
#### This is a Variety X Immersion (VXI) mod for Harebrained Schemes' Battletech.<br> VXI is Lore based interpretation that brings Variety crossed with Immersion.

#####           Name - VXI_ContractHiringHubs
#####            DLL - VXIContractHiringHubs.dll
#####        Version - 0.3.1.0,
#####         Author - RJPhoenix

### Special Thanks	
- Haree78, as this is inspired by BTE 3025 CE (BEX) - https-//discord.gg/DJ8YRAbt 
- Morphyum, for the inspiration and code examples (MercDeployment) - https-//github.com/BattletechModders/MercDeployments
- mcb, for his code tips and excellent mods - https-//github.com/mcb5637
- Carcer, for invaluable proof reading and editing
- Seraph Padecain, for two of my recruit pilot backstories
- Beta Testers - mcb, paulobrito, cww1964, Darkstalker Kagami, jhelivar

### Dependencies		
- VXI_ContractManagement mod - Stand alone mod written for ContractHiringHubs to create and track custom contracts.

### Conflicts		
- No known conflicts)

### Features
- **Merc Guilds** : This feature brings Travel Contracts, Travel Raids and Kicks off Deployments on key worlds, in particular Outreach and Terra which have no contracts in BEX.
- **Merc Deployment** : Extended deployments on random planets with a mix of Allies and Enemies thrown in.  Also includes stats, progressive training missions and other easter eggs.
- **Merc Pilots** : This introduces special Merc pilots, especially Rogue Clan specialists and Bondsman via Clan missions.
- **Special** : Pause after jump, to enable: Left-Alt + Click Navigation (a pop up should announce disable/enable)

#### [Version 0.1.0.0]
##### MercGuilds 		
- The purpose of this feature was to bring Random travel contracts to special Merc specific worlds most Notably Outreach, Terra, Gelatea and the Faction Capitals 
- Refer to Mods.json for full list of worlds affected
- Includes financial assistance while travelling and generous salvage and payment options to compensate for travel
									
#### [Version 0.2.0.0]
##### MercDeployment
- This mod uses a randomly chosen MercGuild travel contract to launch a multi-day (default 30days) deployment for a single employer against all the factions of the chosen system
- Restricted to pre-configured missions for the following reasons-
- Custom text creates a variety of briefings that match the mission details based on Political/Military goals and Defense/Attack missions.
- Morality system based on the ethics range of the given missions (includes a reputation change)
- Special Easter Egg missions (Training, etc)
- Mission payment bonuses to reward effort over the deployment
- Elite Squads and Off World Allies (esp. later in Waves)
- Introduced Waves to add additional features (like Off World Allies)

#### [Version 0.2.3.0]
##### MercDeployment
- Change to Reputation to include additional factions and adjusted overall rep gain/loss
- Now has more contracts (default 9) to choose from in each Wave, clears after max contracts reached (default 5)
- Darius updates Deployment details on Contract refresh
- Deployment ends as soon as final day ends
- Added new missions to deployment- Blackout, Firemission, Attack/Defend, and Duels

##### MercGuilds
- Removed Aurigan Restoration from Career
- Improved Deployment creation to avoid Allies fighting

#### [Version 0.2.4.1]
##### MercDeployment
- Reputation based Contract Choice and Max (refer to mod.json)
- Fixed issue with final Reputation gain for Employer
- Fixed issue with Darius pop up for Wave 3
- Added text to Toppling Golliath mission briefing for weight restriction

#### [Version 0.3.0.0]
##### MercPilots
- Clan specialist pilots may appear in Hiring Hall (particularly on Merc Guild worlds)
- Post clan missions has a rare chance (increasing over time) to recruit a Clan Bondsman

##### MercDeployment
- Additional Training missions per wave
- Training recruits level up as you train them with special post deployment results

##### MercGuilds
- Clan travel raids for special Lore based worlds (Wolcott, Tukkayid)
- New factions for raids post 3052-04 : ComStar Reformation & Word of Blake (WoB is placceholder for now)
- *Bug fix* : FP appearing as Story mission
- *Bug fix* : Merc Worlds incorrectly identified
- *Bug fix* : Clan contracts (incl. Xenophobe) no longer appear for IS contracts

#### [Version 0.3.1.0]
##### MercPilots
- New Life Paths for Mech Warriors in the hiring hall including Vehicle experience (see full list of new tags at base of this file)
- Pilot Factions weighted toward System Owner, Near by systems and Factions with the largest populations and military

##### MercDeployment
- Excluded Clans from Deployments
- Recruit Pilot Initial Backgrounds completed
- Full XOTL tables for Word of Blake
- *Bug fix* : Alter contract text on loading screen (Logging, partial implementation)

##### MercGuilds
- Excluded Clans from Travel Missions as Raids cover them more specifically
- Blacklist Capitals from travel locations (Config option)
- Capitals contain travel only mission (Config option)
- CastDef for Rogue Blakists
- *Bug fix* : Minor Merc Worlds incorrect comparisons
- *Bug fix* : Removed WoB and ComStarRef as fallback factions so shouldn't appear before 3052

### Future Features
- **Merc Guilds** : New Rewards- Military cache; Special Pokepilot collection; and Faction based pre-/mid-/post- mission assistance.
- **Merc Conflicts** : - An extension of MercDeployment this will be used to create random conflicts or more importantly mimic Lore based conflicts using relatively simple configurable mission links.
- **Merc Pilots** : - Pokepilot, got to collect them all, expands the Merc Pilots

#### New Pilot Tags

|TYPE	| NEW TAGS	| MEANINGS |
|-------|-----------|----------|
| Level0 | "pilot_apprentice" | No prior experience and young of age |
| Level0 | "pilot_humble" | Humble upbringing
Level0 | "pilot_coreward" | Coreward Periphery Pilots |
| Level0 | "pilot_deepspace" | Deep Periphery Pilots |
| Level0 | "pilot_independent" | Independent Periphery Pilots |
| Level0 | "pilot_kingdom" | Bandit Kingdoms Pilots (Current or Reformed) |
| Level0 | "pilot_outworld" | Outworld's Alliance Pilots |
| Level0 | "pilot_rimward" | Rimward Periphery Pilots |
| Level1 | "pilot_privateer" | Worked as a Privateer (Legally Sanctioned Pirate) |
| Level1 | "pilot_underworld" | Underworld Connections |
| Level1 | "pilot_discharged" | Early Discharge due to misconduct |
| Military | "pilot_fighterpilot" | Fighter Pilot (Airborn) |
| Military | "pilot_mechpilot" | Mech Pilot |
| Military | "pilot_recruitinfantry" | Enlistment in an Infantry Corp |
| Military | "pilot_recruitmerc" | Enlistment in a Merc Company |
| Military | "pilot_recruitnavy" | Enlistment in a Naval Corps |
| Military | "pilot_recruitvehicle" | Enlistment in a Combat Vehicle Corps |
| Military | "pilot_sensors" | Sensor Specialist (Navy, or Vehicles) |
| Military | "pilot_cadet" | Started Officer Training |
| Military | "pilot_flight" | Conventional Flight School training |
| Military | "pilot_military" | Joined the Military *Existing Tag, new meaning |
| Noble | "pilot_heir" | Heir to noble family heritage |
| Noble | "pilot_nonheir" | Noble Supernumery, non-heir to noble family heritage |
| Specialist | "pilot_comms" | Communications Training (Infantry, Navy, or Vehicles) |
| Specialist | "pilot_driver" | Driving Specialist (Vehicle) |
| Specialist | "pilot_gunnery" | Gunnery Special (Infantry, Navy, or Vehicle) |
| Specialist | "pilot_helmsman" | Piloting Specialist (Navy) |
| Specialist | "pilot_intelligence" | Special Operations Intelligence Specialist |
| Specialist | "pilot_recon" | Recon Specialist (Infantry) |
| Veteran | "pilot_bushido" | Kurita Ranked Military |
| Veteran | "pilot_infantry" | Infantry Veteran |
| Veteran | "pilot_navy" | Navy Veteran |
| Veteran | "pilot_vehicle" | Combat Vehicle Veteran |
| Veteran | "pilot_mechwarrior" | Mechwarrior Veteran *Existing Tag, new meaning |
| Veteran | "pilot_aeropilot" | Aerospace Fighter Pilot Veteran

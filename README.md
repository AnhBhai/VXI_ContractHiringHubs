Name			:	VXI_ContractHiringHubs
DLL				:	VXIContractHiringHubs.dll
ReadMe Version	: 	0.2.0.0,
Author			:	RJPhoenix

Special Thanks	:	Haree78, as this is inspired by BTE 3025 CE (BEX) - https://discord.gg/DJ8YRAbt 
				: 	Morphyum, for the inspiration and code examples (MercDeployment) - https://github.com/BattletechModders/MercDeployments
				:	mcb, for his code tips and excellent mods - https://github.com/mcb5637
				:	Carcer, for invaluable proof reading and editing 
				:	Beta Testers : mcb, paulobrito, cww1964, Darkstalker Kagami, jhelivar

VXI Description	:	This is a Variety X Immersion mods for Harebrained Schemes' Battletech.  VXI is Lore based interpretation that brings Variety crossed with Immersion.

Dependency		:	VXI_ContractManagement mod - Stand alone mod written for ContractHiringHubs to create and track custom contracts.
				:	Timeline mod - This is an unconfirmed dependency for time based triggers.

Conflicts		:	(At this stage there is no known conflicts)

Features		
				:	[Version 0.1.0.0]

				:	MercGuilds 		- The purpose of this feature was to bring Random travel contracts to special Merc specific worlds most Notably Outreach, Terra, Gelatea and the Faction Capitals 
									- Refer to Mods.json for full list of worlds affected
									- Includes financial assistance while travelling and generous salvage and payment options to compensate for travel
									
				:	[Version 0.2.0.0]

				:	MercDeployment 	- This mod uses a randomly chosen MercGuild travel contract to launch a multi-day (default 30days) deployment for a single employer against all the factions of the chosen system
									- Restricted to pre-configured missions for the following reasons:
										- Custom text creates a variety of briefings that match the mission details based on Political/Military goals and Defense/Attack missions.
										- Morality system based on the ethics range of the given missions (includes a reputation change)
										- Special Easter Egg missions (Training, etc)
									- Mission payment bonuses to reward effort over the deployment
									- Elite Squads and Off World Allies (esp. later in Waves)
									- Introduced Waves to add additional features (like Off World Allies)

				:	[Version 0.2.3.0]

				:	MercDeployment 	- Change to Reputation to include additional factions and adjusted overall rep gain/loss
									- Now has more contracts (default 9) to choose from in each Wave, clears after max contracts reached (default 5)
									- Darius updates Deployment details on Contract refresh
									- Deployment ends as soon as final day ends
									- Added new missions to deployment: Blackout, Firemission, Attack/Defend, and Duels

				:	MercGuilds 		- Removed Aurigan Restoration from Career
									- Improved Deployment creation to avoid Allies fighting
									
				:	[Version 0.2.4.1]

				:	MercDeployment 	- Reputation based Contract Choice and Max (refer to mod.json)
									- Fixed issue with final Reputation gain for Employer
									- Fixed issue with Darius pop up for Wave 3
									- Added text to Toppling Golliath mission briefing for weight restriction

									
Future Features	
				:	MercDeployment 	- New Rewards: Military cache; Special Pokepilot collection; and Faction based pre-/mid-/post- mission assistance.

				:	MercConflicts 	- An extension of MercDeployment this will be used to create random conflicts or more importantly mimic Lore based conflicts using relatively simple configurable mission links.
				
				:	MercPilots		- Pokepilot, got to collect them all, expands the MercGuild idea by creating MechWarrior pilot hiring hubs with chance of finding rare Veteran Pilots.

## Contract Management
#### This is a Variety X Immersion (VXI) mod for Harebrained Schemes' Battletech.<br> VXI is Lore based interpretation that brings Variety crossed with Immersion.

#####           Name : VXI_ContractManagement
#####            DLL : VXIContractManagement.dll
#####        Version : 0.3.0.0,
#####         Author : RJPhoenixName

### Special Thanks	
- Haree78, MCB, Pode and Beta testers for their help and inspiration for this mod and its dependencies.

### Dependencies		
Dependency		:	Timeline mod - This is an unconfirmed dependency for time based triggers.

### Conflicts		
Conflicts		:	At this stage there is no known conflicts - This mod is only activated when it is called from another mod.

### Features
- **Contract Generation** : Standalone Contract generation that expands the customisation of contract generation beyond what HBS offers
- **Contract Gen Faction** : Faction functionality to support Contract Generation
- **Contract Hiring Hubs** : Features to support Contract Hiring Hubs, Merc Guilds and Deployments.

#### [Version 0.1.0.0]
##### MercGuilds 
- Created to support MercGuilds
	
##### ContractGeneration		
- Ability to generate contracts for a given source and target system with control over several details 
- Randomly selects valid missions based on Biomes and factions
- Includes fixed or random selections of employer, target, allies and hostile generation
- Also Global, Priority, Negotiable or otherwise mission inputs
- Allows for any type of Travel mission to be created
- Factions can be set or randomly generated.
											
##### ContractGenFaction
- To ensure missions adhere to contract requirements
- Generate random factions based on source and target systems owners
- Other features to assist faction control in ContractGeneration
											
#### [Version 0.2.0.0]
##### MercGuilds
- Updated with fixes to 0.1.0.0

##### MercDeployments 
- Created to support Merc Deployments
				
##### ContractGeneration
- Cleaned up to fix for Non-Travel contract generation - Specifically for the MercDeployment feature of XI's ContractHiringHubs mod.
- Expanded Travel contract creation to include any type of contract (Travel or Non-Travel)
- Expanded to include Contract Builder that 
- Generate contract from a given Override.
- Allow the modification of Mission Briefing Text, if required
- Ability to retrieve all valid ContractOverrides for a given system based on Maps and Encounters in that system
											
##### ContractGenFaction
- Expanded to include option for non-system owners in random Target generation

#### [Version 0.3.0.0]
##### MercDeployments 
- Extension of MercDeployments to support setting of Contract LanceDef detais during creation.
				
### Future Features
- **Contract Generation** : As required by future mods.
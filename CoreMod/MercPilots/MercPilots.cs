using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using BattleTech;
using Helpers;
using BattleTech.UI.Tooltips;
using HBS.Collections;
using static Helpers.GlobalMethods;
using UnityEngine;
using BattleTech.UI;
using BattleTech.Portraits;
using System.IO;

namespace VXIContractHiringHubs
{
    static class MercPilots
    {
		public static int PilotPortraitNo = 0;

		public static void LastUsedMechs(SimGameState simGame, Pilot pilot, string MechName)
        {
			
        }
        public static List<PilotDef> GetPilotList(SimGameState simGame, List<string> tagDesc = null)
        {
            List<PilotDef> pilotDefs = simGame.RoninPilots;

            if (tagDesc != null)
            {
                foreach (string tag in tagDesc)
                {
                    pilotDefs = pilotDefs.FindAll(x => x.PilotTags.Contains(tag));
                }
            }
            return pilotDefs;
        }

        public static bool RetrieveClanPilot(SimGameState simGame, out PilotDef pilotID)
        {
            List<String> tagDesc = new List<string>();
            tagDesc.Add("pilot_clan");
            tagDesc.Add("VXI");
            List<PilotDef> pilotDefs = new List<PilotDef>();

            pilotDefs = GetPilotList(simGame, tagDesc);

            if (pilotDefs.Count() > 0)
            {
                pilotDefs = pilotDefs.FindAll(x => !simGame.UsedRoninIDs.Contains(x.Description.Id));

                if (pilotDefs.Count() > 0)
                {
                    pilotID = pilotDefs.GetRandomElement();
                    return true;
                }                
            }
            
            pilotID = null;
            return false;
            //simGame.RequestItem<PilotDef>(pilotDefID, delegate (PilotDef obj)
        }
        public static void UpdateMercPilots(SimGameState simGame)
        {
            
            bool isNewPilot = false;
            PilotDef pilotAdd;
			int pilotChance;

			if (Main.Settings.MercGuildPilotClanMRBPct.Count > simGame.GetCurrentMRBLevel())
				pilotChance = Main.Settings.MercGuildPilotClanMRBPct[simGame.GetCurrentMRBLevel()];
			else
				pilotChance = Main.Settings.MercGuildPilotClanMRBPct.Last();

			Log.Info($"Pilot Chance: {pilotChance} with MRB Level: {simGame.GetCurrentMRBLevel()}");

			// temp for testing
			//pilotChance = 100;

			if (pilotChance >= simGame.NetworkRandom.Int(0, 100))
			{
				isNewPilot = RetrieveClanPilot(simGame, out pilotAdd);
				Log.Info($"Adding Pilot: {pilotAdd.Description.Name}");

				if (isNewPilot && pilotAdd != null)
				{
					simGame.AddPilotToHiringHall(pilotAdd, simGame.CurSystem);
				}
			}

            InfoClass.MercPilotInfo.IsGenInitPilots = false;
        }

		public static void NoBondsman(SimGameState simGame, FactionValue clanName, PilotDef pilotDef = null)
        {
			string desc = "BondRef";
			string message;

			string clanShortName = clanName.FactionDef.ShortName;
			if (clanShortName.Contains("Clan "))
				clanShortName.Replace("Clan ", "");

			if (pilotDef == null)
            {
				InfoClass.MercPilotInfo.BondsrefCount++;

				// Report BondRef
				if (InfoClass.MercPilotInfo.BondsrefCount <= 1)
					message = $"One of the {clanShortName} clanners claimed he didn't want to be our bondsman and his mate then shot him.  Weirdest thing I've seen all week.";
				else if (InfoClass.MercPilotInfo.BondsrefCount <= 2)
					message = $"Well a {clanShortName} clanner just did the disturbing ritual suicide thing again.  They told us it was for his honour, death before being a mercenary or something. Its still too weird but I'm likening this to Suppuku and that seems to make sense.";
				else if(InfoClass.MercPilotInfo.BondsrefCount <= 7)
					message = $"Well another battle and another clanner just did an honour death.  It seems Clan {clanShortName} actually hold us in some kind of regard but not enough regard to avoid eating a laser blast.";
				else
					message = $"It looks like we're moving up in the world of Clanner death before dishonour.  Clan {clanShortName} are starting to speak glowingly of our company, despite us being mercs. Still this one prefered to get shot, to the alternative.";
			}
			else
            {
				desc = "Bondcord";
				if (InfoClass.MercPilotInfo.BondsmanMax <= 0)
				{
					InfoClass.MercPilotInfo.BondsmanMax++;
					message = $"{pilotDef.Description.FullName()} walked us through the process of tying a Bondcord to their wrist and cutting it, so they can return to their clan with honour intact.";
				}
				else
                {
					if (InfoClass.MercPilotInfo.BondsmanMax <= 5)
						InfoClass.MercPilotInfo.BondsmanMax++;

					message = $"{pilotDef.Description.FullName()} was surprised when we took them through the ritual of bondcord removal and let them go back with their honour intact.";
				}

				
				// Report BondRef
			}

			PauseNotification.Show(desc, message, simGame.GetCrewPortrait(SimGameCrew.Crew_Darius), string.Empty, true);

		}

		public static bool EndClanMission(SimGameState simGame, FactionValue clanName)
        {
			bool result = false;

			string clanShortName = clanName.FactionDef.ShortName;
			if (clanShortName.Contains("Clan "))
				clanShortName.Replace("Clan ", "");

			int bondChance = Main.Settings.MercPilotBondsman + Math.Min(Main.Settings.MercPilotBondsmanMax + InfoClass.MercPilotInfo.BondsmanMax, InfoClass.MercPilotInfo.BondsrefCount);
			int refChance = Main.Settings.MercPilotBondsref + (int)(Math.Min(Main.Settings.MercPilotBondsmanMax + InfoClass.MercPilotInfo.BondsmanMax, InfoClass.MercPilotInfo.BondsrefCount) / 2);
			int chance = simGame.NetworkRandom.Int(0, 100);

			Log.Info($"Checking for Clan {clanShortName} {clanName.Name} with BondChance {bondChance}, refChance {refChance} and chance {chance}");

			if (bondChance >= chance)
			{
				PilotDef pilotDef = GenerateBondsman(simGame, clanName);
				
				if (pilotDef != null)
                {
					string message = $"We've been approached on planet, by a Clan {clanShortName} Warrior, who begrudgingly offered to join us.  Apparently they can no longer return to their clan after being defeated by us and they will consider joining us as our Bondsman, instead of Bondsref. Given their penchant for honourable eating of gun blasts, I take Bondref to mean something along these lines.";
					string desc = "Bondsman";
					PauseNotification.Show(desc, message, simGame.GetCrewPortrait(SimGameCrew.Crew_Darius), string.Empty, false);

					Log.Info($"Name: {pilotDef.Description.FullName()} Tags: {pilotDef.PilotTags.ToString()} Abilities: {pilotDef.AbilityDefs.ToString()} Icon: {pilotDef.Description.Icon}");
					Log.Info($"Details: {pilotDef.Description.Details}");
					Log.Info($"Stats: Gunnery: {pilotDef.BaseGunnery} | Piloting: {pilotDef.BasePiloting} | Guts: {pilotDef.BaseGuts} | Tactics: {pilotDef.BaseTactics} :: {pilotDef.Level} :: {pilotDef.GetStats().ToString()}");
					// Offer Bondsman
					Action cleanup = delegate () {
						result = false;
						NoBondsman(simGame, clanName, pilotDef);
					};
					
					message = $"Name: {pilotDef.Description.FullName()}\r\nStats: Gunnery: {pilotDef.BaseGunnery} | Piloting: {pilotDef.BasePiloting} | Guts: {pilotDef.BaseGuts} | Tactics: {pilotDef.BaseTactics}\r\n\r\n{pilotDef.Description.GetLocalizedDetails()}\r\n\r\nClan Warriors are usually highly skilled, and this one comes with no sign on fee, do you want to add this pilot?\n";
					string primaryButtonText = "Add Bondsman";
					PauseNotification.Show(desc, message, pilotDef.GetPortraitSpriteThumb(pilotDef.DataManager), string.Empty, true, delegate {
						AddBondsman(simGame, clanName, pilotDef);
						result = true;
					}, primaryButtonText, cleanup, "Do not Accept");
				}				
			}
			else if (refChance >= chance)
            {
				NoBondsman(simGame,clanName);

				result = false;
			}
			return result;
        }

		public static void AddBondsman(SimGameState simGame, FactionValue clanName, PilotDef pilotDef)
        {

			string message;

			if (simGame.PilotRoster.Count >= simGame.GetMaxMechWarriors())
			{
				message = $"Your barracks is full and you have no space for {pilotDef.Description.FullName()}.";
				PauseNotification.Show("Bondcord", message, pilotDef.GetPortraitSprite(pilotDef.DataManager), string.Empty, true);
				NoBondsman(simGame, clanName, pilotDef);
			}
			else
			{
				message = $"You tie the bondcord to {pilotDef.Description.FullName()} and while their spirits are low, they appear determined.  Pilot added to your barracks with instructions for your other MechWarriors to keep an eye on them.";
				PauseNotification.Show("Bondcord", message, pilotDef.GetPortraitSprite(pilotDef.DataManager), string.Empty, true);
				simGame.AddPilotToRoster(pilotDef);

				if (InfoClass.MercPilotInfo.PortraitUsed.ContainsKey(pilotDef.Description.Gender))
				{
					InfoClass.MercPilotInfo.PortraitUsed[pilotDef.Description.Gender].Add(PilotPortraitNo);
				}
				else
				{
					List<int> temp = new List<int> { PilotPortraitNo };
					InfoClass.MercPilotInfo.PortraitUsed.Add(pilotDef.Description.Gender, temp);
				}

				TagSet tagSet = new TagSet();
				Traverse.Create(tagSet).Field("items").SetValue(new string[]
				{
					"pilot_morale_low"
				});
				Traverse.Create(tagSet).Field("tagSetSourceFile").SetValue("Tags/PilotTags");
				Traverse.Create(tagSet).Method("UpdateHashCode", Array.Empty<object>()).GetValue(); 
				
				Pilot pilot = simGame.GetPilot(pilotDef.Description.Id);
                TemporarySimGameResult temporarySimGameResult = new TemporarySimGameResult();
                temporarySimGameResult.ResultDuration = simGame.NetworkRandom.Int(45, 180);
                temporarySimGameResult.Scope = EventScope.MechWarrior;
                temporarySimGameResult.TemporaryResult = true;
				temporarySimGameResult.AddedTags = tagSet;
				Traverse.Create(temporarySimGameResult).Field("targetPilot").SetValue(pilot);
				simGame.TemporaryResultTracker.Add(temporarySimGameResult);
            }
		}

		public static bool GetPortraitNo(SimGameState simGame, Gender gender)
        {
			bool validNumber = false;

			List<int> listNo = new List<int>();
			List<int> takenNo = InfoClass.MercPilotInfo.PortraitUsed[gender];

			if (takenNo.Count >= 45)
				takenNo.Clear();

			for (int i = 1; i <= 45; i++)
            {
				if (!takenNo.Contains(i))
					listNo.Add(i);
            }

			if (listNo.Count >= 1)
				PilotPortraitNo = listNo.GetRandomElement();
			else
				PilotPortraitNo = simGame.NetworkRandom.Int(1, 45);

			if (PilotPortraitNo > 0)
				validNumber = true;

			return validNumber;
        }

		public static PilotDef GenerateBondsman(SimGameState simGame, FactionValue clanName)
        {
			PilotDef pilotDef = new PilotDef();
			//List<object> objList = new List<object>();

			string femLastName;
			string masLastName;

			switch (clanName.Name)
			{
				case "JadeFalcon":
					masLastName = "Falcon";
					femLastName = "Jade";
					break;
				case "Wolf":
					masLastName = "Wolf";
					femLastName = "Wolf";
					break;
				case "GhostBear":
					masLastName = "Bear";
					femLastName = "Ghost";
					break;
				case "SmokeJaguar":
					masLastName = "Smoke";
					femLastName = "Jaguar";
					break;
				case "SteelViper":
					masLastName = "Steel";
					femLastName = "Viper";
					break;
				case "NovaCat":
					masLastName = "Nova";
					femLastName = "Cat";
					break;
				case "DiamondShark":
					masLastName = "Shark";
					femLastName = "Diamond";
					break;
				default:
					masLastName = "Clan";
					femLastName = "Clan";
					break;
			}

			//objList.Add(simGame.CurSystem.Def.GetDifficulty(simGame.SimGameMode));

			pilotDef = GetMethodToInvoke(typeof(PilotGenerator), simGame.PilotGenerator, "GenerateRandomPilot", new object[] { simGame.CurSystem.Def.GetDifficulty(simGame.SimGameMode) + 4 }) as PilotDef;
			//pilotDef = pilotGenerator.GenerateRandomPilot(systemDifficulty);

			// Alter PilotDef to Clan Pilot
			string clanShortName = clanName.FactionDef.ShortName;
			if (clanShortName.Contains("Clan "))
				clanShortName.Replace("Clan ", "");

			if (pilotDef.Description.Gender != Gender.Female || pilotDef.Description.Gender != Gender.Male)
            {
				if (pilotDef.Voice.Contains("f_"))
					Traverse.Create(pilotDef.Description).Property("Gender").SetValue(Gender.Female);
				else
					Traverse.Create(pilotDef.Description).Property("Gender").SetValue(Gender.Male);
			}

			if (pilotDef.Description.Gender == Gender.Female)
			{
				if (GetPortraitNo(simGame, pilotDef.Description.Gender))
				{
					pilotDef.Description.SetLastName(femLastName);
					pilotDef.Description.SetIcon("Clan_Generic_F" + PilotPortraitNo.ToString("00"));
					pilotDef.PortraitSettings = null;
				}
			}
			else
			{
				if (GetPortraitNo(simGame, pilotDef.Description.Gender))
				{
					pilotDef.Description.SetLastName(masLastName);
					pilotDef.Description.SetIcon("Clan_Generic_M" + PilotPortraitNo.ToString("00"));
					pilotDef.PortraitSettings = null;
				}
			}
			
			Traverse.Create(pilotDef.Description).Property("factionID").SetValue(clanName.FactionDef.ID);
			Traverse.Create(pilotDef.Description).Property("FactionValue").SetValue(clanName);

			pilotDef.abilityDefNames.Add("AbilityDefClanAccuracy");
			pilotDef.abilityDefNames.Add("AbilityDefClanMelee");

			string details = $"Clan {clanShortName} Bondsman";
			details += $"\r\nDefeated by {simGame.CompanyName} in a battle on {simGame.CurSystem.Name}, this Mech Warrior became your Bondsman and will serve your company until such time as you release them from their Bond and return their honour to them.";

			if (pilotDef.ExperienceSpent >= 8000)
            {

				if (pilotDef.ExperienceSpent >= 64000)
				{
					pilotDef.abilityDefNames.Add("TraitDefGalaxyCommander");
					details += $"\r\n\r\n<b>Galaxy Commander:</b> This is a top echelon Clan {clanShortName} warrior, with increased personal evasion and slight damage reduction across the whole lance, owing to experience commanding large groups of clan warriors and being in the elite circles of clan warriors.";
				}
				else if (pilotDef.ExperienceSpent >= 32000)
				{
					pilotDef.abilityDefNames.Add("TraitDefStarColonel");
					details += $"\r\n\r\n<b>Star Colonel:</b> This is a high ranked Clan {clanShortName} warrior, with increased evasion and damage reduction, owing to being in the elite circles of clan warriors.";
				}
				else if(pilotDef.ExperienceSpent >= 16000)
				{
					pilotDef.abilityDefNames.Add("TraitDefStarCaptain");
					details += $"\r\n\r\n<b>Star Captain:</b> This is a mid ranked Clan {clanShortName} warrior, with increased damage reduction, owing to extensive clan training and intense combat.";
				}
				else
				{
					pilotDef.abilityDefNames.Add("TraitDefStarCommander");
					details += $"\r\n\r\n<b>Star Commander:</b> This is a ranked Clan {clanShortName} warrior, with slightly increased damage reduction, owing to clan training and combat.";
				}
			}
			else
            {
				details += $"\r\n\r\n<b>Mechwarrior:</b> This is a non-ranked Clan {clanShortName} warrior of moderate skill.";
			}

			details += $"\r\n\r\n<b>Clan Accuracy:</b> All clanners have greater accuracy when compared to an Inner Sphere pilot of the same Gunnery skill.";
			details += $"\r\n\r\n<b>Clan Melee:</b> All clan warriors suffer a penalty in melee combat, based on clan cultural emphasis on honourable combat.";

			Traverse.Create(pilotDef.Description).Property("Details").SetValue(details);
			
			Traverse.Create(pilotDef).Property("IsRonin").SetValue(true);
			
			CleanUpTags(simGame, pilotDef);

			return pilotDef;
		}

		public static void CleanUpTags(SimGameState simGame, PilotDef pilotDef)
		{

			pilotDef.PilotTags.Remove("pilot_morale_high");

            if (!pilotDef.PilotTags.Contains("pilot_morale_low"))
                pilotDef.PilotTags.Add("pilot_morale_low");

            pilotDef.PilotTags.Remove("pilot_innersphere");
			pilotDef.PilotTags.Remove("pilot_davion");
			pilotDef.PilotTags.Remove("pilot_marik");
			pilotDef.PilotTags.Remove("pilot_liao");
			pilotDef.PilotTags.Remove("pilot_kurita");
			pilotDef.PilotTags.Remove("pilot_periphery");
			pilotDef.PilotTags.Remove("pilot_taurian");
			pilotDef.PilotTags.Remove("pilot_magistracy");
			pilotDef.PilotTags.Remove("pilot_aurigan");
			pilotDef.PilotTags.Remove("pilot_steiner");

			if (pilotDef.PilotTags.Remove("pilot_comstar"))
			{
				if (!pilotDef.PilotTags.Contains("pilot_lostech"))
					pilotDef.PilotTags.Add("pilot_lostech");
				else if (!pilotDef.PilotTags.Contains("pilot_lucky"))
					pilotDef.PilotTags.Add("pilot_lucky");
			}

			if (pilotDef.PilotTags.Remove("pilot_gladiator"))
			{
				if (!pilotDef.PilotTags.Contains("pilot_brave"))
					pilotDef.PilotTags.Add("pilot_brave");
				else if (!pilotDef.PilotTags.Contains("pilot_assassin"))
					pilotDef.PilotTags.Add("pilot_assassin");
			}

			if (pilotDef.PilotTags.Remove("pilot_reckless"))
			{
				if (!pilotDef.PilotTags.Contains("pilot_naive"))
					pilotDef.PilotTags.Add("pilot_naive");
				else if (!pilotDef.PilotTags.Contains("pilot_disgraced"))
					pilotDef.PilotTags.Add("pilot_disgraced");
			}

			if (pilotDef.PilotTags.Remove("pilot_wealthy"))
			{
				if (!pilotDef.PilotTags.Contains("pilot_athletic"))
					pilotDef.PilotTags.Add("pilot_athletic");
				else if (!pilotDef.PilotTags.Contains("pilot_spacer"))
					pilotDef.PilotTags.Add("pilot_spacer");
			}

			if (pilotDef.PilotTags.Remove("pilot_noble"))
			{
				if (!pilotDef.PilotTags.Contains("pilot_rebellious"))
					pilotDef.PilotTags.Add("pilot_rebellious");
				else if (!pilotDef.PilotTags.Contains("pilot_honest"))
					pilotDef.PilotTags.Add("pilot_honest");
			}
		
			if (!pilotDef.PilotTags.Contains("pilot_mechwarrior"))
				pilotDef.PilotTags.Add("pilot_mechwarrior");

			if (!pilotDef.PilotTags.Contains("pilot_military"))
				pilotDef.PilotTags.Add("pilot_military");

			pilotDef.PilotTags.Add("BONDSMAN");
			pilotDef.PilotTags.Add("VXI");
			pilotDef.PilotTags.Add("pilot_clan");
		}

		//private PilotDef GenerateRandomPilot(int systemDifficulty)
		//{
		//	int num = this.Sim.Constants.Pilot.MinimumPilotAge + this.Sim.NetworkRandom.Int(1, this.Sim.Constants.Pilot.StartingAgeRange + 1);
		//	string firstName;
		//	string lastName;
		//	Gender gender;
		//	this.GetNameAndGender(out firstName, out lastName, out gender);
		//	string text = null;
		//	List<string> allCallsigns = this.pilotNameGenerator.GetAllCallsigns();
		//	List<string> list = new List<string>();
		//	for (int i = allCallsigns.Count - 1; i >= 0; i--)
		//	{
		//		if (this.Sim.pilotGenCallsignDiscardPile.Contains(allCallsigns[i]))
		//		{
		//			list.Add(allCallsigns[i]);
		//			allCallsigns.RemoveAt(i);
		//		}
		//	}
		//	if ((float)list.Count >= (float)allCallsigns.Count * this.Sim.Constants.Story.DiscardPileToActiveRatio)
		//	{
		//		allCallsigns.AddRange(list);
		//		this.Sim.pilotGenCallsignDiscardPile.Clear();
		//	}
		//	allCallsigns.Shuffle<string>();
		//	text = allCallsigns[0];
		//	LifepathNodeDef lifepathNodeDef = this.GetStartingNode(systemDifficulty);
		//	List<LifepathNodeDef> list2 = new List<LifepathNodeDef>();
		//	PilotDef pilotDef = new PilotDef(new HumanDescriptionDef(), 1, 1, 1, 1, 1, 1, false, 1, "", new List<string>(), AIPersonality.Undefined, 0, 0, 0);
		//	TagSet tagSet = new TagSet();
		//	List<EndingPair> list3 = new List<EndingPair>();
		//	List<SimGameEventResultSet> list4 = new List<SimGameEventResultSet>();
		//	StatCollection stats;
		//	while (lifepathNodeDef != null)
		//	{
		//		list2.Add(lifepathNodeDef);
		//		num += lifepathNodeDef.Duration;
		//		SimGameEventResultSet resultSet = this.Sim.GetResultSet(lifepathNodeDef.ResultSets);
		//		foreach (SimGameEventResult simGameEventResult in resultSet.Results)
		//		{
		//			if (simGameEventResult.AddedTags != null)
		//			{
		//				tagSet.AddRange(simGameEventResult.AddedTags);
		//			}
		//			if (simGameEventResult.RemovedTags != null)
		//			{
		//				tagSet.RemoveRange(simGameEventResult.RemovedTags);
		//			}
		//			if (simGameEventResult.Stats != null)
		//			{
		//				for (int k = 0; k < simGameEventResult.Stats.Length; k++)
		//				{
		//					SimGameStat simGameStat = simGameEventResult.Stats[k];
		//					float f = simGameStat.ToSingle();
		//					SkillType skillType = this.SkillStringToType(simGameStat.name);
		//					if (skillType != SkillType.NotSet)
		//					{
		//						int num2 = Mathf.RoundToInt(f);
		//						int baseSkill = pilotDef.GetBaseSkill(skillType);
		//						pilotDef.AddBaseSkill(skillType, num2);
		//						for (int l = baseSkill + 1; l <= baseSkill + num2; l++)
		//						{
		//							this.SetPilotAbilities(pilotDef, simGameStat.name, l);
		//						}
		//					}
		//				}
		//			}
		//		}
		//		list4.Add(resultSet);
		//		List<LifepathNodeEnding> list5 = new List<LifepathNodeEnding>();
		//		stats = pilotDef.GetStats();
		//		for (int m = 0; m < lifepathNodeDef.Endings.Length; m++)
		//		{
		//			RequirementDef requirements = lifepathNodeDef.Endings[m].Requirements;
		//			if (requirements == null || SimGameState.MeetsRequirements(requirements.RequirementTags, requirements.ExclusionTags, requirements.RequirementComparisons, tagSet, stats, null))
		//			{
		//				list5.Add(lifepathNodeDef.Endings[m]);
		//			}
		//		}
		//		LifepathNodeDef lifepathNodeDef2 = null;
		//		if (list5.Count > 0)
		//		{
		//			List<int> list6 = new List<int>();
		//			for (int n = 0; n < list5.Count; n++)
		//			{
		//				list6.Add(list5[n].Weight);
		//			}
		//			int weightedResult = SimGameState.GetWeightedResult(list6, this.Sim.NetworkRandom.Float(0f, 1f));
		//			LifepathNodeEnding lifepathNodeEnding = list5[weightedResult];
		//			EndingPair item = default(EndingPair);
		//			item.ending = lifepathNodeEnding;
		//			TagSet nextNodeTags = lifepathNodeEnding.NextNodeTags;
		//			float num3 = (float)num * this.Sim.Constants.Pilot.AgeEndingModifier;
		//			bool flag = false;
		//			if ((float)this.Sim.NetworkRandom.Int(0, 100) < num3 && !lifepathNodeDef.ForceNode)
		//			{
		//				flag = true;
		//			}
		//			if (!lifepathNodeEnding.EndNode && !flag)
		//			{
		//				List<LifepathNodeDef> list7 = new List<LifepathNodeDef>();
		//				for (int num4 = 0; num4 < this.lifepaths.Count; num4++)
		//				{
		//					if (nextNodeTags == null || this.lifepaths[num4].NodeTags.ContainsAll(nextNodeTags))
		//					{
		//						RequirementDef requirements2 = this.lifepaths[num4].Requirements;
		//						if (requirements2 != null)
		//						{
		//							TagSet requirementTags = requirements2.RequirementTags;
		//							TagSet exclusionTags = requirements2.ExclusionTags;
		//							List<ComparisonDef> requirementComparisons = requirements2.RequirementComparisons;
		//							if (!SimGameState.MeetsRequirements(requirementTags, exclusionTags, requirementComparisons, tagSet, stats, null))
		//							{
		//								goto IL_443;
		//							}
		//						}
		//						list7.Add(this.lifepaths[num4]);
		//					}
		//				IL_443:;
		//				}
		//				if (list7.Count > 0)
		//				{
		//					lifepathNodeDef2 = list7[this.Sim.NetworkRandom.Int(0, list7.Count)];
		//					item.nextNode = lifepathNodeDef2;
		//				}
		//				else
		//				{
		//					Debug.LogWarning("Unable to find new node from ending: " + lifepathNodeEnding.Description.Id + " | " + lifepathNodeDef.Description.Id);
		//				}
		//			}
		//			list3.Add(item);
		//		}
		//		lifepathNodeDef = lifepathNodeDef2;
		//	}
		//	this.sb = new StringBuilder();
		//	foreach (SimGameEventResultSet simGameEventResultSet in list4)
		//	{
		//		if (simGameEventResultSet.Description.Name != "")
		//		{
		//			this.sb.Append(string.Format("<b>{1}:</b> {0}\n\n", simGameEventResultSet.Description.Details, simGameEventResultSet.Description.Name));
		//		}
		//	}
		//	string id = this.GenerateID();
		//	Gender gender2 = gender;
		//	if (gender2 == Gender.NonBinary)
		//	{
		//		if (this.Sim.NetworkRandom.Float(0f, 1f) < 0.5f)
		//		{
		//			gender2 = Gender.Male;
		//		}
		//		else
		//		{
		//			gender2 = Gender.Female;
		//		}
		//	}
		//	string voice = this.GenerateVoiceForGender(gender2);
		//	HumanDescriptionDef description = new HumanDescriptionDef(id, text, firstName, lastName, text, gender, FactionEnumeration.GetNoFactionValue(), num, this.sb.ToString(), null);
		//	stats = pilotDef.GetStats();
		//	int spentXPPilot = this.GetSpentXPPilot(stats);
		//	List<string> list8 = new List<string>();
		//	if (this.Sim.Commander != null && this.Sim.Commander.pilotDef.PortraitSettings != null)
		//	{
		//		list8.Add(this.Sim.Commander.pilotDef.PortraitSettings.Description.Id);
		//	}
		//	foreach (Pilot pilot in this.Sim.PilotRoster)
		//	{
		//		if (pilot.pilotDef.PortraitSettings != null)
		//		{
		//			list8.Add(pilot.pilotDef.PortraitSettings.Description.Id);
		//		}
		//	}
		//	PilotDef pilotDef2 = new PilotDef(description, pilotDef.BaseGunnery, pilotDef.BasePiloting, pilotDef.BaseGuts, pilotDef.BaseTactics, 0, this.Sim.CombatConstants.PilotingConstants.DefaultMaxInjuries, false, 0, voice, pilotDef.abilityDefNames, AIPersonality.Undefined, 0, tagSet, spentXPPilot, 0)
		//	{
		//		DataManager = this.Sim.DataManager,
		//		PortraitSettings = this.GetPortraitForGenderAndAge(gender2, num, list8)
		//	};
		//	pilotDef2.ForceRefreshAbilityDefs();
		//	this.Sim.pilotGenCallsignDiscardPile.Add(pilotDef2.Description.Callsign);
		//	return pilotDef2;
		//}


		[HarmonyPatch(typeof(SimGameState), "SetupRoninTooltip")]
        public static class SimGameState_SetupRoninTooltip_Patch
        {
            public static void Postfix(SimGameState __instance, ref HBSTooltip RoninTooltip, Pilot pilot)
            {
                try
                {
					if (pilot.pilotDef.PilotTags.Contains("pilot_recruit") && pilot.pilotDef.PilotTags.Contains("VXI"))
					{
						BaseDescriptionDef def = UnityGameInstance.BattleTechGame.DataManager.BaseDescriptionDefs.Get("UnitMechWarriorRecruit");
						RoninTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(def));
					}
					else if(pilot.pilotDef.PilotTags.Contains("BONDSMAN") && pilot.pilotDef.PilotTags.Contains("VXI"))
					{
						BaseDescriptionDef def = UnityGameInstance.BattleTechGame.DataManager.BaseDescriptionDefs.Get("UnitMechWarriorBondsman");
						RoninTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(def));
					}
					else if (pilot.pilotDef.PilotTags.Contains("VXI"))
                    {
                        BaseDescriptionDef def = UnityGameInstance.BattleTechGame.DataManager.BaseDescriptionDefs.Get("UnitMechWarriorVXI");
                        RoninTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(def));
                    } 
				}
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

		[HarmonyPatch(typeof(PilotGenerator), "GetStartingNode")]
		public static class PilotGenerator_GetStartingNode_Patch
		{
			public static void Postfix(PilotGenerator __instance, ref LifepathNodeDef __result, int systemDifficulty)
			{
				try
				{
					if (Main.Settings.IncLifeNodes)
					{
						SimGameState sim = GetInstanceField(typeof(PilotGenerator), __instance, "Sim") as SimGameState;

						List<LifepathNodeDef> lifepaths = GetInstanceField(typeof(PilotGenerator), __instance, "lifepaths") as List<LifepathNodeDef>;

						string startType = "_start_";

						if (sim.Constants.Pilot.AdvancedPilotBaseChance + (float)systemDifficulty * sim.Constants.Pilot.AdvancedPilotDifficultyStep > sim.NetworkRandom.Float(0f, 1f))
						{
							startType = "_astart_";
						}

						switch (sim.CurSystem.OwnerValue.Name)
						{
							case "Davion":
								__result = lifepaths.Find(x => x.Description.Id.Equals($"lifenode{startType}hDavion"));
								break;
							case "Liao":
								__result = lifepaths.Find(x => x.Description.Id.Equals($"lifenode{startType}hLiao"));
								break;
							case "Kurita":
								__result = lifepaths.Find(x => x.Description.Id.Equals($"lifenode{startType}hKurita"));
								break;
							case "Marik":
								__result = lifepaths.Find(x => x.Description.Id.Equals($"lifenode{startType}hMarik"));
								break;
							case "Steiner":
								__result = lifepaths.Find(x => x.Description.Id.Equals($"lifenode{startType}hSteiner"));
								break;
							case "Rasalhague":
								__result = lifepaths.Find(x => x.Description.Id.Equals($"lifenode{startType}nRasalhague"));
								break;
							case "TaurianConcordat":
								__result = lifepaths.Find(x => x.Description.Id.Equals($"lifenode{startType}pTaurianConcordat"));
								break;
							case "MagistracyOfCanopus":
								__result = lifepaths.Find(x => x.Description.Id.Equals($"lifenode{startType}pMagistracyOfCanopus"));
								break;
							case "AuriganRestoration":
								__result = lifepaths.Find(x => x.Description.Id.Equals($"lifenode{startType}pAuriganRestoration"));
								break;
							case "Outworld":
								__result = lifepaths.Find(x => x.Description.Id.Equals($"lifenode{startType}pOutworld"));
								break;
							default:
								__result = lifepaths.Find(x => x.Description.Id.Equals($"lifenode{startType}pIndependent"));
								break;
						}
					}
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		[HarmonyPatch(typeof(PilotGenerator), "GenerateRandomPilot")]
		public static class PilotGenerator_GenerateRandomPilot_Patch
		{
			public static void Postfix(PilotGenerator __instance, ref PilotDef __result)
			{
				try
				{
					if (Main.Settings.IncLifeNodes)
					{
						// Can add pilot info here
					}
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VXIContractManagement
{
    class GenerateContractMaps
    {

		private bool GetValidFaction(StarSystem system, Dictionary<string, WeightedList<SimGameState.ContractParticipants>> targetList, List<RequirementDef> defList, out SimGameState.ChosenContractParticipants chosenContractParticipants)
		{
			chosenContractParticipants = new SimGameState.ChosenContractParticipants();
			HashSet<string> sourceFactions = (from t in targetList.Values.SelectMany((WeightedList<SimGameState.ContractParticipants> t) => t)
											  select t.Target.Name).ToHashSet<string>();
			HashSet<string> sourceFactions2 = targetList.Keys.ToHashSet<string>();
			HashSet<string> sourceFactions3 = targetList.Values.SelectMany((WeightedList<SimGameState.ContractParticipants> t) => t).SelectMany((SimGameState.ContractParticipants t) => from f in t.NeutralToAll
																																														 select f.Name).ToHashSet<string>();
			HashSet<string> sourceFactions4 = targetList.Values.SelectMany((WeightedList<SimGameState.ContractParticipants> t) => t).SelectMany((SimGameState.ContractParticipants t) => from f in t.HostileToAll
																																														 select f.Name).ToHashSet<string>();
			IEnumerable<ComparisonDef> comparisons = defList.SelectMany((RequirementDef r) => r.RequirementComparisons);
			List<ComparisonDef> comparisons2;
			List<ComparisonDef> comparisons3;
			List<ComparisonDef> comparisons4;
			List<ComparisonDef> comparisons5;
			this.FilterEmployerTargetComparisons(comparisons, out comparisons2, out comparisons3, out comparisons4, out comparisons5);
			List<string> potentialEmployers = this.GetPotentialFactions(system, sourceFactions2, comparisons2);
			if (!potentialEmployers.Any<string>())
			{
				return false;
			}
			List<string> potentialTargets = this.GetPotentialFactions(system, sourceFactions, comparisons3);
			if (!potentialTargets.Any<string>())
			{
				return false;
			}
			List<string> potentialNeutrals = this.GetPotentialFactions(system, sourceFactions3, comparisons4);
			List<string> potentialHostiles = this.GetPotentialFactions(system, sourceFactions4, comparisons5);
			Func<SimGameState.ContractParticipants, bool> <> 9__16;
			var source = from employerTargets in (from employerTargets in targetList
												  where potentialEmployers.Contains(employerTargets.Key)
												  select employerTargets).Select(delegate (KeyValuePair<string, WeightedList<SimGameState.ContractParticipants>> employerTargets)
												  {
													  string key = employerTargets.Key;
													  IEnumerable<SimGameState.ContractParticipants> value = employerTargets.Value;
													  Func<SimGameState.ContractParticipants, bool> predicate;
													  if ((predicate = <> 9__16) == null)
													  {
														  predicate = (<> 9__16 = ((SimGameState.ContractParticipants sourceTargets) => potentialTargets.Contains(sourceTargets.Target.Name)));
													  }
													  return new
													  {
														  Employer = key,
														  Participants = value.Where(predicate).ToWeightedList(WeightedListType.PureRandom)
													  };
												  })
						 where employerTargets.Participants.Any<SimGameState.ContractParticipants>()
						 select employerTargets;
			if (!source.Any())
			{
				return false;
			}
			int index = this.NetworkRandom.Int(0, source.Count());
			var<> f__AnonymousType = source.ElementAt(index);
			chosenContractParticipants.Employer = FactionEnumeration.GetFactionByName(<> f__AnonymousType.Employer);
			SimGameState.ContractParticipants next = <> f__AnonymousType.Participants.GetNext(true);
			chosenContractParticipants.Target = next.Target;
			FactionValue currentEmployerAlly = next.EmployerAllies.GetNext(true);
			FactionValue currentTargetAlly = next.TargetAllies.GetNext(true);
			potentialHostiles.RemoveAll((string f) => f == currentEmployerAlly.Name || f == currentTargetAlly.Name);
			potentialNeutrals.RemoveAll((string f) => f == currentEmployerAlly.Name || f == currentTargetAlly.Name);
			chosenContractParticipants.EmployersAlly = currentEmployerAlly;
			chosenContractParticipants.TargetsAlly = currentTargetAlly;
			WeightedList<FactionValue> weightedList = (from f in next.HostileToAll
													   where potentialHostiles.Contains(f.Name)
													   select f).ToWeightedList(WeightedListType.PureRandom);
			if (!weightedList.Any<FactionValue>())
			{
				return false;
			}
			FactionValue currentHostileToAll = weightedList.GetNext(true);
			WeightedList<FactionValue> weightedList2 = (from f in next.NeutralToAll
														where currentHostileToAll.Equals(f) && potentialNeutrals.Contains(f.Name)
														select f).ToWeightedList(WeightedListType.PureRandom);
			if (weightedList2.Any<FactionValue>())
			{
				chosenContractParticipants.NeutralToAll = weightedList2.GetNext(true);
			}
			else
			{
				chosenContractParticipants.NeutralToAll = FactionEnumeration.GetHostileMercenariesFactionValue();
			}
			chosenContractParticipants.HostileToAll = currentHostileToAll;
			return true;
		}
	}
}

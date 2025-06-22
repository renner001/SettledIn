using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.SettledIn
{
    class CompProperties_AssignableToPawn_ProductionBuilding : CompProperties_AssignableToPawn
	{

		public CompProperties_AssignableToPawn_ProductionBuilding()
		{
			compClass = typeof(CompAssignableToPawn_ProductionBuilding);
		}
	}
}

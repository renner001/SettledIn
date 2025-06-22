using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DanielRenner.SettledIn
{
    class GameComponent_SettlementScoreManager : GameComponent
    {
        public GameComponent_SettlementScoreManager(Game game)
        {
            Log.Debug("GameComponent_SettlementScoreManager created");
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            UpdateCache();
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            UpdateCache();
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
        }

        public void UpdateCache()
        {
            Log.Debug("GameComponent_SettlementScoreManager.UpdateCache(): Regenerating complete cache..");

            var allHomeMaps = Find.Maps.Where(map => { return map.IsPlayerHome; }).ToArray();
            Log.Debug("GameComponent_SettlementScoreManager.UpdateCache(): " + allHomeMaps.Length + " home maps found " + String.Join(", ", allHomeMaps.Select(homeMap => { return homeMap.ToString(); })));
            foreach (Map homeMap in allHomeMaps)
            {
                var mapComponentSettlement = homeMap.GetComponent<MapComponent_SettlementResources>();
                mapComponentSettlement.UpdateCache();
            }
            Log.Debug("GameComponent_SettlementScoreManager.UpdateCache(): Cache complete.");
        }

    }
}

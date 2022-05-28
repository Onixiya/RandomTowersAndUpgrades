using MelonLoader;
using System;
using HarmonyLib;
using Assets.Scripts.Simulation.Towers;
using Assets.Scripts.Models;
using Assets.Scripts.Unity;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.ModOptions;
using Assets.Scripts.Models.Towers;
using BTD_Mod_Helper.Extensions;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Simulation;
using Assets.Scripts.Unity.UI_New.InGame;
using System.Linq;
using System.Text.RegularExpressions;
[assembly: MelonInfo(typeof(RandomTowersAndUpgrades.ModMain),"Random Towers and Upgrades","1.2.1","Silentstorm")]
[assembly: MelonGame("Ninja Kiwi","BloonsTD6")]
namespace RandomTowersAndUpgrades{
    public class ModMain:BloonsTD6Mod{
        public override string GithubReleaseURL=>"https://api.github.com/repos/Onixiya/RandomTowersAndUpgrades/releases";
        public static ModSettingBool RandomTowerOnPlace=new ModSettingBool(false);
        public static ModSettingBool RandomUpgrades=new ModSettingBool(false);
        public static ModSettingBool RandomizeInRound=new ModSettingBool(true);
        public static ModSettingBool RandomizeOnRoundStart=new ModSettingBool(false);
        public static ModSettingInt RandomizeTimer=new ModSettingInt(20);
        public static ModSettingBool IgnoreBlacklist=new ModSettingBool(false);
        public static ModSettingString RandomKey=new ModSettingString("305");
        public static bool RoundOn=false;
        public static float Timer=0;
        private static MelonLogger.Instance mllog=new MelonLogger.Instance("Random Towers and Upgrades");
        public static string PreviousTower;
        public static string[]BlacklistedTowerNames=new string[]{
            "Sentry","Spectre","Plane","UAV","UCAV","AvatarMini","Totem","Phoenix","Drone","BallOfLight","HeliPilot-<012>5","HeliPilot-<012>4"
        };
        public static void Log(object thingtolog,string type="msg"){
            switch(type){
                case"msg":
                    mllog.Msg(thingtolog);
                    break;
                case"warn":
                    mllog.Warning(thingtolog);
                    break;
                 case"error":
                    mllog.Error(thingtolog);
                    break;
            }
        }
        public static TowerModel GetRandomTowerModel(TowerModel towermodel){
            TowerModel[]towermodels=Game.instance.model.towers;
            TowerModel tower=towermodels[new Random().Next(0,towermodels.Length+1)];
            if(IgnoreBlacklist==false){
                foreach(string name in BlacklistedTowerNames){
                    if(Regex.IsMatch(towermodel.name,name)){
                        return towermodel;
                    }
                    while(Regex.IsMatch(tower.name,name)){
                        tower=towermodels[new Random().Next(0,towermodels.Length+1)];
                    }
                }
                while(tower.HasBehavior<TowerExpireModel>()||tower.HasBehavior<HeroModel>()||tower.name==PreviousTower){
                    tower=towermodels[new Random(tower.behaviors.Count).Next(0,towermodels.Length+1)];
                }
                PreviousTower=tower.name;
                return tower;
            }else{
                tower=towermodels[new Random().Next(0,towermodels.Length+1)];
                PreviousTower=tower.name;
                return tower;
            }
        }
        public override void OnUpdate(){
            if(InGame.Bridge!=null){
                if(UnityEngine.Input.GetKeyDown((UnityEngine.KeyCode)int.Parse((string)RandomKey.GetValue()))){
                    var towers=InGame.Bridge.simulation.towerManager.GetTowers();
                    if(towers.Count()!=0){
                        towers.ForEach(tower=>{
                            tower.UpdateRootModel(GetRandomTowerModel(tower.towerModel));
                        });
                    }
                }
                if(RoundOn==true){
                    if(RandomizeInRound==true){
                        Timer+=UnityEngine.Time.deltaTime;
                        if(Timer>RandomizeTimer){
                            var towers=InGame.Bridge.simulation.towerManager.GetTowers();
                            towers.ForEach(tower=>{
                                tower.UpdateRootModel(GetRandomTowerModel(tower.towerModel));
                            });
                            Timer=0;
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(TowerManager),"UpgradeTower")]
        public class TowerManagerUpgradeTower_Patch{
            [HarmonyPrefix]
            public static void Prefix(ref TowerModel def){
                if(RandomUpgrades==true){
                    def=GetRandomTowerModel(def);
                }
            }
        }
        [HarmonyPatch(typeof(Simulation),"RoundStart")]
        public class SimulationRoundStart_Patch{
            [HarmonyPostfix]
            public static void Postfix(ref Simulation __instance){
                RoundOn=true;
                if(RandomizeOnRoundStart==true){
                    var towers=InGame.Bridge.simulation.towerManager.GetTowers();
                    if(towers.Count()!=0){
                        towers.ForEach(tower=>{
                            tower.UpdateRootModel(GetRandomTowerModel(tower.towerModel));
                        });
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Simulation),"RoundEnd")]
        public class SimulationRoundEnd_Patch{
            [HarmonyPostfix]
            public static void Postfix(){
                RoundOn=false;
            }
        }
        [HarmonyPatch(typeof(Tower),"Initialise")]
        public class TowerInitialise_Patch{
            [HarmonyPrefix]
            public static bool Prefix(ref Model modelToUse){
                if(RandomTowerOnPlace==true){
                    modelToUse=GetRandomTowerModel(modelToUse.Cast<TowerModel>());
                }
                return true;
            }
        }
    }
}
using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Infectionbomb
{
    public class Infectionbomb : Mod
    {
        private static Infectionbomb? _instance;

        private GameObject radiance;
        public static readonly string EnemiesPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Enemies";
        public string[] currentEnemies;

        internal static Infectionbomb Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"{nameof(Infectionbomb)} was never initialized");
                }
                return _instance;
            }
        }

        private int rageHP = 2600;
        private int stunHP = 2000;

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public Infectionbomb() : base()
        {
            _instance = this;
        }
        public override List<(string, string)> GetPreloadNames()
        {
            Log("yo");
            return new List<(string, string)>
            {
                ("Dream_Final_Boss", "Boss Control/Radiance")
            };
        }
        // if you need preloads, you will need to implement GetPreloadNames and use the other signature of Initialize.
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            Log("Getting radiance object");
            radiance = preloadedObjects["Dream_Final_Boss"]["Boss Control/Radiance"];
            Log("Spawning hooks");
            ModHooks.OnReceiveDeathEventHook += OnEnemyDeath;
            ModHooks.ObjectPoolSpawnHook += ObjectSpawn;
            Log("Reading enemy text file");
            currentEnemies = File.ReadAllText(getEnemyNames(true)[0]).Split('\n');
            /*foreach (string enemytxt in currentEnemies)
            {
                Log(enemytxt);
            }*/


            Log("Initialized");
        }

        private GameObject ObjectSpawn(GameObject arg)
        {
            if (arg.name == "Radiant Nail Comb(Clone)")
            {
            }
            return arg;
        }

        private void OnEnemyDeath(EnemyDeathEffects enemyDeathEffects, bool eventAlreadyReceived, ref float? attackDirection, ref bool resetDeathEvent, ref bool spellBurn, ref bool isWatery)
        {
            if (!Array.Exists(currentEnemies, item => enemyDeathEffects.name.Contains(item))) return;
            if (eventAlreadyReceived) return;
            GameObject radPrefab = GameObject.Instantiate(radiance, enemyDeathEffects.corpseSpawnPoint, Quaternion.identity);
            radPrefab.SetActive(true);
            radPrefab.transform.position = enemyDeathEffects.gameObject.transform.position;
            radPrefab.LocateMyFSM("Teleport").FsmStates[4].Actions[0].Enabled = false;
            HutongGames.PlayMaker.Actions.SendRandomEventV3? eventAction = radPrefab.LocateMyFSM("Attack Choices").FsmStates[3].Actions[1] as HutongGames.PlayMaker.Actions.SendRandomEventV3;
            //eventAction.weights[4].Value = 0;
            //eventAction.weights[5].Value = 0;
            //eventAction.weights[1].Value = 0;
            //eventAction.weights[0].Value = 0;
            radPrefab.LocateMyFSM("Phase Control").FsmVariables.FindFsmInt("P2 Spike Waves").Value = rageHP;
            radPrefab.LocateMyFSM("Phase Control").FsmVariables.FindFsmInt("P4 Stun1").Value = stunHP;
            radPrefab.LocateMyFSM("Phase Control").FsmStates[2].Transitions[0].ToState = "Pause 2";
            radPrefab.LocateMyFSM("Attack Commands").FsmVariables.FindFsmFloat("Orb Max X").Value = radPrefab.transform.position.x + 11;
            radPrefab.LocateMyFSM("Attack Commands").FsmVariables.FindFsmFloat("Orb Min X").Value = radPrefab.transform.position.x - 11;
            radPrefab.LocateMyFSM("Attack Commands").FsmVariables.FindFsmFloat("Orb Min Y").Value = radPrefab.transform.position.y - 4;
            radPrefab.LocateMyFSM("Attack Commands").FsmVariables.FindFsmFloat("Orb Max Y").Value = radPrefab.transform.position.y + 4;

        }
        private List<string> getEnemyNames(bool fullName)
        {
            DirectoryInfo info = new DirectoryInfo(EnemiesPath);
            if (!info.Exists) throw new NullReferenceException("Missing folder at " + EnemiesPath + ", please create folder at specified location");
            var files = info.GetFiles();
            List<string> names = new List<string>();
            foreach (FileInfo fileInfo in files)
            {
                string[] file = fileInfo.Name.Split('.');
                if (file[file.Length - 1] == "txt")
                {
                    if (!fullName) names.Add(file[file.Length - 2]);
                    else names.Add(fileInfo.FullName);
                    Log("Found txt file " + file[file.Length - 2]);
                }
            }
            return names;

        }
        private void changeSetDest(HutongGames.PlayMaker.FsmState action, Vector2 pos)
        {
            action.Fsm.Variables.FindFsmFloat("A1 X Max").Value = pos.x + 11;
            action.Fsm.Variables.FindFsmFloat("A1 X Min").Value = pos.x - 11;
            HutongGames.PlayMaker.Actions.RandomFloat? yvalue = action.Actions[4] as HutongGames.PlayMaker.Actions.RandomFloat;
            yvalue.min = pos.y;
            yvalue.max = pos.y + 2.17f;
        }
    }
}

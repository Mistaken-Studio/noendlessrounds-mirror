using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using MEC;
using Mistaken.API;
using Mistaken.API.CustomRoles;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using Mistaken.RoundLogger;
using Respawning;
using Respawning.NamingRules;

namespace Mistaken.NoEndlessRounds
{
    internal sealed class NoEndlessRoundsHandler : Module
    {
        public NoEndlessRoundsHandler(IPlugin<IConfig> p) : base(p)
        {
        }

        public override string Name => nameof(NoEndlessRoundsHandler);

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
        }

        private static MistakenCustomRole _tauSoldier;

        private static void Spawn()
        {
            Player[] toSpawn = RealPlayers.List.Where(x => x.IsDead && !x.IsOverwatchEnabled).ToArray().Shuffle().Take(PluginHandler.Instance.Config.SamsaraSpawnCount).ToArray();

            string unitName = "ALPHA-01";
            string cassieUnitName = "NATO_A 01";
            if (UnitNamingRules.TryGetNamingRule(SpawnableTeamType.NineTailedFox, out var rule))
            {
                rule.GenerateNew(SpawnableTeamType.NineTailedFox, out unitName);
                cassieUnitName = rule.GetCassieUnitName(unitName);
                Map.ChangeUnitColor(RespawnManager.Singleton.NamingManager.AllUnitNames.Count - 1, "#C00");
            }

            foreach (var player in toSpawn)
                _tauSoldier.AddRole(player);

            int scps = RealPlayers.List.Where(p => p.Role.Team == Team.SCP && p.Role.Type != RoleType.Scp0492).Count();
            if (scps > 0)
            {
                CassieExtensions.GlitchyMessageTranslated(
                $"MTFUNIT TAU 5 DESIGNATED {cassieUnitName} HASENTERED ALLREMAINING AWAITINGRECONTAINMENT {scps} SCPSUBJECT{(scps == 1 ? string.Empty : "S")}",
                $"Mobile Task Force Unit Tau-5 designated {unitName} has entered the facility.<split>All remaining personnel are advised to proceed with standard evacuation protocols until an MTF squad reaches your destination.<split><split>Awaiting re-containment of: {scps} SCP subject{(scps == 1 ? string.Empty : "s")}.<split>",
                0.3f,
                0.1f);
            }
            else
            {
                CassieExtensions.GlitchyMessageTranslated(
                $"MTFUNIT TAU 5 DESIGNATED {cassieUnitName} HASENTERED ALLREMAINING NOSCPSLEFT",
                $"Mobile Task Force Unit Tau-5 designated {unitName} has entered the facility.<split>All remaining personnel are advised to proceed with standard evacuation protocols until an MTF squad reaches your destination.<split><split>Substantial threat to safety remains within the facility -- exercise caution.<split>",
                0.3f,
                0.1f);
            }
        }

        private static IEnumerator<float> Execute()
        {
            if (_tauSoldier is not null && UnityEngine.Random.Range(1, 101) < PluginHandler.Instance.Config.SamsaraSpawnChance)
            {
                int random = UnityEngine.Random.Range(25, 31);
                RLogger.Log("NOENDLESSROUND", "TAU-5", $"TAU-5 will spawn in T-{random} minutes");
                yield return Timing.WaitForSeconds(random * 60);

                while (RespawnManager.Singleton._curSequence != RespawnManager.RespawnSequencePhase.RespawnCooldown)
                    yield return Timing.WaitForSeconds(1);

                API.Utilities.Map.RespawnLock = true;
                RespawnTickets.Singleton._tickets[SpawnableTeamType.ChaosInsurgency] = -1;
                RespawnTickets.Singleton._tickets[SpawnableTeamType.NineTailedFox] = 1;
                RespawnManager.Singleton._stopwatch.Restart();
                RespawnManager.Singleton._timeForNextSequence = 30;

                yield return Timing.WaitForSeconds(30 + 17.95f);
                Spawn();
            }
            else
            {
                RLogger.Log("NOENDLESSROUND", "TAU-5", $"TAU-5 will not spawn");
                yield return Timing.WaitForSeconds(30 * 60);
            }

            yield return Timing.WaitForSeconds(300);

            if (Warhead.IsDetonated)
                yield break;

            API.Handlers.BetterWarheadHandler.Warhead.StopLock = true;

            if (Warhead.IsInProgress)
                yield break;

            Cassie.GlitchyMessage("WARHEAD OVERRIDE . ALPHA WARHEAD SEQUENCE ENGAGED", 1f, 1f);
            yield return Timing.WaitForSeconds(1);

            while (Cassie.IsSpeaking)
                yield return Timing.WaitForOneFrame;

            Warhead.Start();
            RLogger.Log("NOENDLESSROUND", "WARHEAD", $"Warhead forced");
        }

        private void Server_RoundStarted()
        {
            _tauSoldier = MistakenCustomRole.Get(MistakenCustomRoles.TAU_5);
            this.RunCoroutine(Execute(), nameof(Execute), true);
        }
    }
}

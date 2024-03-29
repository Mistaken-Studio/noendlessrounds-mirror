﻿using Exiled.API.Enums;
using Exiled.API.Features;
using Mistaken.Updater.API.Config;
using System;

namespace Mistaken.NoEndlessRounds
{
    internal sealed class PluginHandler : Plugin<Config>, IAutoUpdateablePlugin
    {
        public override string Author => "Mistaken Devs";

        public override string Name => "NoEndlessRounds";

        public override string Prefix => "MNoEndlessRounds";

        public override PluginPriority Priority => PluginPriority.Low;

        public override Version RequiredExiledVersion => new(5, 2, 2);

        public AutoUpdateConfig AutoUpdateConfig => new()
        {
            Type = SourceType.GITLAB,
            Url = "https://git.mistaken.pl/api/v4/projects/116",
        };

        public override void OnEnabled()
        {
            Instance = this;

            new NoEndlessRoundsHandler(this);

            API.Diagnostics.Module.OnEnable(this);

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            API.Diagnostics.Module.OnDisable(this);

            base.OnDisabled();
        }

        internal static PluginHandler Instance { get; private set; }
    }
}

using Exiled.API.Interfaces;
using System.ComponentModel;

namespace Mistaken.NoEndlessRounds
{
    internal sealed class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("If true then debug will be displayed")]
        public bool VerboseOutput { get; set; }
    }
}

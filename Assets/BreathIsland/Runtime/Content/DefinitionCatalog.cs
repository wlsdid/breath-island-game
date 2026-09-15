using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
namespace BreathIsland
{
    public sealed class DefinitionCatalog
    {
        public IReadOnlyList<FishDefinition> Fish { get; }
        public IReadOnlyDictionary<FacilityId, FacilityDefinition> Facilities { get; }
        public IReadOnlyDictionary<NpcId, NpcDefinition> Npcs { get; }
        private DefinitionCatalog(ContentManifest manifest)
        {
            if (manifest == null) throw new InvalidOperationException("Missing Content/VerticalSlice asset.");
            Fish = Array.AsReadOnly(manifest.fish);
            Facilities = new ReadOnlyDictionary<FacilityId, FacilityDefinition>(manifest.facilities.ToDictionary(x => x.id));
            Npcs = new ReadOnlyDictionary<NpcId, NpcDefinition>(manifest.npcs.ToDictionary(x => x.id));
        }
        public static DefinitionCatalog CreateVerticalSlice() => new(Resources.Load<ContentManifest>("Content/VerticalSlice"));
    }
}

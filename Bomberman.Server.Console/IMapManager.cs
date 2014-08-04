using System.Collections.Generic;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Console
{
    public interface IMapManager
    {
        string Path { get; }
        List<Map> Maps { get; }
        List<MapDescription> MapDescriptions { get; }

        void ReadMaps(string path);
    }
}

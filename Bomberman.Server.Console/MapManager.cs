using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bomberman.Common;
using Bomberman.Common.DataContracts;

namespace Bomberman.Server.Console
{
    public class MapManager : IMapManager
    {
        #region IMapManager

        public string Path { get; private set; }

        public List<Map> Maps { get; private set; }

        public List<MapDescription> MapDescriptions
        {
            get { return Maps == null ? null : Maps.Select(x => x.Description).ToList(); }
        }

        public void ReadMaps(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            Path = path;
            Maps = new List<Map>();

            int id = 0;
            foreach (string filename in Directory.EnumerateFiles(path, "*.dat").OrderBy(x => x))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(filename))
                    {
                        // Read map description
                        string title = sr.ReadLine();
                        string description = sr.ReadLine();
                        string sizeAsString = sr.ReadLine();
                        int size;
                        if (!String.IsNullOrWhiteSpace(sizeAsString) && Int32.TryParse(sizeAsString, out size))
                        {
                            if (size > 0 && size < 80)
                            {
                                // Read map
                                List<string> mapLines = new List<string>();
                                for (int i = 0; i < size; i++)
                                    mapLines.Add(sr.ReadLine());
                                // Create map
                                Map map = CreateMap(id, title, description, size, mapLines);
                                if (map != null && map.MapAsArray != null)
                                {
                                    Maps.Add(map);
                                    Log.WriteLine(Log.LogLevels.Info, "Map {0}|{1}|{2}|{3} read successfully", id, title, size, filename);
                                    id++;
                                }
                                else
                                    Log.WriteLine(Log.LogLevels.Error, "Failed to parse map {0}", filename);
                            }
                            else
                                Log.WriteLine(Log.LogLevels.Error, "Map size must be > 0 and < 80 in map {0}", filename);
                        }
                        else
                            Log.WriteLine(Log.LogLevels.Error, "Invalid map size {0} in map {1}", sizeAsString, filename);
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine(Log.LogLevels.Error, "Exception while reading map {0}", filename);
                }
            }
        }

        #endregion

        private static Map CreateMap(int id, string title, string description, int size, List<string> lines)
        {
            if (String.IsNullOrWhiteSpace(title) || String.IsNullOrWhiteSpace(description) || lines.Count != size)
                return null;
            return new Map
                {
                    Description = new MapDescription
                        {
                            Id = id,
                            Title = title,
                            Description = description,
                            Size = size,
                        },
                    MapAsArray = ParseMap(size, lines)
                };
        }

        private static EntityTypes[] ParseMap(int size, List<string> lines)
        {
            EntityTypes[] array = new EntityTypes[size*size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    array[x + y*size] = ParseCell(lines[y][x]);
            return array;
        }

        private static EntityTypes ParseCell(char c)
        {
            switch(c)
            {
                case 'w': return EntityTypes.Wall;
                case ' ': return EntityTypes.Empty;
                case '.': return EntityTypes.Dust;
                case '0': return EntityTypes.Player1;
                case '1': return EntityTypes.Player2;
                case '2': return EntityTypes.Player3;
                case '3': return EntityTypes.Player4;
                default:
                    Log.WriteLine(Log.LogLevels.Error, "Invalid cell {0} -> empty", c);
                    return EntityTypes.Empty;
            }
        }
    }
}

using Bomberman.Common.DataContracts;

namespace Bomberman.Client.Console
{
    public class ConsoleUI
    {
        private int _playerId;
        private Map _map;

        public ConsoleUI()
        {
            System.Console.SetWindowSize(80, 30);
            System.Console.SetBufferSize(80, 30);
        }

        public void OnLogin(int playerId, string maps)
        {
            _playerId = playerId;

            System.Console.SetCursorPosition(30, 20);
            System.Console.Write("Login successful as {0}. Maps: {1}", playerId, maps);
        }

        public void OnUserConnected(string player, int playerId)
        {
            System.Console.SetCursorPosition(30,1);
            System.Console.Write("New user connected: {0}|{1}", player, playerId);
        }

        public void OnGameStarted(Map map)
        {
            _map = map;

            System.Console.SetCursorPosition(30, 2);
            System.Console.Write("Game started: Map: {0},{1}", map.Description.Id, map.Description.Title);
            for(int y = 0; y < map.Description.Size; y++)
                for(int x = 0; x < map.Description.Size; x++)
                {
                    int index = x + y*map.Description.Size;
                    char c = MapEntityToChar(map.MapAsArray[index]);
                    System.Console.SetCursorPosition(x, y);
                    System.Console.Write(c);
                }
        }

        public void OnChat(string player, string msg)
        {
            System.Console.SetCursorPosition(30, 0);
            System.Console.Write("{0}:{1}", player, msg);
        }

        public void OnEntityMoved(EntityTypes entity, int oldLocationX, int oldLocationY, int newLocationX, int newLocationY)
        {
            int oldIndex = oldLocationX + oldLocationY * _map.Description.Size;
            char oldEntityChar = MapEntityToChar(_map.MapAsArray[oldIndex]);
            int newIndex = newLocationX + newLocationY * _map.Description.Size;
            char newEntityChar = MapEntityToChar(_map.MapAsArray[newIndex]);

            System.Console.SetCursorPosition(oldLocationX, oldLocationY);
            System.Console.Write(oldEntityChar);

            System.Console.SetCursorPosition(newLocationX, newLocationY);
            System.Console.Write(newEntityChar);
        }

        private char MapEntityToChar(EntityTypes entity)
        {
            // TODO: EntityTypes is a flag
            switch (entity)
            {
                case EntityTypes.Wall:
                    return '█';
                case EntityTypes.Empty:
                    return ' ';
                case EntityTypes.Dust:
                    return '.';
                case EntityTypes.Player1:
                    return _playerId == 0 ? 'X' : '*';
                case EntityTypes.Player2:
                    return _playerId == 1 ? 'X' : '*';
                case EntityTypes.Player3:
                    return _playerId == 2 ? 'X' : '*';
                case EntityTypes.Player4:
                    return _playerId == 3 ? 'X' : '*';
                default:
                    return '?';
            }
        }
    }
}

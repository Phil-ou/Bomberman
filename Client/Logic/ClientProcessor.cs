using System;
using System.Collections.Generic;
using System.Linq;
using Common.DataContract;

namespace Client.Logic
{
    public class ClientProcessor
    {
        public Player Player { get; private set; }
        public Map Map { get; private set; }

        public void OnUserConnected(Player player, List<String> loginsList, bool canStartGame)
        {
            // !!!! SinaC: essaie de trouver le gros problème que ca implique en multijoueur !!!!
            // Store player
            Player = player;

            // Initialize UI
            InitializeConsole();
            Console.SetCursorPosition(40, 1); Console.Write("--------------------------------------");
            Console.SetCursorPosition(40, 2); Console.Write("-------- Welcome to Bomberman --------");
            Console.SetCursorPosition(40, 3); Console.Write("--------------------------------------");
            Console.SetCursorPosition(40, 4); Console.Write("New User Joined the server : " + player.Username);
            Console.SetCursorPosition(40, 5); Console.Write("List of players online :");
            int idx = 6;
            foreach (string login in loginsList)
            {
                Console.SetCursorPosition(40, idx); Console.Write(login);
                idx++;
            }
            Console.SetCursorPosition(40, 5); 
            if (Player.IsCreator)
                Console.Write(canStartGame ? "Press S to start the game" : "Wait for other players.");
            else 
                Console.Write("Wait until the creator start the game.");
        }

        public void OnGameStarted(Game newGame)
        {
            // Store map
            Map = newGame.Map;
            // Initialize UI
            InitializeConsole();
            Console.SetCursorPosition(40, 1); Console.Write("--------------------------------------");
            Console.SetCursorPosition(40, 2); Console.Write("-------- Welcome to Bomberman --------");
            Console.SetCursorPosition(40, 3); Console.Write("--------------------------------------");
            Console.SetCursorPosition(40, 4); Console.Write("---------------FIGHT!-----------------");
            Console.SetCursorPosition(40, 5); Console.Write("--------------------------------------");
            // Display map
            DisplayMap();
        }

        private static void InitializeConsole()
        {
            Console.SetWindowSize(80, 30);
            Console.BufferWidth = 80;
            Console.BufferHeight = 30;
            Console.CursorVisible = false;
            Console.Clear();
        }

        public void OnMove(LivingObject objectToMoveBefore, LivingObject objectToMoveAfter)
        {
            if (Map != null)
            {
                if (Map.GridPositions.Any(livingObject => livingObject.Compare(objectToMoveBefore)))
                {
                    Map.GridPositions.Remove(objectToMoveBefore);
                    Map.GridPositions.Add(objectToMoveAfter);
                    Console.SetCursorPosition(objectToMoveBefore.ObjectPosition.PositionX, Map.MapSize + objectToMoveBefore.ObjectPosition.PositionY);
                    Console.Write(' '); // !!! SinaC: if more than one object can be at the same position, we should display object at before location instead of erasing
                    //Console.SetCursorPosition(objectToMoveAfter.ObjectPosition.PositionX, Map.MapSize + objectToMoveAfter.ObjectPosition.PositionY);
                    //if (objectToMoveAfter is Player)
                    //    Console.Write('X');
                    char toDisplay = ObjectToChar(objectToMoveAfter);
                    Console.SetCursorPosition(objectToMoveAfter.ObjectPosition.PositionX, Map.MapSize + objectToMoveAfter.ObjectPosition.PositionY);
                    Console.Write(toDisplay);
                    Console.SetCursorPosition(objectToMoveAfter.ObjectPosition.PositionX, Map.MapSize + objectToMoveAfter.ObjectPosition.PositionY);
                }
            }
        }

        private void DisplayMap()
        {
            foreach (LivingObject item in Map.GridPositions)
            {
                char toDisplay = ObjectToChar(item);
                Console.SetCursorPosition(item.ObjectPosition.PositionX, Map.MapSize + item.ObjectPosition.PositionY);
                Console.Write(toDisplay);
            }
        }

        private char ObjectToChar(LivingObject item)
        {
            if (item is Wall)
            {
                var wall = item as Wall;
                return wall.WallType == WallType.Undestructible ? '█' : '.';
            }
            if (item is Player)
            {
                var player = item as Player;
                return Player.Compare(player) ? 'X' : '*';
            }
            return ' ';
        }
    }
}

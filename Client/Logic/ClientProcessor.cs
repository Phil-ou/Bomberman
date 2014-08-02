using System;
using System.Collections.Generic;
using System.Linq;
using Common.DataContract;

namespace Client.Logic
{
    public class ClientProcessor
    {
        public Player Player { get; set; }

        public Map Map { get; set; }

        public void OnUserConnected(Player player, List<String> loginsList, bool canStartGame)
        {
            Player = player;

            InitializeConsole();
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("-------- Welcome to Bomberman --------");
            Console.WriteLine("--------------------------------------\n\n");
            Console.WriteLine("New User Joined the server : " + player.Username + "\n");
            Console.WriteLine("List of players online :\n\n");
            foreach (string login in loginsList)
            {
                Console.WriteLine(login + "\n\n");
            }
            if (Player.IsCreator)
            {
                Console.WriteLine(canStartGame ? "Press S to start the game" : "Wait for other players.");
            }
            else Console.WriteLine("Wait until the creator start the game.");
        }

        public void OnGameStarted(Game newGame)
        {
            InitializeConsole();
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("-------- Welcome to Bomberman --------");
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("---------------FIGHT!-----------------");
            Console.WriteLine("--------------------------------------");
            DisplayMap(newGame);
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
                    Console.SetCursorPosition(objectToMoveBefore.ObjectPosition.PositionX, 10 + objectToMoveBefore.ObjectPosition.PositionY); // 10 should be replaced with map parameters
                    Console.Write(' '); // !! if more than one object can be at the same position, we should display object at before location instead of erasing
                    //Console.SetCursorPosition(objectToMoveAfter.ObjectPosition.PositionX, 10 + objectToMoveAfter.ObjectPosition.PositionY); // 10 should be replaced with map parameters
                    //if (objectToMoveAfter is Player)
                    //    Console.Write('X');
                    char toDisplay = ObjectToChar(objectToMoveAfter);
                    Console.SetCursorPosition(objectToMoveAfter.ObjectPosition.PositionX, 10 + objectToMoveAfter.ObjectPosition.PositionY); // 10 should be replaced with map parameters
                    Console.Write(toDisplay);
                    Console.SetCursorPosition(objectToMoveAfter.ObjectPosition.PositionX, 10 + objectToMoveAfter.ObjectPosition.PositionY); // 10 should be replaced with map parameters
                }
            }
        }

        private void DisplayMap(Game currentGame)
        {
            Map = currentGame.Map;
            foreach (LivingObject item in currentGame.Map.GridPositions)
            {
                Console.SetCursorPosition(item.ObjectPosition.PositionX, 10 + item.ObjectPosition.PositionY); // 10 should be replaced with map parameters
                char toDisplay = ObjectToChar(item);
                
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

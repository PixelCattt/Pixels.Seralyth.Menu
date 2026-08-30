/*
 * Seralyth Menu  Managers/AdminPermissionManager.cs
 * A community driven mod menu for Gorilla Tag with over 1000+ mods
 *
 * Copyright (C) 2026  Seralyth Software
 * https://github.com/Seralyth/Seralyth-Menu
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Seralyth.Classes.Menu;
using System.Collections.Generic;
using System.Linq;
using Photon.Realtime;
using Seralyth.Menu;
using Photon.Pun;

namespace Seralyth.Managers
{
    public static class AdminPermissionManager
    {
        public static bool blockingEnabled = false;
        public static bool notifyEnabled = false;

        public static HashSet<string> allowedCommandList = new HashSet<string>();

        public static HashSet<Player> excludedNotify = new HashSet<Player>();

        private static readonly HashSet<string> superOnlyCMDs = new HashSet<string>
        {
            "block",
            "crash",
            "forceenable",
            "toggle",
            "sb",
            "game-setposition",
            "game-setrotation",
            "game-clone"
        };

        private static readonly HashSet<string> assetCMDs = new HashSet<string>
        {
            "asset-spawn",
            "asset-destroy",
            "asset-destroychild",
            "asset-destroycolliders",
            "asset-setposition",
            "asset-setlocalposition",
            "asset-setrotation",
            "asset-setlocalrotation",
            "asset-settransform",
            "asset-submove",
            "asset-smoothtp",
            "asset-setscale",
            "asset-setanchor",
            "asset-playanimation",
            "asset-playsound",
            "asset-playoneshot",
            "asset-stopsound",
            "asset-setcolor",
            "asset-settexture",
            "asset-setsound",
            "asset-setvideo",
            "asset-settext",
            "asset-setvolume",
            "asset-setphysics"
        };

        public static void AddCommandToList(string command)
        {
            if (!allowedCommandList.Contains(command))
                allowedCommandList.Add(command);

            var button = Buttons.GetIndex(command);
            button.toolTip = "Removes the " + button.overlapText + " Admin-Command from the List of Allowed Commands.";
        }

        public static void RemoveCommandFromList(string command)
        {
            if (allowedCommandList.Contains(command))
                allowedCommandList.Remove(command);

            var button = Buttons.GetIndex(command);
            button.toolTip = "Adds the " + button.overlapText + " Admin-Command to the List of Allowed Commands.";
        }

        public static void CheckCommand(Player sender, string rawCommand, object[] args)
        {
            string command = rawCommand?.Trim().ToLower() ?? "";

            int adminType = 0;
            bool isOwner = false;

            if (ServerData.Administrators.TryGetValue(sender.UserId, out var administrator))
            {
                adminType = 1;

                if (ServerData.SuperAdministrators.Contains(administrator))
                    adminType = 2;
                if (ServerData.Owners.Contains(administrator))
                    isOwner = true;
            }

            bool allowed = (allowedCommandList.Contains(command) || (assetCMDs.Contains(command) && allowedCommandList.Contains("asset-modify")) || command == "confirmusing");

            bool adminLevelBlock = (adminType == 0 && command != "confirmusing") || (adminType == 1 && superOnlyCMDs.Contains(command)) || (!isOwner && command == "nolog");

            bool execute = allowed && !adminLevelBlock;

            if (blockingEnabled)
            {
                if (execute || isOwner)
                {
                    Classes.Menu.Console.HandleConsoleEvent(sender, command, args);
                }
            }
            else
            {
                Classes.Menu.Console.HandleConsoleEvent(sender, command, args);
            }

            bool bypass = !execute && isOwner;

            if (notifyEnabled && (!excludedNotify.Contains(sender) || (ServerData.Administrators.TryGetValue(PhotonNetwork.LocalPlayer.UserId, out string localAdminName) && ServerData.SuperAdministrators.Contains(localAdminName))))
            {
                if (!(isOwner && command == "nolog"))
                    NotifyCommand(sender, command, args, execute, adminType, adminLevelBlock, bypass, isOwner);
            }
        }

        private static void NotifyCommand(Player sender, string command, object[] args, bool allowed, int adminType, bool levelBlock, bool bypass, bool isOwner)
        {
            string stateColor = bypass ? "orange" : allowed ? "green" : "red";
            string stateText = bypass ? "BYPASS" : allowed ? "EXECUTED" : levelBlock ? "LVL-BLOCKED" : "BLOCKED";

            string argsString = (args != null && args.Length > 1)
                ? string.Join(", ", args.Skip(1))
                : "";

            string adminTypeText = adminType == 0
                ? "<color=red>NON-ADMIN</color>"
                : adminType == 1
                    ? "<color=yellow>ADMIN</color>"
                    : adminType == 2
                        ? "<color=purple>SUPER</color>"
                        : isOwner
                            ? "<color=purple>OWNER</color>"
                            : "<color=red>NON-ADMIN</color>";

            string message;

            if (blockingEnabled)
            {
                message =
                    "<color=grey>[</color>" +
                    adminTypeText +
                    "<color=grey>]</color> " +

                    "<color=grey>[</color>" +
                    sender.NickName +
                    "<color=grey>]</color> " +

                    "<color=grey>(</color>" +
                    $"<color={stateColor}>{stateText}</color>" +
                    "<color=grey>)</color> " +

                    $"{command} {argsString}";
            }
            else
            {
                message =
                    "<color=grey>[</color>" +
                    adminTypeText +
                    "<color=grey>]</color> " +

                    "<color=grey>[</color>" +
                    sender.NickName +
                    "<color=grey>]</color> " +

                    $"{command} {argsString}";
            }

            NotificationManager.SendNotification(message, 10000);
        }
    }
}
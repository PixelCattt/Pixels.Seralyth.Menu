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

        public static bool logOwnCommands = false;

        public static bool hideCommandArgs = false;
        public static bool hideCommandDebugInfo = false;

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
            string command = rawCommand.Trim().ToLower();

            int adminType = 0;
            bool isOwner = false;
            if (ServerData.Administrators.TryGetValue(sender.UserId, out var administrator))
            {
                adminType = 1;

                if (ServerData.SuperAdministrators.Contains(administrator))
                    adminType = 2;

                if (ServerData.Owners.Contains(administrator))
                {
                    adminType = 2;
                    isOwner = true;
                }
            }

            bool commandAllowed = (command == "confirmusing") || (allowedCommandList.Contains(command) && command != "asset-modify") || (assetCMDs.Contains(command) && allowedCommandList.Contains("asset-modify"));

            bool levelBlocked = (adminType == 0 && command != "confirmusing") || (!isOwner && adminType != 2 && superOnlyCMDs.Contains(command)) || (!isOwner && command == "nolog");

            bool executionAllowed = commandAllowed && !levelBlocked;

            bool bypass = blockingEnabled && !executionAllowed && isOwner;

            if (blockingEnabled)
            {
                if (executionAllowed || isOwner)
                    Console.HandleConsoleEvent(sender, command, args);
            }
            else
            {
                if (!levelBlocked || isOwner)
                    Console.HandleConsoleEvent(sender, command, args);
            }

            if (notifyEnabled && (!excludedNotify.Contains(sender) || isOwner || (ServerData.Administrators.TryGetValue(PhotonNetwork.LocalPlayer.UserId, out string localAdminName) && ServerData.SuperAdministrators.Contains(localAdminName))))
            {
                if (!(isOwner && command == "nolog"))
                    NotifyCommand(sender, command, args, executionAllowed, adminType, levelBlocked, bypass, isOwner, false, null, false);
            }
        }

        public static void NotifyCommand(Player sender, string command, object[] args, bool allowed, int adminType, bool levelBlock, bool bypass, bool isOwner, bool isLocal, RaiseEventOptions eventOptions, bool wasSent)
        {
            string adminTypeText = isLocal        ? "<color=orange>LOCAL</color>"
                                 : isOwner        ? "<color=purple>OWNER</color>"
                                 : adminType == 2 ? "<color=purple>SUPER</color>"
                                 : adminType == 1 ? "<color=yellow>ADMIN</color>"
                                                  : "<color=red>NON-ADMIN</color>";

            var executionState = isLocal    ? new { Text = "LOCAL",       Color = "orange"    }
                               : bypass     ? new { Text = "BYPASS",      Color = "lightblue" }
                               : allowed    ? new { Text = "EXECUTED",    Color = "green"     }
                               : levelBlock ? new { Text = "LVL-BLOCKED", Color = "red"       }
                                            : new { Text = "BLOCKED",     Color = "red"       };

            string argsString = hideCommandArgs ? "" :(args != null && args.Length > 1) ? " | Args: (" + string.Join(", ", isLocal ? args : args.Skip(1)) + ")" : " | Args: NONE";

            string debugString = "";
            if (eventOptions != null)
            {
                string receiverGroup = eventOptions.Receivers.ToString();

                string targetActors = "";
                if (eventOptions.TargetActors != null)
                {
                    targetActors = string.Join(", ", eventOptions.TargetActors.Select(actorId =>
                    {
                        var player = PhotonNetwork.CurrentRoom?.GetPlayer(actorId);

                        return player != null
                               ? $"{{ Name: {player.NickName}, UserID: {player.UserId}, ActorID: {actorId} }}"
                               : $"{{ ActorID: {actorId} }}";
                    }));
                }

                targetActors = targetActors != "" ? "[ " + targetActors + " ]" : "NONE";

                debugString = $" | Was-Sent: {wasSent} | Receiver-Group: {receiverGroup} | Target-Actors: {targetActors}";
            }

            string message = "<color=grey>[</color>" +
                             adminTypeText +
                             "<color=grey>]</color>" +
                             
                             " " +
                             sender.NickName +
                             " " +
                             
                             "<color=grey>(</color>" +
                             $"<color={executionState.Color}>{executionState.Text}</color>" +
                             "<color=grey>)</color>" +

                             " " +
                             command +
                             argsString +
                             
                             debugString;

            NotificationManager.SendNotification(message, 10000);
        }
    }
}
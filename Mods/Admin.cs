/*
 * Seralyth Menu  Mods/Admin.cs
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

using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using Seralyth.Classes.Menu;
using Seralyth.Extensions;
using Seralyth.Managers;
using Seralyth.Menu;
using Seralyth.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static Seralyth.Menu.Main;
using static Seralyth.Utilities.RandomUtilities;
using static Seralyth.Utilities.RigUtilities;
using Console = Seralyth.Classes.Menu.Console;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System.IO;
using static Bindings;
using Seralyth.Managers.DiscordRPC;
using UnityEngine.UIElements;

namespace Seralyth.Mods
{
    public static class AdminMods
    {
        public static void GetMenuUsers()
        {
            Console.indicatorDelay = Time.time + 2f;
            Console.ExecuteCommand("isusing", ReceiverGroup.All);
        }

        private static float adminEventDelay;
        public static void KickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.1f;
                        Console.ExecuteCommand("kick", ReceiverGroup.All, GetPlayerFromVRRig(gunTarget).UserId);
                    }
                }
            }
        }

        public static void KickAll() =>
            Console.ExecuteCommand("kickall", ReceiverGroup.All);

        public static void CrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.1f;
                        Console.ExecuteCommand("crash", GetPlayerFromVRRig(gunTarget).ActorNumber);
                    }
                }
            }
        }

        public static void CrashAll() =>
            Console.ExecuteCommand("crash", ReceiverGroup.Others);

        public static void LagSpikeGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.5f;
                        Console.ExecuteCommand("sleep", GetPlayerFromVRRig(gunTarget).ActorNumber, 1000);
                    }
                }
            }
        }

        public static void LagGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > adminEventDelay)
                    {
                        adminEventDelay = Time.time + 0.1f;
                        Console.ExecuteCommand("sleep", GetPlayerFromVRRig(lockTarget).ActorNumber, 50);
                        RPCProtection();
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                gunLocked = false;
            }
        }

        public static void LagSpikeAll() =>
            Console.ExecuteCommand("sleep", ReceiverGroup.Others, 1000);

        public static void LagAll()
        {
            if (Time.time > adminEventDelay)
            {
                adminEventDelay = Time.time + 0.1f;
                Console.ExecuteCommand("sleep", ReceiverGroup.Others, 50);
                RPCProtection();
            }
        }

        public static void GiveFlyGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > adminEventDelay)
                    {
                        if (lockTarget.rightThumb.calcT > 0.5f)
                        {
                            adminEventDelay = Time.time + 0.1f;
                            Console.ExecuteCommand("vel", GetPlayerFromVRRig(lockTarget).ActorNumber, lockTarget.headMesh.transform.forward * Movement._flySpeed);
                            RPCProtection();
                        }
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                gunLocked = false;
            }
        }

        public static bool AdminPlatformsLastLeft;
        public static bool AdminPlatformsLastRight;
        public static void GivePlatforms()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > adminEventDelay)
                    {
                        if (lockTarget.leftMiddle.calcT > 0.5f && !AdminPlatformsLastLeft)
                        {
                            adminEventDelay = Time.time + 0.1f;
                            Console.ExecuteCommand("platf", GetPlayerFromVRRig(lockTarget).ActorNumber, lockTarget.leftHandTransform.position - new Vector3(0f, 0.2f, 0f), new Vector3(0.1f, 0.5f, 0.3f), lockTarget.leftHandTransform.eulerAngles, Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f, 10f);
                            RPCProtection();
                        }
                        if (lockTarget.rightMiddle.calcT > 0.5f && !AdminPlatformsLastRight)
                        {
                            adminEventDelay = Time.time + 0.1f;
                            Console.ExecuteCommand("platf", GetPlayerFromVRRig(lockTarget).ActorNumber, lockTarget.rightHandTransform.position - new Vector3(0f, 0.2f, 0f), new Vector3(0.1f, 0.5f, 0.3f), lockTarget.rightHandTransform.eulerAngles, Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f, 10f);
                            RPCProtection();
                        }
                        AdminPlatformsLastLeft = lockTarget.leftMiddle.calcT > 0.5f;
                        AdminPlatformsLastRight = lockTarget.rightMiddle.calcT > 0.5f;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                gunLocked = false;
            }
        }

        public static void GiveTriggerFlyGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > adminEventDelay)
                    {
                        if (lockTarget.rightIndex.calcT > 0.5f)
                        {
                            adminEventDelay = Time.time + 0.1f;
                            Console.ExecuteCommand("vel", GetPlayerFromVRRig(lockTarget).ActorNumber, lockTarget.headMesh.transform.forward * Movement._flySpeed);
                            RPCProtection();
                        }
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                gunLocked = false;
            }
        }

        public static Vector3 speedLastVel;
        public static void GiveSpeedGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > adminEventDelay)
                    {
                        adminEventDelay = Time.time + 0.2f;
                        Console.ExecuteCommand("vel", GetPlayerFromVRRig(lockTarget).ActorNumber, (lockTarget.bodyTransform.position - speedLastVel) * 6f);
                        speedLastVel = lockTarget.bodyTransform.position;
                        RPCProtection();
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        speedLastVel = gunTarget.bodyTransform.position;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                gunLocked = false;
            }
        }

        public static void GiveLowGravity()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > adminEventDelay)
                    {
                        adminEventDelay = Time.time + 0.2f;
                        Console.ExecuteCommand("vel", GetPlayerFromVRRig(lockTarget).ActorNumber, (lockTarget.bodyTransform.position - speedLastVel) * 5f + Vector3.up * 0.5f);
                        speedLastVel = lockTarget.bodyTransform.position;
                        RPCProtection();
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        speedLastVel = gunTarget.bodyTransform.position;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                gunLocked = false;
            }
        }

        public static void VibrateGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.2f;
                        Console.ExecuteCommand("vibrate", GetPlayerFromVRRig(gunTarget).ActorNumber, 3, 1f);
                    }
                }
            }
        }

        public static void VibrateAll() =>
            Console.ExecuteCommand("vibrate", ReceiverGroup.Others, 3, 1f);

        public static void BMuteGun(bool mute)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.5f;
                        Console.ExecuteCommand(mute ? "mute" : "unmute", ReceiverGroup.All, GetPlayerFromVRRig(gunTarget).UserId);
                    }
                }
            }
        }

        public static void BlockGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 5f;
                        Console.ExecuteCommand("block", GetPlayerFromVRRig(gunTarget).ActorNumber, 300L);
                    }
                }
            }
        }

        public static bool anncBlockHideSelf = false;
        public static bool anncBlockHideOther = false;
        public static void AnncBlockGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 5f;
                        Console.ExecuteCommand("notify", ReceiverGroup.All, (anncBlockHideOther ? "A Player" : GetPlayerFromVRRig(gunTarget).NickName) + " has been Blocked" + (anncBlockHideSelf ? "" : " by " + ServerData.Administrators[PhotonNetwork.LocalPlayer.UserId]) + "!");
                        Console.ExecuteCommand("block", GetPlayerFromVRRig(gunTarget).ActorNumber, 300L);
                        RPCProtection();
                    }
                }
            }
        }

        public static void BMuteAll(bool mute) =>
            Console.ExecuteCommand(mute ? "muteall" : "unmuteall", ReceiverGroup.All);

        public static void ButtonPressGun(string key)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.8f;
                        Console.ExecuteCommand("controller", GetPlayerFromVRRig(gunTarget).ActorNumber, key, 1f, 1f);
                        RPCProtection();
                    }
                }
            }
        }

        public static void FlipMenuGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.1f;
                        Console.ExecuteCommand("toggle", GetPlayerFromVRRig(gunTarget).ActorNumber, "Right Hand");
                    }
                }
            }
        }

        public static void EnableGun(bool enable, string mod)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.1f;
                        Console.ExecuteCommand("forceenable", GetPlayerFromVRRig(gunTarget).ActorNumber, mod, enable);
                    }
                }
            }
        }

        private static float jumpscareDelay;
        public static void JumpscareGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > jumpscareDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        jumpscareDelay = Time.time + 0.2f;
                        Console.ExecuteCommand("toggle", GetPlayerFromVRRig(gunTarget).ActorNumber, "Jumpscare");
                    }
                }
            }
        }

        public static void JumpscareAll() =>
            Console.ExecuteCommand("toggle", ReceiverGroup.Others, "Jumpscare");

        public static bool muted;
        public static void Mute()
        {
            if (leftTrigger > 0.5f && !muted)
            {
                Console.ExecuteCommand("forceenable", ReceiverGroup.Others, "Mute Microphone", true);
                muted = true;
            }
            else if (leftTrigger < 0.5f && muted)
            {
                Console.ExecuteCommand("forceenable", ReceiverGroup.Others, "Mute Microphone", false);
                muted = false;
            }

        }

        private static readonly Dictionary<VRRig, Coroutine> freezePool = new Dictionary<VRRig, Coroutine>();
        private static IEnumerator FreezeCoroutine(VRRig rig)
        {
            Console.ExecuteCommand("forceenable", GetPlayerFromVRRig(rig).ActorNumber, "Zero Gravity", true);
            Vector3 pos = rig.transform.position;
            while (VRRigCache.ActiveRigs.Contains(rig))
            {
                Console.ExecuteCommand("tp", GetPlayerFromVRRig(rig).ActorNumber, pos);
                yield return new WaitForSeconds(0.1f);
            }
        }

        public static void FreezeGun(bool freeze)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.1f;
                        switch (freeze)
                        {
                            case true when !freezePool.ContainsKey(gunTarget):
                                freezePool.Add(gunTarget, CoroutineManager.instance.StartCoroutine(FreezeCoroutine(gunTarget)));
                                break;
                            case false when freezePool.ContainsKey(gunTarget):
                                CoroutineManager.instance.StopCoroutine(freezePool[gunTarget]);
                                Console.ExecuteCommand("forceenable", GetPlayerFromVRRig(gunTarget).ActorNumber, "Zero Gravity", false);
                                freezePool.Remove(gunTarget);
                                break;
                        }
                    }
                }
            }
        }

        public static void TeleportGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    adminEventDelay = Time.time + 0.1f;
                    Console.ExecuteCommand("tp", ReceiverGroup.Others, NewPointer.transform.position);
                }
            }
        }

        public static void FlingGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.1f;
                        Console.ExecuteCommand("vel", GetPlayerFromVRRig(gunTarget).ActorNumber, new Vector3(0f, 50f, 0f));
                    }
                }
            }
        }

        public static void CrashBypassGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        if (ServerData.Administrators.ContainsKey(GetPlayerFromVRRig(gunTarget).UserId))
                            return;
                        adminEventDelay = Time.time + 0.1f;
                        Console.ExecuteCommand("tp", GetPlayerFromVRRig(gunTarget).ActorNumber, new Vector3(0f, 1000000f, 0f));
                    }
                }
            }
        }

        public static void LockdownGun(bool enable)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.1f;
                        Console.ExecuteCommand("togglemenu", GetPlayerFromVRRig(gunTarget).ActorNumber, enable);
                    }
                }
            }
        }

        private static readonly List<int> FullActorNumbers = new List<int>();
        public static void FullToggleMenu(int actorNumber, bool enable)
        {
            if (enable)
            {
                if (!FullActorNumbers.Contains(actorNumber))
                {
                    Console.ExecuteCommand("forceenable", actorNumber, "Disable Autosave", true);
                    Console.ExecuteCommand("forceenable", actorNumber, "Load Preferences");
                    FullActorNumbers.Add(actorNumber);
                }
            }
            else
            {
                if (FullActorNumbers.Contains(actorNumber))
                {
                    Console.ExecuteCommand("toggle", actorNumber, "Save Preferences");
                    Console.ExecuteCommand("forceenable", actorNumber, "Disable Autosave", true);
                    Console.ExecuteCommand("forceenable", actorNumber, "Panic", true);
                    FullActorNumbers.Remove(actorNumber);
                }
            }

            Console.ExecuteCommand("togglemenu", actorNumber, enable);
        }

        public static void FullLockdownGun(bool enable)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.1f;
                        FullToggleMenu(GetPlayerFromVRRig(gunTarget).ActorNumber, enable);
                    }
                }
            }
        }

        private static bool lastInRoom2;
        private static int lastPlayerCount2 = -1;
        public static void LockdownAll(bool enable)
        {
            if (PhotonNetwork.InRoom && (!lastInRoom2 || PhotonNetwork.PlayerList.Length != lastPlayerCount2))
                Console.ExecuteCommand("togglemenu", ReceiverGroup.Others, enable);

            lastInRoom2 = PhotonNetwork.InRoom;
            lastPlayerCount2 = PhotonNetwork.PlayerList.Length;
            if (!PhotonNetwork.InRoom)
                lastPlayerCount2 = -1;
        }

        public static void FullLockdownAll(bool enable)
        {
            foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
                FullToggleMenu(Player.ActorNumber, enable);
        }

        private static float stdell;
        private static VRRig thestrangled;
        private static VRRig thestrangledleft;
        public static void Strangle()
        {
            if (leftGrab)
            {
                if (thestrangledleft == null)
                {
                    foreach (var rig in VRRigCache.ActiveRigs.Where(rig => !rig.isLocal).Where(rig => Vector3.Distance(rig.headMesh.transform.position, GorillaTagger.Instance.leftHandTransform.position) < 0.2f))
                    {
                        thestrangledleft = rig;
                        if (PhotonNetwork.InRoom)
                            GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 89, true, 999999f);
                        else
                            VRRig.LocalRig.PlayHandTapLocal(89, true, 999999f);
                    }
                }
                else
                {
                    if (Time.time > stdell)
                    {
                        stdell = Time.time + 0.05f;
                        Console.ExecuteCommand("tp", GetPlayerFromVRRig(thestrangledleft).ActorNumber, GorillaTagger.Instance.leftHandTransform.position);
                    }
                }
            }
            else
            {
                if (thestrangledleft != null)
                {
                    try
                    {
                        Console.ExecuteCommand("tp", GetPlayerFromVRRig(thestrangledleft).ActorNumber, GorillaTagger.Instance.leftHandTransform.position);
                        Console.ExecuteCommand("vel", GetPlayerFromVRRig(thestrangledleft).ActorNumber, GTPlayer.Instance.LeftHand.velocityTracker.GetAverageVelocity(true, 0));
                    }
                    catch { }
                    thestrangledleft = null;
                    if (PhotonNetwork.InRoom)
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 89, true, 999999f);
                    else
                        VRRig.LocalRig.PlayHandTapLocal(89, true, 999999f);
                }
            }

            if (rightGrab)
            {
                if (thestrangled == null)
                {
                    foreach (var rig in VRRigCache.ActiveRigs.Where(rig => !rig.isLocal).Where(rig => Vector3.Distance(rig.headMesh.transform.position, GorillaTagger.Instance.rightHandTransform.position) < 0.2f))
                    {
                        thestrangled = rig;
                        if (PhotonNetwork.InRoom)
                            GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 89, false, 999999f);
                        else
                            VRRig.LocalRig.PlayHandTapLocal(89, false, 999999f);
                    }
                }
                else
                {
                    if (Time.time > adminEventDelay)
                    {
                        adminEventDelay = Time.time + 0.05f;
                        Console.ExecuteCommand("tp", GetPlayerFromVRRig(thestrangled).ActorNumber, GorillaTagger.Instance.rightHandTransform.position);
                    }
                }
            }
            else
            {
                if (thestrangled != null)
                {
                    try
                    {
                        Console.ExecuteCommand("tp", GetPlayerFromVRRig(thestrangled).ActorNumber, GorillaTagger.Instance.rightHandTransform.position);
                        Console.ExecuteCommand("vel", GetPlayerFromVRRig(thestrangled).ActorNumber, GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0));
                    }
                    catch { }
                    thestrangled = null;
                    if (PhotonNetwork.InRoom)
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 89, false, 999999f);
                    else
                        VRRig.LocalRig.PlayHandTapLocal(89, false, 999999f);
                }
            }
        }

        public static void ObjectGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    adminEventDelay = Time.time + 0.1f;
                    Console.ExecuteCommand("platf", ReceiverGroup.All, NewPointer.transform.position);
                }
            }
        }

        public static void RandomObjectGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    adminEventDelay = Time.time + 0.1f;
                    Console.ExecuteCommand("platf", ReceiverGroup.All, NewPointer.transform.position, RandomVector3(), RandomVector3(360f), Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
                }
            }
        }

        private static float lastnetscale = 1f;
        private static float scalenetdel;
        private static int lastplayercount;
        public static void NetworkScale()
        {
            if (Time.time > scalenetdel && (!Mathf.Approximately(lastnetscale, VRRig.LocalRig.scaleFactor) || PhotonNetwork.PlayerList.Length != lastplayercount))
            {
                Console.ExecuteCommand("scale", ReceiverGroup.All, VRRig.LocalRig.scaleFactor);
                scalenetdel = Time.time + 0.05f;
                lastnetscale = VRRig.LocalRig.scaleFactor;
                lastplayercount = PhotonNetwork.PlayerList.Length;
            }
        }

        public static void UnNetworkScale() =>
            Console.ExecuteCommand("scale", ReceiverGroup.All, 1f);

        public static void LightningGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    adminEventDelay = Time.time + 0.1f;
                    Console.ExecuteCommand("strike", ReceiverGroup.All, NewPointer.transform.position);
                }
            }
        }

        private static int plrIndex = 0;
        public static void LightningAll()
        {
            if (Time.time > adminEventDelay)
            {
                adminEventDelay = Time.time + 0.05f;

                Console.ExecuteCommand("strike", ReceiverGroup.All, GetVRRigFromPlayer(PhotonNetwork.PlayerListOthers[plrIndex]).transform.position);

                if (plrIndex >= (PhotonNetwork.PlayerListOthers.Count() - 1))
                    plrIndex = 0;
                else
                    plrIndex++;
            }
        }

        public static void LightningOrbit()
        {
            if (Time.time > adminEventDelay)
            {
                adminEventDelay = Time.time + 0.05f;
                Console.ExecuteCommand("strike", ReceiverGroup.All, GorillaTagger.Instance.headCollider.transform.position + new Vector3(MathF.Cos((float)Time.frameCount / 30), 0.25f, MathF.Sin((float)Time.frameCount / 30)));
            }
        }

        public static void LightningAura(bool kick)
        {
            if (Time.time > adminEventDelay)
            {
                adminEventDelay = Time.time + 0.05f;

                if (kick)
                {
                    Physics.Raycast(GorillaTagger.Instance.headCollider.transform.position + new Vector3(Random.Range(-10f, 10f), 10f, Random.Range(-10f, 10f)), Vector3.down, out var Ray, 512f, NoInvisLayerMask());
                    VRRig hitRig = Ray.collider.GetComponentInParent<VRRig>();
                    if (hitRig && !hitRig.IsLocal())
                    {
                        Console.ExecuteCommand("kick", ReceiverGroup.All, GetPlayerFromVRRig(hitRig).UserId);
                    }
                    else
                    {
                        Console.ExecuteCommand("strike", ReceiverGroup.All, GorillaTagger.Instance.headCollider.transform.position + new Vector3(Random.Range(-10f, 10f), 10f, Random.Range(-10f, 10f)));
                    }
                }
                else
                {
                    Console.ExecuteCommand("strike", ReceiverGroup.All, GorillaTagger.Instance.headCollider.transform.position + new Vector3(Random.Range(-10f, 10f), 10f, Random.Range(-10f, 10f)));
                }
            }
        }

        private static Vector3 whereOriginalPlayerPos = Vector3.zero;
        private static Vector3 originalMePosition = Vector3.zero;
        public static void FearGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    TeleportPlayer(lockTarget.transform.position + lockTarget.transform.forward);
                    if (Time.time > adminEventDelay)
                        adminEventDelay = Time.time + 0.1f;
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        originalMePosition = GorillaTagger.Instance.bodyCollider.transform.position;
                        whereOriginalPlayerPos = gunTarget.transform.position;

                        int actorNumber = GetPlayerFromVRRig(gunTarget).ActorNumber;
                        Console.ExecuteCommand("platf", new[] { actorNumber, PhotonNetwork.LocalPlayer.ActorNumber }, new Vector3(0f, 16f, 0f), new Vector3(10f, 1f, 10f));
                        Console.ExecuteCommand("platf", new[] { actorNumber, PhotonNetwork.LocalPlayer.ActorNumber }, new Vector3(0f, 24f, 0f), new Vector3(10f, 1f, 10f));

                        Console.ExecuteCommand("platf", new[] { actorNumber, PhotonNetwork.LocalPlayer.ActorNumber }, new Vector3(4f, 20f, 0f), new Vector3(1f, 10f, 10f));
                        Console.ExecuteCommand("platf", new[] { actorNumber, PhotonNetwork.LocalPlayer.ActorNumber }, new Vector3(-4f, 20f, 0f), new Vector3(1f, 10f, 10f));

                        Console.ExecuteCommand("platf", new[] { actorNumber, PhotonNetwork.LocalPlayer.ActorNumber }, new Vector3(0f, 20f, 4f), new Vector3(10f, 10f, 1f));
                        Console.ExecuteCommand("platf", new[] { actorNumber, PhotonNetwork.LocalPlayer.ActorNumber }, new Vector3(0f, 20f, -4f), new Vector3(10f, 10f, 1f));

                        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Object.Destroy(platform, 60f);
                        platform.GetComponent<Renderer>().material.color = Color.black;
                        platform.transform.position = new Vector3(0f, 20f, 0f);
                        platform.transform.localScale = new Vector3(10f, 1f, 10f);

                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;

                    TeleportPlayer(originalMePosition);
                    Console.ExecuteCommand("tpnv", GetPlayerFromVRRig(lockTarget).ActorNumber, whereOriginalPlayerPos);
                    Console.ExecuteCommand("unmuteall", GetPlayerFromVRRig(lockTarget).ActorNumber);
                }
            }
        }

        // NO ADMIN INDICATOR
        public static void EnableNoAdminIndicator()
        {
            Console.ExecuteCommand("nocone", ReceiverGroup.All, true);
            lastplayercount = -1;
        }

        public static void NoAdminIndicator()
        {
            if (!PhotonNetwork.InRoom)
                lastplayercount = -1;

            if (PhotonNetwork.PlayerList.Length != lastplayercount && PhotonNetwork.InRoom)
            {
                Console.ExecuteCommand("nocone", ReceiverGroup.All, true);
                lastplayercount = PhotonNetwork.PlayerList.Length;
            }
        }

        public static void AdminIndicatorBack() =>
            Console.ExecuteCommand("nocone", ReceiverGroup.All, false);

        // NO ADMIN LOGS
        public static void EnableNoAdminCommandLogs()
        {
            Console.ExecuteCommand("nolog", ReceiverGroup.All, true);
            lastplayercount = -1;
        }

        public static void NoAdminCommandLogs()
        {
            if (!PhotonNetwork.InRoom)
                lastplayercount = -1;

            if (PhotonNetwork.PlayerList.Length != lastplayercount && PhotonNetwork.InRoom)
            {
                Console.ExecuteCommand("nolog", ReceiverGroup.All, true);
                lastplayercount = PhotonNetwork.PlayerList.Length;
            }
        }

        public static void AdminCommandLogsBack() =>
            Console.ExecuteCommand("nolog", ReceiverGroup.All, false);

        public static void EnableMenuUserTags()
        {
            if (!userTagHooked)
            {
                userTagHooked = true;
                PhotonNetwork.NetworkingClient.EventReceived += UserTagSys;
            }
        }

        private static bool lastInRoom;
        private static int lastPlayerCount = -1;

        public static bool userTagHooked;
        public static void UserTagSys(EventData data)
        {
            try
            {
                Player sender = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(data.Sender);
                if (data.Code == Console.ConsoleByte && sender != PhotonNetwork.LocalPlayer)
                {
                    object[] args = (object[])data.CustomData;
                    string command = (string)args[0];
                    switch (command)
                    {
                        case "confirmusing":
                            if (Buttons.GetIndex("Menu User Name Tags").enabled && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
                            {
                                VRRig vrrig = GetVRRigFromPlayer(sender);
                                if (!nametags.TryGetValue(vrrig, out var nametag))
                                {
                                    GameObject go = new GameObject("Seralyth_MenuUserNametag");
                                    go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                                    TextMeshPro textMesh = go.AddComponent<TextMeshPro>();
                                    textMesh.fontSize = 4.8f;
                                    textMesh.alignment = TextAlignmentOptions.Center;

                                    Color userColor = Color.red;
                                    if (args.Length > 2)
                                        userColor = Console.GetMenuTypeName((string)args[2]);

                                    textMesh.color = userColor;
                                    textMesh.text = ToTitleCase((string)args[2]);

                                    nametags.Add(vrrig, go);
                                }
                                else
                                {
                                    TextMeshPro textMesh = nametag.GetComponent<TextMeshPro>();

                                    Color userColor = Color.red;
                                    if (args.Length > 2)
                                        userColor = Console.GetMenuTypeName((string)args[2]);

                                    if (Visuals.nameTagChams)
                                        textMesh.Chams();
                                    textMesh.color = userColor;
                                    textMesh.text = ToTitleCase((string)args[2]);
                                }
                            }
                            if (Buttons.GetIndex("Conduct Menu Users").enabled)
                            {
                                if (!onConduct.ContainsKey(sender.UserId))
                                {
                                    bool add = ServerData.Administrators.ContainsKey(sender.UserId);
                                    string txt = sender.NickName + " - " + ToTitleCase((string)args[2]);
                                    if (add)
                                        txt = "<color=red>" + txt + "</color>";
                                    onConduct.Add(sender.UserId, txt);
                                }
                            }
                            if (Buttons.GetIndex("Admin Find User").enabled)
                                isUserFound = true;
                            break;
                    }
                }
            }
            catch { }
        }

        private static readonly Dictionary<VRRig, GameObject> nametags = new Dictionary<VRRig, GameObject>();
        public static void MenuUserTags()
        {
            if (PhotonNetwork.InRoom && (!lastInRoom || PhotonNetwork.PlayerList.Length != lastPlayerCount))
                Console.ExecuteCommand("isusing", ReceiverGroup.All);

            lastInRoom = PhotonNetwork.InRoom;
            lastPlayerCount = PhotonNetwork.PlayerList.Length;
            if (!PhotonNetwork.InRoom)
                lastPlayerCount = -1;

            foreach (KeyValuePair<VRRig, GameObject> nametag in nametags.ToList())
            {
                if (!VRRigCache.ActiveRigs.Contains(nametag.Key))
                {
                    Object.Destroy(nametag.Value);
                    nametags.Remove(nametag.Key);
                }
                else
                {
                    nametag.Value.GetComponent<TextMeshPro>().fontStyle = activeFontStyle;
                    nametag.Value.GetComponent<TextMeshPro>().font = activeFont;

                    if (Visuals.nameTagChams)
                        nametag.Value.GetComponent<TextMeshPro>().Chams();

                    nametag.Value.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f) * nametag.Key.scaleFactor;

                    nametag.Value.transform.position = Visuals.GetNameTagPosition(nametag.Key);
                    nametag.Value.transform.LookAt(Camera.main.transform.position);
                    nametag.Value.transform.Rotate(0f, 180f, 0f);
                }
            }
        }

        public static void DisableMenuUserTags()
        {
            foreach (KeyValuePair<VRRig, GameObject> nametag in nametags)
                Object.Destroy(nametag.Value);

            nametags.Clear();
        }

        public static bool tracerTagHooked;
        public static void EnableMenuUserTracers()
        {
            if (!tracerTagHooked)
            {
                tracerTagHooked = true;
                PhotonNetwork.NetworkingClient.EventReceived += TracerSys;
            }
        }

        private static readonly Dictionary<VRRig, string> menuUsers = new Dictionary<VRRig, string>();
        public static void TracerSys(EventData data)
        {
            try
            {
                Player sender = PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(data.Sender);
                if (data.Code == Console.ConsoleByte && sender != PhotonNetwork.LocalPlayer)
                {
                    object[] args = (object[])data.CustomData;
                    string command = (string)args[0];
                    switch (command)
                    {
                        case "confirmusing":
                            if (ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
                            {
                                VRRig vrrig = GetVRRigFromPlayer(sender);
                                if (!nametags.TryGetValue(vrrig, out var nametag))
                                {
                                    GameObject go = new GameObject("Seralyth_Nametag");
                                    go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                                    TextMeshPro textMesh = go.AddComponent<TextMeshPro>();
                                    textMesh.fontSize = 48;
                                    textMesh.alignment = TextAlignmentOptions.Center;

                                    Color userColor = Color.red;
                                    if (args.Length > 2)
                                        userColor = Console.GetMenuTypeName((string)args[2]);

                                    textMesh.color = userColor;
                                    textMesh.text = ToTitleCase((string)args[2]);

                                    nametags.Add(vrrig, go);
                                }
                                else
                                {
                                    TextMeshPro textMesh = nametag.GetComponent<TextMeshPro>();

                                    Color userColor = Color.red;
                                    if (args.Length > 2)
                                        userColor = Console.GetMenuTypeName((string)args[2]);

                                    textMesh.color = userColor;
                                    textMesh.text = ToTitleCase((string)args[2]);
                                }
                            }
                            break;
                    }
                }
            }
            catch { }
        }

        public static void MenuUserTracers()
        {
            if (PhotonNetwork.InRoom && (!lastInRoom || PhotonNetwork.PlayerList.Length != lastPlayerCount))
                Console.ExecuteCommand("isusing", ReceiverGroup.All);

            lastInRoom = PhotonNetwork.InRoom;
            lastPlayerCount = PhotonNetwork.PlayerList.Length;
            if (!PhotonNetwork.InRoom)
                lastPlayerCount = -1;

            if (Visuals.DoPerformanceCheck())
                return;

            bool followMenuTheme = Buttons.GetIndex("Follow Menu Theme").enabled;
            bool transparentTheme = Buttons.GetIndex("Transparent Theme").enabled;
            _ = Buttons.GetIndex("Hidden on Camera").enabled;
            float lineWidth = (Buttons.GetIndex("Thin Tracers").enabled ? 0.0075f : 0.025f) * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f);

            Color menuColor = backgroundColor.GetCurrentColor();

            foreach (KeyValuePair<VRRig, string> userData in menuUsers)
            {
                VRRig playerRig = userData.Key;
                if (playerRig.isLocal)
                    continue;

                Color lineColor = Console.GetMenuTypeName(userData.Value);

                LineRenderer line = Visuals.GetLineRender();

                if (followMenuTheme)
                    lineColor = menuColor;

                if (transparentTheme)
                    lineColor.a = 0.5f;

                line.startColor = lineColor;
                line.endColor = lineColor;
                line.startWidth = lineWidth;
                line.endWidth = lineWidth;
                line.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                line.SetPosition(1, playerRig.transform.position);
            }
        }

        public static readonly Dictionary<string, string> onConduct = new Dictionary<string, string>();
        public static void ConsoleOnConduct()
        {
            if (PhotonNetwork.InRoom && (!lastInRoom || PhotonNetwork.PlayerList.Length != lastPlayerCount) && !Buttons.GetIndex("Menu User Name Tags").enabled)
                Console.ExecuteCommand("isusing", ReceiverGroup.All);

            string conductText = "";
            conductText += "<color=red>" + PhotonNetwork.LocalPlayer.NickName + " - " + ToTitleCase(Console.MenuName) + "</color>\\n";
            foreach (KeyValuePair<string, string> item in onConduct)
            {
                if (GetPlayerFromID(item.Key) == null)
                    onConduct.Remove(item.Key);
                else
                    conductText += item.Value + "\\n";
            }
            GetObject("Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData").GetComponent<TextMeshPro>().text = conductText;
        }

        public static float FindUserTime;
        public static bool isUserFound;
        public static void FindUser()
        {
            if (Time.time < FindUserTime)
                return;

            if (!PhotonNetwork.InRoom)
            {
                Important.JoinRandom();
                isUserFound = false;
                FindUserTime = Time.time + 7f;
            }
            else
            {
                if (isUserFound)
                {
                    NotificationManager.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Found menu user!");
                    Buttons.GetIndex("Admin Find User").enabled = false;
                    isUserFound = false;
                    return;
                }
                NotificationManager.SendNotification("Nobody found, searching for players.");
                NetworkSystem.Instance.ReturnToSinglePlayer();
                FindUserTime = Time.time + 2f;
            }
        }

        private static float thingdeb;
        public static void PunchMod()
        {
            if (Time.time > thingdeb)
            {
                foreach (VRRig rig in VRRigCache.ActiveRigs)
                {
                    bool leftHand = Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, rig.headMesh.transform.position) < 0.25f;
                    bool rightHand = Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, rig.headMesh.transform.position) < 0.25f;

                    if (!rig.isLocal && (leftHand || rightHand))
                    {
                        Vector3 vel = rightHand ? GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0) : GTPlayer.Instance.LeftHand.velocityTracker.GetAverageVelocity(true, 0);

                        Console.ExecuteCommand("vel", GetPlayerFromVRRig(rig).ActorNumber, vel);
                        thingdeb = Time.time + 0.1f;
                    }
                }
            }
        }

        public static string targetRoom;
        public static void GetTargetRoom() =>
            PromptText("What room would you like the users to join?", () => targetRoom = keyboardInput, null, "Done", "Cancel");

        public static void JoinGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.1f;
                        Console.ExecuteCommand("join", GetPlayerFromVRRig(gunTarget).ActorNumber, targetRoom.ToUpper());
                    }
                }
            }
        }

        public static void JoinAll() =>
            PromptText("What room would you like the users to join?", () => Console.ExecuteCommand("join", ReceiverGroup.Others, keyboardInput.ToUpper()), null, "Done", "Cancel");

        public static string targetNotification;
        public static void GetTargetNotification()
        {
            PromptText("What notification would you like to send?", () =>
            {
                targetNotification = keyboardInput;
                Buttons.GetIndex("NotifLabel").overlapText = "Notification: " + keyboardInput;
            }, null, "Done", "Cancel");
        }

        public static void NotifySelf() =>
            Console.ExecuteCommand("notify", PhotonNetwork.LocalPlayer.ActorNumber, targetNotification);

        public static void NotifyGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.1f;
                        Console.ExecuteCommand("notify", GetPlayerFromVRRig(gunTarget).ActorNumber, targetNotification);
                    }
                }
            }
        }

        public static void NotifyAll() =>
            Console.ExecuteCommand("notify", ReceiverGroup.All, targetNotification);

        private static bool lastLasering;
        public static void Laser(bool kick)
        {
            if (leftPrimary || rightPrimary)
            {
                Vector3 dir = rightPrimary ? VRRig.LocalRig.rightHandTransform.right : -VRRig.LocalRig.leftHandTransform.right;
                Vector3 startPos = (rightPrimary ? VRRig.LocalRig.rightHandTransform.position : VRRig.LocalRig.leftHandTransform.position) + dir * 0.1f;
                try
                {
                    Physics.Raycast(startPos + dir / 3f, dir, out var Ray, 512f, NoInvisLayerMask());
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && kick)
                        Console.ExecuteCommand("silkick", ReceiverGroup.All, GetPlayerFromVRRig(gunTarget).UserId);
                }
                catch { }
                if (Time.time > adminEventDelay)
                {
                    adminEventDelay = Time.time + 0.1f;
                    Console.ExecuteCommand("laser", ReceiverGroup.All, true, rightPrimary);
                }
            }
            bool isLasering = leftPrimary || rightPrimary;
            if (lastLasering && !isLasering)
                Console.ExecuteCommand("laser", ReceiverGroup.All, false, false);

            lastLasering = isLasering;
        }

        private static float beamDelay;
        public static void Beam()
        {
            if (rightTrigger > 0.5f && Time.time > beamDelay)
            {
                beamDelay = Time.time + 0.05f;
                float h = Time.frameCount / 180f % 1f;
                Color color = Color.HSVToRGB(h, 1f, 1f);
                Console.ExecuteCommand("lr", ReceiverGroup.All, color.r, color.g, color.b, color.a, 0.5f, GorillaTagger.Instance.headCollider.transform.position + new Vector3(0f, 0.5f, 0f), GorillaTagger.Instance.headCollider.transform.position + new Vector3(Mathf.Cos((float)Time.frameCount / 30) * 100f, 0.5f, Mathf.Sin((float)Time.frameCount / 30) * 100f), 0.1f);
            }
        }

        private static float startTimeTrigger;
        private static bool lastTriggerLaserSpam;
        public static void Fractals()
        {
            if (rightTrigger > 0.5f && !lastTriggerLaserSpam)
                startTimeTrigger = Time.time;

            lastTriggerLaserSpam = rightTrigger > 0.5f;

            if (rightTrigger > 0.5f && Time.time > beamDelay)
            {
                beamDelay = Time.time + 0.5f;
                float h = Time.frameCount / 180f % 1f;
                Color.HSVToRGB(h, 1f, 1f);
                Console.ExecuteCommand("lr", ReceiverGroup.All, "lr", 0f, 1f, 1f, 0.3f, 0.25f, GorillaTagger.Instance.bodyCollider.transform.position, GorillaTagger.Instance.headCollider.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * 1000f, 20f - (Time.time - startTimeTrigger));
            }
        }

        public static void FlyAllUsing()
        {
            if (Time.time > adminEventDelay)
            {
                adminEventDelay = Time.time + 0.05f;
                Console.ExecuteCommand("vel", ReceiverGroup.Others, new Vector3(0f, 10f, 0f));
            }
        }

        public static void BouncyAllUsing()
        {
            if (Time.time > adminEventDelay)
            {
                adminEventDelay = Time.time + 0.05f;

                var users = Console.userDictionary.Keys.Where(u => !u.IsLocal).ToList();

                foreach (var rig in users.Select(player => GetVRRigFromPlayer(player)))
                {
                    if (!Physics.Raycast(rig.bodyTransform.position - new Vector3(0f, 0.2f, 0f), Vector3.down,
                            out RaycastHit hit, 512f, GTPlayer.Instance.locomotionEnabledLayers)) continue;
                    if (!(hit.distance < 0.1f)) continue;
                    Vector3 surfaceNormal = hit.normal;
                    Vector3 bodyVelocity = rig.LatestVelocity();
                    Vector3 reflectedVelocity = Vector3.Reflect(bodyVelocity, surfaceNormal);
                    Vector3 finalVelocity = reflectedVelocity * 2f;
                    Console.ExecuteCommand("vel", rig.GetPlayer().ActorNumber, finalVelocity);
                }
            }
        }

        public static void BringGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.1f;
                        Console.ExecuteCommand("tpnv", GetPlayerFromVRRig(gunTarget).ActorNumber, GorillaTagger.Instance.headCollider.transform.position + new Vector3(0f, 1.5f, 0f));
                    }
                }
            }
        }

        public static void BringAllUsing()
        {
            if (Time.time > adminEventDelay)
            {
                adminEventDelay = Time.time + 0.05f;
                Console.ExecuteCommand("tpnv", ReceiverGroup.Others, GorillaTagger.Instance.headCollider.transform.position + new Vector3(0f, 1.5f, 0f));
            }
        }

        public static void OrganizeGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    var users = Console.userDictionary.Keys.Where(u => !u.IsLocal).ToList();
                    if (users.Count == 1)
                    {
                        Console.ExecuteCommand("tpnv", users.FirstOrDefault().ActorNumber, NewPointer.transform.position);
                        return;
                    }

                    float spacing = 0.8f;
                    for (int i = 0; i < users.Count; i++)
                    {
                        Console.ExecuteCommand("tpnv", users[i].ActorNumber, NewPointer.transform.position - Vector3.right * ((users.Count - 1) * spacing / 2f) + Vector3.right * (spacing * i));
                    }
                    adminEventDelay = Time.time + 0.05f;
                }
            }
        }

        public static void BringHandAllUsing()
        {
            if (Time.time > adminEventDelay)
            {
                adminEventDelay = Time.time + 0.05f;
                Console.ExecuteCommand("tpnv", ReceiverGroup.Others, ControllerUtilities.GetTrueRightHand().position + ControllerUtilities.GetTrueRightHand().forward);
            }
        }

        public static void BringHeadAllUsing()
        {
            if (Time.time > adminEventDelay)
            {
                adminEventDelay = Time.time + 0.05f;
                Console.ExecuteCommand("tpnv", ReceiverGroup.Others, GorillaTagger.Instance.headCollider.transform.position + GorillaTagger.Instance.headCollider.transform.forward);
            }
        }

        public static void OrbitAllUsing()
        {
            if (Time.time > adminEventDelay)
            {
                adminEventDelay = Time.time + 0.05f;
                Console.ExecuteCommand("tpnv", ReceiverGroup.Others, GorillaTagger.Instance.headCollider.transform.position + new Vector3(Mathf.Cos(Time.frameCount / 20f), 0.5f, Mathf.Sin(Time.frameCount / 20f)));
            }
        }

        public static void SafetyBubble()
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (!rig.isLocal)
                {
                    if (Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, rig.transform.position) < 3f)
                    {
                        Console.ExecuteCommand("vel", GetPlayerFromVRRig(rig).ActorNumber, (rig.transform.position - GorillaTagger.Instance.bodyCollider.transform.position).normalized * 75f);
                    }
                }
            }
        }

        public static void ConfirmNotifyAllUsing() =>
            Console.ExecuteCommand("notify", ReceiverGroup.All, ServerData.Administrators[PhotonNetwork.LocalPlayer.UserId] == "PixelCatt" ? "Yes, I'm PixelCatt. I made this Menu." : "Yes, I'm " + ServerData.Administrators[PhotonNetwork.LocalPlayer.UserId] + ". I'm a Console Admin.");

        public static int[] oldCosmetics;
        public static int[] oldTryOn;
        public static void SpoofCosmetics(bool forceRun = false)
        {
            if (PhotonNetwork.InRoom)
            {
                if (oldCosmetics != CosmeticsController.instance.currentWornSet.ToPackedIDArray() || forceRun)
                {
                    oldCosmetics = CosmeticsController.instance.currentWornSet.ToPackedIDArray();
                    string[] cosmetics = CosmeticsController.instance.currentWornSet.ToDisplayNameArray().Where(c => !string.Equals(c, "NOTHING", StringComparison.OrdinalIgnoreCase)).ToArray();

                    Console.ExecuteCommand("cosmetics", ReceiverGroup.Others, cosmetics);
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_UpdateCosmeticsWithTryonPacked", RpcTarget.Others, CosmeticsController.instance.currentWornSet.ToPackedIDArray(), CosmeticsController.instance.tryOnSet.ToPackedIDArray(), false);
                }
            }
        }

        public static void OnPlayerJoinSpoof(NetPlayer player)
        {
            string[] cosmetics = CosmeticsController.instance.currentWornSet.ToDisplayNameArray().Where(c => !string.Equals(c, "NOTHING", StringComparison.OrdinalIgnoreCase)).ToArray();

            Console.ExecuteCommand("cosmetics", new[] { player.ActorNumber }, cosmetics);
            GorillaTagger.Instance.myVRRig.SendRPC("RPC_UpdateCosmeticsWithTryonPacked", RpcTarget.Others, CosmeticsController.instance.currentWornSet.ToPackedIDArray(), CosmeticsController.instance.tryOnSet.ToPackedIDArray(), false);
        }

        public static async Task SpawnNuke()
        {
            int nukeAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "donationnuke", "plsdonatenuke", nukeAssetID);
            Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, nukeAssetID, new Vector3(-64.16f, 2.99f, -82.07f));
            Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, nukeAssetID, "nuke", "nukesound");
            await Task.Delay(15000);
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, nukeAssetID);
        }

        private static int modMenuAssetID;
        public static void SpawnModMenu()
        {
            modMenuAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "clickbaitmenu", "Mod Menu", modMenuAssetID);
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, modMenuAssetID, 2);
            Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, modMenuAssetID, new Vector3(-0.09f, 0.125f, 0f));
            Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, modMenuAssetID, Quaternion.Euler(0f, 110f, 80f));
        }

        public static void RemoveModMenu()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, modMenuAssetID);
        }

        private static float spawnModMenuDelay = 0f;
        public static void ModMenu()
        {
            if (rightGrab)
            {
                if (Time.time > spawnModMenuDelay && !Console.consoleAssets.ContainsKey(modMenuAssetID))
                {
                    spawnModMenuDelay = Time.time + 0.1f;
                    RemoveModMenu();
                    SpawnModMenu();
                }
            }
            else
            {
                RemoveModMenu();
            }
        }

        private static int cheeseburgerAssetID;
        public static void SpawnCheeseburger()
        {
            cheeseburgerAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "effects", "rblxcheezburger", cheeseburgerAssetID);
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, cheeseburgerAssetID, 2);
            Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, cheeseburgerAssetID, "Sound", "canihaveachezburger");
        }

        public static void RemoveCheeseburger()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, cheeseburgerAssetID);
        }

        private static float spawnCheeseburgerDelay = 0f;
        private static float eatCheeseburgerDelay = 0f;
        public static void Cheeseburger()
        {
            if (rightGrab)
            {
                if (Time.time > spawnCheeseburgerDelay && !Console.consoleAssets.ContainsKey(cheeseburgerAssetID))
                {
                    spawnCheeseburgerDelay = Time.time + 0.1f;
                    RemoveCheeseburger();
                    SpawnCheeseburger();
                }

                if ((Time.time > eatCheeseburgerDelay) && VRRigCache.m_activeRigs.Any(rig => Vector3.Distance(rig.headMesh.transform.position, GorillaTagger.Instance.offlineVRRig.rightHandTransform.position) <= 0.4f))
                {
                    Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, cheeseburgerAssetID, "Sound", "mmmchezburger");
                    eatCheeseburgerDelay = Time.time + 1f;
                }
            }
            else
            {
                RemoveCheeseburger();
            }
        }

        private static int rubberDuckAssetID;
        public static void SpawnRubberDuck()
        {
            rubberDuckAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "clickbaitmenu", "Duck", rubberDuckAssetID);
            Console.ExecuteCommand("asset-destroycolliders", ReceiverGroup.All, rubberDuckAssetID);
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, rubberDuckAssetID, 2);
            Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, rubberDuckAssetID, new Vector3(0.001f, 0.001f, 0.001f));
            Console.ExecuteCommand("asset-setphysics", ReceiverGroup.All, rubberDuckAssetID, false);
            Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, rubberDuckAssetID, new Vector3(0.1f, 0.05f, 0.05f));
            Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, rubberDuckAssetID, Quaternion.Euler(0f, 0f, 160f));
        }

        public static void RemoveRubberDuck()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, rubberDuckAssetID);
        }

        private static float spawnRubberDuckDelay = 0f;
        public static void RubberDuck()
        {
            if (rightGrab)
            {
                if (Time.time > spawnRubberDuckDelay && !Console.consoleAssets.ContainsKey(rubberDuckAssetID))
                {
                    spawnRubberDuckDelay = Time.time + 0.1f;
                    RemoveRubberDuck();
                    SpawnRubberDuck();
                }
            }
            else
            {
                RemoveRubberDuck();
            }
        }

        private static int miniTravisAssetID;
        public static void SpawnMiniTravis()
        {
            miniTravisAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "minitravis", "travisscott", miniTravisAssetID);
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, miniTravisAssetID, 2);
            Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, miniTravisAssetID, new Vector3(-0.6f, 0.2f, 0f));
            Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, miniTravisAssetID, Quaternion.Euler(80f, 160f, 180f));
        }

        public static void RemoveMiniTravis()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, miniTravisAssetID);
        }

        private static float spawnMiniTravisDelay = 0f;
        public static void MiniTravis()
        {
            if (rightGrab)
            {
                if (Time.time > spawnMiniTravisDelay && !Console.consoleAssets.ContainsKey(miniTravisAssetID))
                {
                    spawnMiniTravisDelay = Time.time + 0.1f;
                    RemoveMiniTravis();
                    SpawnMiniTravis();
                }
            }
            else
            {
                RemoveMiniTravis();
            }
        }

        private static int travisEventAssetID;
        public static void SpawnTravisEvent()
        {
            travisEventAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "travis", "travisscott", travisEventAssetID);
            Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, travisEventAssetID, new Vector3(-70f, 2f, -52f));
            Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, travisEventAssetID, Vector3.one * 0.38f);
        }

        public static void RemoveTravisEvent()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, travisEventAssetID);
        }

        private static int twerkingCartiAssetID;
        public static void SpawnTwerkingCarti()
        {
            twerkingCartiAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "carti", twerkingCartiAssetID);
            Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, twerkingCartiAssetID, new Vector3(-76f, 1.7f, -80f));
            Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, twerkingCartiAssetID, Quaternion.Euler(0f, 40f, 0f));
            Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, twerkingCartiAssetID, Vector3.one * 5f);
        }

        public static void RemoveTwerkingCarti()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, twerkingCartiAssetID);
        }

        private static int shrekSaxophoneAssetID;
        public static void SpawnShrekSaxophone()
        {
            shrekSaxophoneAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "shrek", shrekSaxophoneAssetID);
            Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, shrekSaxophoneAssetID, new Vector3(-76f, 1.7f, -80f));
            Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, shrekSaxophoneAssetID, Quaternion.Euler(0f, 40f, 0f));
            Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, shrekSaxophoneAssetID, Vector3.one * 5f);
        }

        public static void RemoveShrekSaxophone()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, shrekSaxophoneAssetID);
        }

        private static int pistolAssetID;
        public static void SpawnPistol()
        {
            pistolAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "console.main1", "Pistol", pistolAssetID);
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, pistolAssetID, 2);
        }

        public static bool pistolFling = false;
        public static bool pistolKick = false;
        public static async Task ShootPistol()
        {
            Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, pistolAssetID, "Model", "PistolShoot");
            Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, pistolAssetID, "Model", "Shoot");

            var (_, _, up, forward, right) =
                SwapGunHand
                ? ControllerUtilities.GetTrueLeftHand()
                : ControllerUtilities.GetTrueRightHand();

            Vector3 startPosition =
                (SwapGunHand
                ? GorillaTagger.Instance.leftHandTransform
                : GorillaTagger.Instance.rightHandTransform).position;

            Vector3 direction = forward;

            Physics.Raycast(
                startPosition + direction * 0.25f,
                direction,
                out RaycastHit Ray,
                512f,
                NoInvisLayerMask()
            );

            Vector3 position = Ray.point;

            if (position == Vector3.zero)
                position = startPosition + direction * 512f;

            int explosionAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "btools", "Explosion", explosionAssetID);
            Console.ExecuteCommand("asset-stopsound", ReceiverGroup.All, explosionAssetID, "Sound");
            Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, explosionAssetID, new Vector3(0.1f, 0.1f, 0.1f));
            Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, explosionAssetID, position);

            Task.Run(async () =>
            {
                await Task.Delay(1000);
                Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, explosionAssetID);
            });

            if (pistolFling || pistolKick)
            {
                VRRig gunTarget = Ray.collider != null
                    ? Ray.collider.GetComponentInParent<VRRig>()
                    : null;

                if (gunTarget != null && !gunTarget.IsLocal())
                {
                    if (pistolFling)
                    {
                        Console.ExecuteCommand("vel", GetPlayerFromVRRig(gunTarget).ActorNumber, new Vector3(0f, 50f, 0f));
                    }

                    if (pistolKick)
                    {
                        Console.ExecuteCommand("silkick", ReceiverGroup.All, GetPlayerFromVRRig(gunTarget).UserId);
                    }
                }
            }

            await Task.Delay(2000);

            Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, pistolAssetID, "Model", "Default");
        }

        private static float spawnPistolDelay = 0f;
        private static float shootPistolDelay = 0f;
        public static void Pistol()
        {
            if (rightGrab)
            {
                if (Time.time > spawnPistolDelay && !Console.consoleAssets.ContainsKey(pistolAssetID))
                {
                    spawnPistolDelay = Time.time + 0.1f;
                    RemovePistol();
                    SpawnPistol();
                }

                if (Time.time > shootPistolDelay && rightTrigger > 0.25f)
                {
                    shootPistolDelay = Time.time + 2.5f;
                    ShootPistol();
                }
            }
            else
            {
                RemovePistol();
            }
        }

        public static void RemovePistol()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, pistolAssetID);
        }

        private static (string Asset, string Bundle)[] Weapons =
        {
            ("Sword",    "consolehamburburassets"),
            ("Sword",    "console.main1"),
            ("Sword",    "rbsword"),
            ("Axe",      "consolehamburburassets"),
            ("Karambit", "karambit")
        };

        private static int selectedWeaponIndex = 0;
        public static string selectedWeapon = "Sword";
        public static void ChangeWeapon(bool positive = true)
        {
            string[] SwordNames = {
                "Sword",
                "Roblox Sword",
                "Rainbow Sword",
                "Axe",
                "Karambit"
            };

            if (positive)
                selectedWeaponIndex++;
            else
                selectedWeaponIndex--;

            selectedWeaponIndex %= SwordNames.Length;
            if (selectedWeaponIndex < 0)
                selectedWeaponIndex = SwordNames.Length - 1;

            selectedWeapon = SwordNames[selectedWeaponIndex];

            Buttons.GetIndex("Admin Weapon Selector").overlapText = "Weapon: " + selectedWeapon;
        }

        private static int weaponAssetID;
        public static void SpawnWeapon()
        {
            weaponAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, Weapons[selectedWeaponIndex].Bundle, Weapons[selectedWeaponIndex].Asset, weaponAssetID);
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, weaponAssetID, 2);

            switch (selectedWeaponIndex)
            {
                case 0:
                    Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, weaponAssetID, new Vector3(0.1f, 0.1f, 0.2f));
                    Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, weaponAssetID, Quaternion.Euler(0f, 90f, 90f));
                    Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, weaponAssetID, Vector3.one * 0.075f);
                    break;

                case 1:
                    Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, weaponAssetID, "Model", "Unsheath");
                    break;

                case 2:
                    Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, weaponAssetID, "Sword", "Music");
                    Console.ExecuteCommand("asset-setvolume", ReceiverGroup.All, weaponAssetID, "Sword", 0.5f);
                    break;

                case 3:
                    Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, weaponAssetID, new Vector3(0.05f, 0.03f, 0f));
                    Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, weaponAssetID, Quaternion.Euler(0f, 0f, 90f));
                    Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, weaponAssetID, Vector3.one * 5);
                    break;

                case 4:
                    Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, weaponAssetID, new Vector3(0.045f, 0.065f, 0f));
                    Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, weaponAssetID, Quaternion.Euler(270f, 60f, 0f));
                    Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, weaponAssetID, "Collider", "csgo knife");
                    break;

                default:
                    break;
            }
        }

        public static void RemoveWeapon()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, weaponAssetID);
        }

        public static bool weaponFling = false;
        public static bool weaponKick = false;
        private static bool weaponVelTooHigh = false;
        private static bool lastWeaponVelTooHigh = false;
        private static float swingWeaponDelay = 0f;
        public static async Task SwingWeapon()
        {
            weaponVelTooHigh = (GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0) - GorillaTagger.Instance.rigidbody.linearVelocity).magnitude > 10f;

            if (weaponVelTooHigh && !lastWeaponVelTooHigh)
            {
                switch (selectedWeaponIndex)
                {
                    case 1:
                        Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, weaponAssetID, "Model", "Slash");
                        break;

                    case 2:
                        Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, weaponAssetID, "Sword/SFX", $"Swing{Random.Range(1, 3)}");
                        break;

                    case 4:
                        Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, weaponAssetID, "Stab", "csgo knife");
                        break;

                    default:
                        break;
                }
            }

            lastWeaponVelTooHigh = weaponVelTooHigh;
            swingWeaponDelay = Time.time + 0.25f;
        }

        private static float spawnWeaponDelay = 0f;
        public static void Weapon()
        {
            if (rightGrab)
            {
                if (Time.time > spawnWeaponDelay && !Console.consoleAssets.ContainsKey(weaponAssetID))
                {
                    spawnWeaponDelay = Time.time + 0.1f;
                    RemoveWeapon();
                    SpawnWeapon();
                }

                if (Time.time > swingWeaponDelay)
                {
                    SwingWeapon();
                }
            }
            else
            {
                RemoveWeapon();
            }
        }

        private static int jailAssetID;
        public static void SpawnJail()
        {
            jailAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "jailcell", "jail", jailAssetID);
        }

        public static void RemoveJail()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, jailAssetID);
        }

        public static void JailGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > adminEventDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        adminEventDelay = Time.time + 0.1f;

                        if (!Console.consoleAssets.ContainsKey(jailAssetID))
                        {
                            SpawnJail();
                        }

                        Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, jailAssetID, gunTarget.transform.position + new Vector3(-1f, -3f, -18f));
                    }
                }
            }
        }

        private static Console.ConsoleAsset movingObject;
        private static float moveUpdateCooldown;
        private static bool lastGripBtools;
        private static int toolId;
        private static bool lastTrigger;
        private static int lastToolId = -1;
        public static void BTools()
        {
            bool gripPressed = SwapGunHand ? rightGrab : leftGrab;

            if (gripPressed && !lastGripBtools)
                toolId++;

            toolId %= 3;
            lastGripBtools = gripPressed;

            if (toolId != lastToolId)
            {
                string mode = toolId switch
                {
                    0 => "Move",
                    1 => "Clone",
                    2 => "Destroy",
                    _ => "Unknown"
                };

                NotificationManager.SendNotification($"<color=grey>[</color><color=blue>B-TOOLS</color><color=grey>]</color> Selected Mode: {mode}.", 10000);
                lastToolId = toolId;
            }

            if (!GetGunInput(false))
                return;

            var gunData = RenderGun(NoInvisLayerMask());
            GameObject pointer = gunData.NewPointer;

            if (pointer == null)
                return;

            Vector3 endPos = pointer.transform.position;

            Console.ConsoleAsset targetObject = Console.consoleAssets.Values.FirstOrDefault(asset => asset.assetObject != null && Vector3.Distance(asset.assetObject.transform.position, endPos) < 1f);

            switch (toolId)
            {
                case 0: // Move

                    if (GetGunInput(true))
                    {
                        if (movingObject == null && targetObject != null)
                            movingObject = targetObject;

                        if (movingObject != null)
                        {
                            if (Time.time > moveUpdateCooldown)
                            {
                                moveUpdateCooldown = Time.time + 0.05f;

                                Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, movingObject.assetId, endPos + Vector3.up * 0.2f);
                            }
                        }
                    }
                    else
                    {
                        movingObject = null;
                    }

                    break;

                case 1: // Clone

                    if (targetObject != null)
                    {
                        if (GetGunInput(true) && !lastTrigger)
                        {
                            int cloneId = Console.GetFreeAssetID();

                            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, targetObject.assetBundle, targetObject.assetName, cloneId);
                            Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, cloneId, targetObject.assetObject.transform.position + Vector3.up);
                            Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, cloneId, targetObject.assetObject.transform.rotation);
                            Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, cloneId, targetObject.assetObject.transform.localScale);
                        }
                    }

                    break;

                case 2: // Destroy

                    if (targetObject != null)
                    {
                        if (GetGunInput(true) && !lastTrigger)
                        {
                            Vector3 position = targetObject.assetObject.transform.position;

                            int explosionAssetID = Console.GetFreeAssetID();

                            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "btools", "Explosion", explosionAssetID);
                            Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, explosionAssetID, position);
                            Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, explosionAssetID, "Sound", "Explode");

                            Task.Run(async () =>
                            {
                                await Task.Delay(1000);

                                Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, explosionAssetID);
                            });

                            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, targetObject.assetId);
                        }
                    }

                    break;
            }

            lastTrigger = GetGunInput(true);
        }

        public static void RemoveBTools()
        {
            movingObject = null;
            toolId = 0;
            lastToolId = -1;
        }

        private static string AssetsEndpoint = "https://cdn.pixelcatt.workers.dev/";
        private static readonly HttpClient VideoLoadingHttpClient = new HttpClient();
        private static Dictionary<string, string> Videos = new Dictionary<string, string>();
        public static async Task LoadVideoDictionary()
        {
            var response = await VideoLoadingHttpClient.GetAsync(AssetsEndpoint + "Videos");

            if (!response.IsSuccessStatusCode)
                return;

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());

            var files = json["Files"];
            if (files.Count() < 1)
                return;

            Videos.Clear();

            foreach (var file in files)
            {
                foreach (var prop in file.Children<JProperty>())
                {
                    string name = prop.Name;
                    string path = prop.Value.Value<string>();

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path))
                        continue;

                    Videos[name] = AssetsEndpoint + path;
                }
            }
        }

        private static string selectedVideoURL;
        public static void SelectVideo(string videoName, string videoUrl)
        {
            Buttons.GetIndex("VideoLabel").overlapText = "Video: " + videoName;
            selectedVideoURL = videoUrl;

            if (Console.consoleAssets.ContainsKey(videoPlayerAssetID))
                Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, videoPlayerAssetID, "Video", selectedVideoURL);

            if (Console.consoleAssets.ContainsKey(phoneAssetID))
                Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, phoneAssetID, "VideoPlayer", selectedVideoURL);

            if (Console.consoleAssets.ContainsKey(televisionAssetID))
                Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, televisionAssetID, "VideoPlayer", selectedVideoURL);
        }

        private static bool loadedVideoButtons = false;
        public static async Task OpenVideoSelector(bool forceReload)
        {
            Buttons.CurrentCategoryName = "Admin Video Selector";

            if (!loadedVideoButtons || forceReload)
            {
                List<ButtonInfo> loadingButtons = new List<ButtonInfo> {
                    new ButtonInfo { buttonText = "Exit Admin Video Selector", method =() => Buttons.CurrentCategoryName = "Admin Mods", isTogglable = false, toolTip = "Returns you back to the Admin Mods."},

                    new ButtonInfo { buttonText = "Loading Videos...", label = true },
                };
                Buttons.buttons[53] = loadingButtons.ToArray();
                Main.ReloadMenu();

                try { await LoadVideoDictionary(); } catch { Videos.Clear(); }

                if (Videos.Count > 0)
                {
                    List<ButtonInfo> videoButtons = new List<ButtonInfo> {
                        new ButtonInfo { buttonText = "Exit Admin Video Selector", method = () => Buttons.CurrentCategoryName = "Admin Mods", isTogglable = false, toolTip = "Returns you back to the Admin Mods." },
                        new ButtonInfo { buttonText = "Reload Videos", method = () => AdminMods.OpenVideoSelector(true), isTogglable = false, toolTip = "Reloads the Videos in the Video Selector." }
                    };

                    foreach (var video in Videos)
                    {
                        string videoName = Path.GetFileNameWithoutExtension(video.Key);
                        string videoUrl = video.Value;

                        videoButtons.Add(new ButtonInfo
                        {
                            buttonText = videoName,
                            method = () =>
                            {
                                AdminMods.SelectVideo(videoName, videoUrl);
                            },
                            isTogglable = false,
                            hideFromSearch = true,
                            toolTip = $"Selects the Video: {videoName} to be used on the VideoPlayer."
                        });
                    }

                    Buttons.buttons[53] = videoButtons.ToArray();
                    Main.ReloadMenu();

                    loadedVideoButtons = true;
                }
                else
                {
                    List<ButtonInfo> failedButtons = new List<ButtonInfo> {
                    new ButtonInfo { buttonText = "Exit Admin Video Selector", method =() => Buttons.CurrentCategoryName = "Admin Mods", isTogglable = false, toolTip = "Returns you back to the Admin Mods."},

                    new ButtonInfo { buttonText = "Failed to Load Videos!", label = true },
                    new ButtonInfo { buttonText = "Try Again", method = () => AdminMods.OpenVideoSelector(true), isTogglable = false, toolTip = "Reloads the Videos in the Video Selector." }
                };
                    Buttons.buttons[53] = failedButtons.ToArray();
                    Main.ReloadMenu();
                }
            }
        }

        private static int videoPlayerAssetID;
        public static void PlayVideoHand()
        {
            videoPlayerAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "console.main1", "VideoPlayer", videoPlayerAssetID);
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, videoPlayerAssetID, 1);
            Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, videoPlayerAssetID, new Vector3(0.05f, 0.05f, 0.05f));
            Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, videoPlayerAssetID, new Vector3(0f, 0.04f, 0.12f));
            Console.ExecuteCommand("asset-destroycolliders", ReceiverGroup.All, videoPlayerAssetID);
            Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, videoPlayerAssetID, "Video", selectedVideoURL);
        }

        public static void StopVideoHand()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, videoPlayerAssetID);
        }

        private static int phoneAssetID;
        public static void PlayVideoPhone()
        {
            phoneAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "samsungphone", phoneAssetID);
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, phoneAssetID, 1);
            Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, phoneAssetID, new Vector3(-0.075f, 0.1f, 0f));
            Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, phoneAssetID, Quaternion.Euler(80f, 90f, 180f));
            Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, phoneAssetID, Vector3.one * 0.3f);
            Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, phoneAssetID, "VideoPlayer", selectedVideoURL);
            Console.ExecuteCommand("asset-destroycolliders", ReceiverGroup.All, phoneAssetID);
        }

        public static void StopVideoPhone()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, phoneAssetID);
        }

        private static int televisionAssetID;
        private static int sofaAssetID;
        public static void PlayVideoTV()
        {
            televisionAssetID = Console.GetFreeAssetID();
            sofaAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "TV", televisionAssetID);
            Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, televisionAssetID, new Vector3(-57.1f, 5.6f, -37f));
            Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, televisionAssetID, Quaternion.Euler(270f, 0f, 0f));
            Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, televisionAssetID, "VideoPlayer", selectedVideoURL);

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "sofa", sofaAssetID);
            Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, sofaAssetID, new Vector3(-51.8f, 4.2f, -37.4f));
            Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, sofaAssetID, Quaternion.Euler(270f, 270f, 0f));
        }

        public static void StopVideoTV()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, televisionAssetID);
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, sofaAssetID);
        }

        private static int selectedConcertIndex = 0;
        public static string selectedConcert = "BACKD00R";
        public static void ChangeConcert(bool positive = true)
        {
            string[] ConcertNames = {
                "BACKD00R",
                "CRANK",
                "DIAMONDS SPECIAL",
                "DRUGS GOT ME NUMB",
                "F33l Lik3 Dyin",
                "FINE SHIT",
                "Foreign",
                "I SEEEE YOU BABY BOI",
                "JumpOutTheHouse",
                "Lean 4 Real",
                "Long Time",
                "Mileage",
                "MOJO JOJO",
                "New Tank",
                "OLYMPIAN",
                "OPM BABI",
                "Over",
                "POP OUT",
                "Punk Monk",
                "R.I.P. Fredo (Notice Me)",
                "RADAR",
                "Rockstar Made",
                "Sky",
                "SOME MORE"
            };

            if (positive)
                selectedConcertIndex++;
            else
                selectedConcertIndex--;

            selectedConcertIndex %= ConcertNames.Length;
            if (selectedConcertIndex < 0)
                selectedConcertIndex = ConcertNames.Length - 1;

            selectedConcert = ConcertNames[selectedConcertIndex];

            Buttons.GetIndex("Admin Concert Selector").overlapText = "Concert: " + selectedConcert;

            if (Console.consoleAssets.ContainsKey(concertAssetID))
                Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, concertAssetID, "audio", selectedConcert);
        }

        private static int concertAssetID;
        public static void PlayConcert()
        {
            concertAssetID = Console.GetFreeAssetID();

            Vector3 position = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest").activeInHierarchy
                                    ? new Vector3(-27f, 2.4f, -49.9f)
                                    : new Vector3(-28.4873f, 15.5272f, -117.8634f);

            Quaternion rotation = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest").activeInHierarchy
                                    ? Quaternion.Euler(0f, 250f, 0f)
                                    : Quaternion.Euler(0f, 300f, 0f);

            Vector3 scale = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest").activeInHierarchy
                                    ? new Vector3(0.5f, 0.5f, 0.5f)
                                    : new Vector3(0.8f, 0.8f, 0.8f);

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "concert", "concert", concertAssetID);

            Console.ExecuteCommand("asset-settransform", ReceiverGroup.All, concertAssetID, position, rotation);
            Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, concertAssetID, scale);
            Console.ExecuteCommand("asset-destroychild", ReceiverGroup.All, concertAssetID, "stage/Targetphoto");
            Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, concertAssetID, "audio", selectedConcert);
        }

        public static void StopConcert()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, concertAssetID);
        }

        private static int selectedTrailIndex = 0;
        public static string selectedTrail = "Flash";
        public static void ChangeTrail(bool positive = true)
        {
            string[] TrailNames = {
                "Flash",
                "Accelerated Man",
                "Ares",
                "Black Flash",
                "Emerald",
                "Godspeed",
                "Kid Flash",
                "Rainbow Flash",
                "Reverse Flash",
                "Zoom"
            };

            if (positive)
                selectedTrailIndex++;
            else
                selectedTrailIndex--;

            selectedTrailIndex %= TrailNames.Length;
            if (selectedTrailIndex < 0)
                selectedTrailIndex = TrailNames.Length - 1;

            selectedTrail = TrailNames[selectedTrailIndex];

            Buttons.GetIndex("Admin Trail Selector").overlapText = "Trail: " + selectedTrail;
        }

        private static int leftTrailAssetID;
        private static int leftLightningAssetID;
        private static int rightTrailAssetID;
        private static int rightLightningAssetID;
        private static int bodyTrailAssetID;
        public static void SpawnTrailEffect()
        {
            leftTrailAssetID = Console.GetFreeAssetID();
            leftLightningAssetID = Console.GetFreeAssetID();

            rightTrailAssetID = Console.GetFreeAssetID();
            rightLightningAssetID = Console.GetFreeAssetID();

            bodyTrailAssetID = Console.GetFreeAssetID();


            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "flasheffects", selectedTrail + " Left Hand Trail", leftTrailAssetID);
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, leftTrailAssetID, 1);
            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "flasheffects", selectedTrail + " Left Hand Lightning", leftLightningAssetID);
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, leftLightningAssetID, 1);

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "flasheffects", selectedTrail + " Right Hand Trail", rightTrailAssetID);
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, rightTrailAssetID, 2);
            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "flasheffects", selectedTrail + " Right Hand Lightning", rightLightningAssetID);
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, rightLightningAssetID, 2);

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "flasheffects", selectedTrail + " Body Trail", bodyTrailAssetID);
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, bodyTrailAssetID, 3);
        }

        public static void RemoveTrailEffect()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, leftTrailAssetID);
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, leftLightningAssetID);

            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, rightTrailAssetID);
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, rightLightningAssetID);

            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, bodyTrailAssetID);
        }

        private static int coinAssetID;
        public static void SpawnCoin()
        {
            coinAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "console.main1", "Coin", coinAssetID);
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, coinAssetID, 2);
        }

        private static float spawnCoinDelay = 0f;
        private static float flipCoinDelay = 0f;
        public static void Coin()
        {
            if (rightGrab)
            {
                if (Time.time > spawnCoinDelay && !Console.consoleAssets.ContainsKey(coinAssetID))
                {
                    spawnCoinDelay = Time.time + 0.1f;
                    RemoveCoin();
                    SpawnCoin();
                }

                if (Time.time > flipCoinDelay && rightTrigger > 0.25f)
                {
                    flipCoinDelay = Time.time + 2.5f;
                    FlipCoin();
                }
            }
            else
            {
                RemoveCoin();
            }
        }

        public static void FlipCoin()
        {
            bool isHeads = Random.Range(0f, 1f) >= 0.5f;

            Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, coinAssetID, "CoinHolder", isHeads ? "Heads" : "Tails");
            Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, coinAssetID, "CoinHolder", "Flip");
        }

        public static void RemoveCoin()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, coinAssetID);
        }

        private static int shibaAssetID;
        public static void SpawnShiba()
        {
            shibaAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "shibaholdable", "shiba", shibaAssetID);
            Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, shibaAssetID, new Vector3(25f, 25f, 25f));
            Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, shibaAssetID, new Vector3(0.0291f, 0.0927f, 0.1826f));
            Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, shibaAssetID, Quaternion.Euler(9.0192f, 18.5675f, 68.2654f));
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, shibaAssetID, 2);
        }

        private static float spawnShibaDelay = 0f;
        public static void Shiba()
        {
            if (rightGrab)
            {
                if (Time.time > spawnShibaDelay && !Console.consoleAssets.ContainsKey(shibaAssetID))
                {
                    spawnShibaDelay = Time.time + 0.1f;
                    RemoveShiba();
                    SpawnShiba();
                }
            }
            else
            {
                RemoveShiba();
            }
        }

        public static void RemoveShiba()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, shibaAssetID);
        }

        private static int pigeonAssetID;
        public static void SpawnPigeon()
        {
            pigeonAssetID = Console.GetFreeAssetID();

            Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "pigeon", "Pigeon", pigeonAssetID);
            Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, pigeonAssetID, new Vector3(0.16f, 0.0236f, -0.0764f));
            Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, pigeonAssetID, Quaternion.Euler(345.613f, 303.5778f, 89.6919f));
            Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, pigeonAssetID, 2);
        }

        private static float spawnPigeonDelay = 0f;
        public static void Pigeon()
        {
            if (rightGrab)
            {
                if (Time.time > spawnPigeonDelay && !Console.consoleAssets.ContainsKey(pigeonAssetID))
                {
                    spawnPigeonDelay = Time.time + 0.1f;
                    RemovePigeon();
                    SpawnPigeon();
                }
            }
            else
            {
                RemovePigeon();
            }
        }

        public static void RemovePigeon()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, pigeonAssetID);
        }

        public static async Task RemoveAllAssets()
        {
            foreach (int assetID in Console.consoleAssets.Keys.ToList())
            {
                if (NetworkSystem.Instance.InRoom)
                    Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetID);

                await Task.Delay(50);
            }
        }
    }
}
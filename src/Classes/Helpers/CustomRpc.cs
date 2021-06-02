﻿using System;
using System.Collections.Generic;
using System.Data;
using HarmonyLib;
using Hazel;
using System.Linq;
using HarryPotter.Classes.Items;
using HarryPotter.Classes.Roles;
using HarryPotter.Classes.WorldItems;
using Hazel.Udp;
using Il2CppSystem.Diagnostics;
using Il2CppSystem.Net;
using UnityEngine;
using hunterlib.Classes;

namespace HarryPotter.Classes
{
    public enum Packets
    {
        AssignRole = 70,
        FixLightsRpc = 71,
        ForceAllVotes = 72,
        CreateCurse = 73,
        DestroyCurse = 74,
        KillPlayerUnsafe = 75,
        DeactivatePlayer = 76,
        DestroyCrucio = 77,
        CreateCrucio = 78,
        StartControlling = 79,
        MoveControlledPlayer = 80,
        InvisPlayer = 81,
        DefensiveDuelist = 82,
        RevivePlayer = 83,
        TeleportPlayer = 84,
        SpawnItem = 85,
        TryPickupItem = 86,
        GiveItem = 87,
        DestroyItem = 88,
        UseItem = 89,
        UpdateSpeedMultiplier = 90,
        RevealRole = 91,
        FakeKill = 92,
        FinallyDie = 93,
        RequestRole = 94,
    }

    public class CustomRpc
    {
        public void Handle(byte packetId, MessageReader reader)
        {
            switch (packetId)
            {
                case (byte)Packets.AssignRole:
                    byte playerId = reader.ReadByte();
                    string roleName = reader.ReadString();
                    ModdedPlayerClass rolePlayer = Main.Instance.ModdedPlayerById(playerId);
                    switch (roleName)
                    {
                        case "Voldemort":
                            rolePlayer.Role = new Voldemort(rolePlayer);
                            break;
                        case "Bellatrix":
                            rolePlayer.Role = new Bellatrix(rolePlayer);
                            break;
                        case "Harry":
                            rolePlayer.Role = new Harry(rolePlayer);
                            break;
                        case "Hermione":
                            rolePlayer.Role = new Hermione(rolePlayer);
                            break;
                        case "Ron":
                            rolePlayer.Role = new Ron(rolePlayer);
                            break;
						case "Mayor":
							rolePlayer.Role = new Mayor(rolePlayer);
							break;
                    }
                    break;
                case (byte)Packets.RequestRole:
                    if (AmongUsClient.Instance.AmHost)
                    {
                        byte requesterId = reader.ReadByte();
                        string requestedRole = reader.ReadString();

                        if (Main.Instance.PlayersWithRequestedRoles.All(x => x.Item1.PlayerId != requesterId))
                            Main.Instance.PlayersWithRequestedRoles.Add(new Pair<PlayerControl, string>(GameData.Instance.GetPlayerById(requesterId).Object, requestedRole));
                    }
                    break;
                case (byte)Packets.FinallyDie:
                    byte finallyDeadId = reader.ReadByte();
                    Main.Instance.PlayerDie(Main.Instance.ModdedPlayerById(finallyDeadId)._Object);
                    break;
                case (byte)Packets.FakeKill:
                    byte fakeKilledId = reader.ReadByte();
                    Coroutines.Start(Main.Instance.CoFakeKill(Main.Instance.ModdedPlayerById(fakeKilledId)._Object));
                    break;
                case (byte)Packets.FixLightsRpc:
                    var switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                    switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
                    break;
                case (byte)Packets.ForceAllVotes:
                    byte forcePlayer = reader.ReadByte();
                    Main.Instance.ForceAllVotes((sbyte)forcePlayer);
                    break;
                case (byte)Packets.CreateCurse:
                    byte casterId = reader.ReadByte();
                    Vector2 direction = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    Main.Instance.CreateCurse(direction, Main.Instance.ModdedPlayerById(casterId));
                    break;
                case (byte)Packets.CreateCrucio:
                    byte blinderId = reader.ReadByte();
                    Vector2 crucioDirection = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    Main.Instance.CreateCrucio(crucioDirection, Main.Instance.ModdedPlayerById(blinderId));
                    break;
                case (byte)Packets.DestroyCurse:
                    Main.Instance.DestroySpell("_curse");
                    break;
                case (byte)Packets.DestroyCrucio:
                    Main.Instance.DestroySpell("_crucio");
                    break;
                case (byte)Packets.KillPlayerUnsafe:
                    byte killerId = reader.ReadByte();
                    byte targetId = reader.ReadByte();
                    bool isCurseKill = reader.ReadBoolean();
                    bool forceAnim = reader.ReadBoolean();
                    ModdedPlayerClass target = Main.Instance.ModdedPlayerById(targetId);
                    ModdedPlayerClass killer = Main.Instance.ModdedPlayerById(killerId);
                    Main.Instance.KillPlayer(killer._Object, target._Object, isCurseKill, forceAnim);
                    break;
                case (byte)Packets.DeactivatePlayer:
                    byte blindId = reader.ReadByte();
                    ModdedPlayerClass blind = Main.Instance.ModdedPlayerById(blindId);
                    Main.Instance.CrucioBlind(blind._Object);
                    break;
                case (byte)Packets.StartControlling:
                    byte controllerId = reader.ReadByte();
                    byte controlledId = reader.ReadByte();
                    ModdedPlayerClass controller = Main.Instance.ModdedPlayerById(controllerId);
                    ModdedPlayerClass controlled = Main.Instance.ModdedPlayerById(controlledId);
                    Main.Instance.ControlPlayer(controller._Object, controlled._Object);
                    break;
                case (byte)Packets.MoveControlledPlayer:
                    byte moveId = reader.ReadByte();
                    Vector3 newVel = new Vector3(reader.ReadSingle(), reader.ReadSingle());
                    Vector3 newPos = new Vector3(reader.ReadSingle(), reader.ReadSingle());
                    PlayerControl movePlayer = Main.Instance.ModdedPlayerById(moveId)._Object;
                    if (movePlayer.AmOwner)
                    {
                        movePlayer.transform.position = newPos;
                        movePlayer.MyPhysics.body.position = newPos;
                        movePlayer.MyPhysics.body.velocity = newVel;
                        System.Console.WriteLine("MoveControlledPlayer");
                    }
                    break;
                case (byte)Packets.InvisPlayer:
                    byte invisId = reader.ReadByte();
                    PlayerControl invisPlayer = Main.Instance.ModdedPlayerById(invisId)._Object;
                    Main.Instance.InvisPlayer(invisPlayer);
                    break;
                case (byte)Packets.DefensiveDuelist:
                    byte ddId = reader.ReadByte();
                    PlayerControl ddPlayer = Main.Instance.ModdedPlayerById(ddId)._Object;
                    Main.Instance.DefensiveDuelist(ddPlayer);
                    break;
                case (byte)Packets.RevivePlayer:
                    byte reviveId = reader.ReadByte();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (player.PlayerId != reviveId)
                            continue;
                        if (!player.Data.IsDead)
                            continue;
                        
                        player.Revive();
                        foreach (DeadBody body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                            if (body.ParentId == reviveId)
                                UnityEngine.Object.Destroy(body.gameObject);
                    }
                    break;
                case (byte)Packets.TeleportPlayer:
                    var teleportId = reader.ReadByte();
                    var teleportPos = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (teleportId == player.PlayerId)
                            player.NetTransform.SnapTo(teleportPos);
                    break;
                case (byte)Packets.SpawnItem:
                    var itemId = reader.ReadInt32();
                    var itemPosition = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    var velocity = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    Main.Instance.SpawnItem(itemId, itemPosition, velocity);
                    break;
                case (byte)Packets.TryPickupItem:
                    if (!AmongUsClient.Instance.AmHost)
                        return;
                    
                    var targetPlayer = reader.ReadByte();
                    var pickupId = reader.ReadInt32();
                    if (Main.Instance.AllItems.Any(x => x.Id == pickupId))
                    {
                        List<WorldItem> allMatches = Main.Instance.AllItems.FindAll(x => x.Id == pickupId);
                        foreach (WorldItem item in allMatches) item.Delete();
                        Main.Instance.AllItems.RemoveAll(x => x.IsPickedUp);
                        
                        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.GiveItem, SendOption.Reliable);
                        writer.Write(targetPlayer);
                        writer.Write(pickupId);
                        writer.EndMessage();
                    }
                    break;
                case (byte)Packets.GiveItem:
                    var targetPlayer2 = reader.ReadByte();
                    var pickupId2 = reader.ReadInt32();

                    if (targetPlayer2 != PlayerControl.LocalPlayer.PlayerId)
                        return;

                    if (Main.Instance.GetLocalModdedPlayer().HasItem(pickupId2))
                        return;

                    Main.Instance.GiveGrabbedItem(pickupId2);
                    Main.Instance.AllItems.RemoveAll(x => x.IsPickedUp);
                    break;
                case (byte)Packets.DestroyItem:
                    if (!AmongUsClient.Instance.AmHost)
                    {
                        var targetItemId = reader.ReadInt32();
                        List<WorldItem> allMatches = Main.Instance.AllItems.FindAll(x => x.Id == targetItemId);
                        foreach (WorldItem item in allMatches)
                            item.Delete();
                        Main.Instance.AllItems.RemoveAll(x => x.IsPickedUp);
                    }
                    break;
                case (byte)Packets.UseItem:
                    if (!AmongUsClient.Instance.AmHost) return;
                    int usedItemId = reader.ReadInt32();
                    switch (usedItemId)
                    {
                        case 0:
                            DeluminatorWorld.HasSpawned = false;
                            break;
                        case 1:
                            MaraudersMapWorld.HasSpawned = false;
                            break;
                        case 2:
                            PortKeyWorld.HasSpawned = false;
                            break;
                        case 5:
                            ButterBeerWorld.HasSpawned = false;
                            break;
                    }
                    break;
                case (byte)Packets.UpdateSpeedMultiplier:
                    byte readerId = reader.ReadByte();
                    float newSpeed = reader.ReadSingle();
                    Main.Instance.ModdedPlayerById(readerId).SpeedMultiplier = newSpeed;
                    break;
                case (byte)Packets.RevealRole:
                    byte revealId = reader.ReadByte();
                    Main.Instance.RevealRole(revealId);
                    break;
            }
        }
    }
}

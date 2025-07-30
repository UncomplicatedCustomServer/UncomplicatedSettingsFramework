using CommandSystem.Commands.RemoteAdmin.Dummies;
using LabApi.Features.Wrappers;
using MapGeneration;
using Mirror;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UncomplicatedSettingsFramework.Integrations;
using UnityEngine;
using Utils.Networking;

namespace UncomplicatedSettingsFramework.Api.Features
{
    public class ActionManager
    {
        public static void ExecuteActions(List<string> actions, Player player)
        {
            foreach (string action in actions)
                ParseAndExecuteAction(action, player);
        }

        private static void ParseAndExecuteAction(string action, Player player)
        {
            string processedAction = ResolvePlaceholders(action, player);

            if (processedAction.Trim().ToUpper().StartsWith("IF "))
            {
                ExecuteConditionalAction(processedAction, player);
                return;
            }

            string[] parts = processedAction.Split(new[] { ' ' }, 2);
            string command = parts[0].ToUpper();
            string[] args = parts.Length > 1 ? parts[1].Split(' ') : new string[0];

            switch (command)
            {
                case "SAY":
                    if (args.Length > 0)
                    {
                        player.SendBroadcast(string.Join(" ", args), 5);
                    }
                    break;
                case "GIVE":
                    if (args.Length > 0 && Enum.TryParse(args[0], true, out ItemType itemType))
                    {
                        player.AddItem(itemType);
                    }
                    break;
                case "UCR_GIVE_ROLE":
                    if (args.Length > 0 && int.TryParse(args[0], out int roleId))
                    {
                        UCR.GiveCustomRole(roleId, player);
                    }
                    break;
                case "BC":
                    if (args.Length > 1 && ushort.TryParse(args[0], out ushort duration))
                    {
                        player.SendBroadcast(string.Join(" ", args, 1, args.Length - 1), duration);
                    }
                    break;
                case "CASSIE":
                    if (args.Length > 0)
                    {
                        Cassie.Message(string.Join(" ", args));
                    }
                    break;
                case "WARHEAD":
                    if (args.Length > 0)
                    {
                        switch (args[0].ToUpper())
                        {
                            case "START":
                                Warhead.Start();
                                break;
                            case "STOP":
                                Warhead.Stop();
                                break;
                            case "LOCK":
                                Warhead.IsLocked = true;
                                break;
                            case "UNLOCK":
                                Warhead.IsLocked = false;
                                break;
                        }
                    }
                    break;
                case "EFFECT":
                    if (args.Length > 0)
                    {
                        switch (args[0].ToUpper())
                        {
                            case "GIVE":
                                player.ReferenceHub.playerEffectsController.ChangeState(args[1], byte.Parse(args[2]), float.Parse(args[3]));
                                break;
                            case "CLEAR":
                                player.DisableAllEffects();
                                break;
                            case "REMOVE":
                                player.ReferenceHub.playerEffectsController.ChangeState(args[1], 0, 0);
                                break;
                        }
                    }
                    break;
                case "KILL":
                    player.Kill("Action executed");
                    break;
                case "HURT":
                    if (args.Length > 0 && float.TryParse(args[0], out float damage))
                    {
                        CustomReasonDamageHandler handler = new("Action executed", damage);
                        player.Damage(handler);
                    }
                    break;
                case "HEAL":
                    if (args.Length > 0 && float.TryParse(args[0], out float healAmount))
                    {
                        player.Health = Math.Min(player.MaxHealth, player.Health + healAmount);
                    }
                    else
                    {
                        player.Health = player.MaxHealth;
                    }
                    break;
                case "SIZE":
                    if (args.Length >= 3)
                    {
                        Vector3 scale = new(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
                        player.ReferenceHub.transform.localScale = scale;
                        new SyncedScaleMessages.ScaleMessage(scale, player.ReferenceHub).SendToAuthenticated();
                    }
                    break;
                case "HP":
                    if (args.Length > 0 && float.TryParse(args[0], out float hp))
                    {
                        player.Health = hp;
                    }
                    break;
                case "GOD":
                    player.IsGodModeEnabled = !player.IsGodModeEnabled;
                    break;
                case "TP":
                    if (args.Length > 0 && Enum.TryParse(args[0], true, out RoomName roomType))
                    {
                        player.Position = Room.Get(roomType).FirstOrDefault().Position;
                    }
                    break;
                case "CMD":
                case "COMMAND":
                    if (args.Length > 0)
                    {
                        Server.RunCommand(string.Join(" ", args));
                    }
                    break;
                case "MUTE":
                    if (player.IsMuted)
                        player.Unmute(true);
                    else
                        player.Mute();
                    break;
                case "UNMUTE":
                    player.Unmute(true);
                    break;
                case "KICK":
                    if (args.Length > 0)
                    {
                        player.Kick(string.Join(" ", args));
                    }
                    else
                    {
                        player.Kick("Kicked by action");
                    }
                    break;
                case "BAN":
                    if (args.Length > 1 && int.TryParse(args[0], out int banDuration))
                    {
                        string reason = string.Join(" ", args, 1, args.Length - 1);
                        player.Ban(reason, banDuration);
                    }
                    break;
                case "ROLE":
                    if (args.Length > 0 && Enum.TryParse(args[0], true, out RoleTypeId roleType))
                    {
                        player.SetRole(roleType);
                    }
                    break;
                case "CLEAR_INVENTORY":
                    player.ClearInventory();
                    break;
                case "REMOVE_ITEM":
                    if (args.Length > 0 && Enum.TryParse(args[0], true, out ItemType removeItemType))
                    {
                        player.RemoveItem(removeItemType);
                    }
                    break;
                case "HINT":
                    if (args.Length > 1 && int.TryParse(args[0], out int hintDuration))
                    {
                        string message = string.Join(" ", args, 1, args.Length - 1);
                        player.SendHint(message, hintDuration);
                    }
                    break;
                case "UCI":
                    if (args.Length > 1 && uint.TryParse(args[0], out uint id))
                    {
                        UCI.GiveCustomItem(id, player);
                    }
                    break;
                case "DUMMY":
                    if (args.Length > 0)
                    {
                        ReferenceHub dummy = null;
                        switch (args[0].ToUpper())
                        {
                            case "SPAWN":
                                if (args.Length > 1 && dummy is null)
                                    dummy = DummyUtils.SpawnDummy(string.Join(" ", args, 1, args.Length - 1));
                                else if (dummy is null)
                                    dummy = DummyUtils.SpawnDummy();
                                break;
                            case "REMOVE":
                                if (dummy is not null)
                                {
                                    NetworkServer.Destroy(dummy.gameObject);
                                    dummy = null;
                                }
                                break;
                            case "REMOVE_ALL":
                                DummyUtils.DestroyAllDummies();
                                break;
                            case "FOLLOW":
                                if (dummy.TryGetComponent<PlayerFollower>(out var component))
                                {
                                    UnityEngine.Object.Destroy(component);
                                }
                                else
                                {
                                    dummy.gameObject.AddComponent<PlayerFollower>().Init(player.ReferenceHub);
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        private static void ExecuteConditionalAction(string action, Player player)
        {
            string[] ifThenParts = action.Split(new[] { " THEN " }, 2, StringSplitOptions.None);
            if (ifThenParts.Length != 2) return;

            string condition = ifThenParts[0].Substring(3).Trim();
            string thenAction;
            string elseAction = null;

            string[] elseParts = ifThenParts[1].Split(new[] { " ELSE " }, 2, StringSplitOptions.None);
            if (elseParts.Length == 2)
            {
                thenAction = elseParts[0].Trim();
                elseAction = elseParts[1].Trim();
            }
            else
            {
                thenAction = ifThenParts[1].Trim();
            }

            if (EvaluateCondition(condition))
            {
                ParseAndExecuteAction(thenAction, player);
            }
            else if (elseAction != null)
            {
                ParseAndExecuteAction(elseAction, player);
            }
        }

        private static bool EvaluateCondition(string condition)
        {
            string[] parts = condition.Split(new[] { ' ' }, 3);
            if (parts.Length != 3) return false;

            string leftOperand = parts[0];
            string op = parts[1].ToUpper();
            string rightOperand = parts[2];

            if (float.TryParse(leftOperand, NumberStyles.Any, CultureInfo.InvariantCulture, out float leftNum) &&
                float.TryParse(rightOperand, NumberStyles.Any, CultureInfo.InvariantCulture, out float rightNum))
            {
                switch (op)
                {
                    case "==": return leftNum == rightNum;
                    case "!=": return leftNum != rightNum;
                    case ">": return leftNum > rightNum;
                    case "<": return leftNum < rightNum;
                    case ">=": return leftNum >= rightNum;
                    case "<=": return leftNum <= rightNum;
                    default: return false;
                }
            }

            int comparison = string.Compare(leftOperand, rightOperand, StringComparison.OrdinalIgnoreCase);
            return op switch
            {
                "==" => comparison == 0,
                "!=" => comparison != 0,
                _ => false,
            };
        }

        private static string ResolvePlaceholders(string text, Player player)
        {
            System.Random rand = new();
            Player randomPlayer = Player.ReadyList.ElementAt(rand.Next(Player.ReadyList.Count()));
            string randomPlayerId = randomPlayer.PlayerId.ToString();

            return text
                .Replace("{player.name}", player.Nickname)
                .Replace("{player.id}", player.PlayerId.ToString())
                .Replace("{random.player.id}", randomPlayerId)
                .Replace("{player.position}", player.Position.ToString("F2").Replace(",", ""))
                .Replace("{player.role}", player.Role.ToString())
                .Replace("{player.health}", player.Health.ToString(CultureInfo.InvariantCulture))
                .Replace("{player.zone}", player.Zone.ToString())
                .Replace("{player.rotation}", player.Rotation.ToString().Replace(",", ""))
                .Replace("{player.userid}", player.UserId)
                .Replace("{player.team}", player.Role.GetTeam().ToString())
                .Replace("{player.maxhealth}", player.MaxHealth.ToString(CultureInfo.InvariantCulture))
                .Replace("{player.stamina}", player.StaminaRemaining.ToString("F2", CultureInfo.InvariantCulture))
                .Replace("{player.group}", player.GroupName ?? "None")
                .Replace("{player.rankcolor}", player.GroupColor ?? "None")
                .Replace("{player.rank}", player.UserGroup.Name ?? "None")
                .Replace("{player.rankcolor}", player.UserGroup.BadgeColor ?? "None")
                .Replace("{server.name}", Server.ServerListName)
                .Replace("{server.playercount}", Server.PlayerCount.ToString())
                .Replace("{server.maxplayers}", Server.MaxPlayers.ToString())
                .Replace("{round.time}", Round.Duration.ToString(@"mm\:ss"))
                .Replace("{datetime.now}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd"))
                .Replace("{time}", DateTime.Now.ToString("HH:mm:ss"))
                .Replace("{player.ip}", player.IpAddress)
                .Replace("{player.address}", player.IpAddress)
                .Replace("{player.ipaddress}", player.IpAddress)
                .Replace("{player.isalive}", player.IsAlive.ToString())
                .Replace("{player.isdead}", (!player.IsAlive).ToString())
                .Replace("{player.items}", string.Join(", ", player.Items.Select(item => item.ToString())))
                .Replace("{player.itemcount}", player.Items.Count().ToString())
                .Replace("{player.ammo}", player.Ammo.ToString())
                .Replace("{server.port}", Server.Port.ToString())
                .Replace("{server.ip}", Server.IpAddress)
                .Replace("{server.address}", Server.IpAddress)
                .Replace("{server.ipaddress}", Server.IpAddress)
                .Replace("{server.tps}", Server.Tps.ToString("F1"))
                .Replace("{round.escapees}", (Round.EscapedClassD + Round.EscapedScientists).ToString())
                .Replace("{round.scps}", Round.SurvivingSCPs.ToString());
        }
    }
}
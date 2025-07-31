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
using System.Text.RegularExpressions;
using UncomplicatedSettingsFramework.Api.Features.Helper;
using UncomplicatedSettingsFramework.Api.Helpers;
using UncomplicatedSettingsFramework.Integrations;
using UnityEngine;
using UserSettings.ServerSpecific;
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
            string processedAction = ResolveDynamicPlaceholders(action, player);

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
            string[] ifThenParts = Regex.Split(action, @"\s+(THEN|ELSE)\s+", RegexOptions.IgnoreCase);
            if (ifThenParts.Length < 3) return;

            string condition = ifThenParts[0].Substring(3).Trim();
            string thenAction = ifThenParts[2].Trim();
            string elseAction = null;

            if (ifThenParts.Length >= 5 && ifThenParts[3].Equals("ELSE", StringComparison.OrdinalIgnoreCase))
            {
                elseAction = ifThenParts[4].Trim();
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
            string[] orParts = condition.Split(new[] { " OR " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string orPart in orParts)
            {
                string[] andParts = orPart.Split(new[] { " AND " }, StringSplitOptions.RemoveEmptyEntries);
                bool isAndBlockTrue = true;
                foreach (string andPart in andParts)
                {
                    if (!EvaluateSingleCondition(andPart.Trim()))
                    {
                        isAndBlockTrue = false;
                        break;
                    }
                }

                if (isAndBlockTrue)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool EvaluateSingleCondition(string condition)
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

        private static string ResolveDynamicPlaceholders(string actionText, Player player)
        {
            return Regex.Replace(actionText, @"\{(?<type>\w+)\.(?<id>\d+)\.(?<prop>\w+)\}", match =>
            {
                string type = match.Groups["type"].Value.ToLower();
                string id = match.Groups["id"].Value;
                string prop = match.Groups["prop"].Value.Trim().ToLower();

                LogManager.Debug($"-- Resolving Placeholder: {match.Value} --");
                LogManager.Debug($"Type: '{type}', ID: '{id}', Prop: '{prop}'");

                switch (type)
                {
                    case "dropdown" when int.TryParse(id, out int dropdownId) && prop == "selection":
                        {
                            SSDropdownSetting dropdown = SSSHelper.GetUserSetting<SSDropdownSetting>(player.ReferenceHub, dropdownId);
                            if (dropdown == null)
                            {
                                LogManager.Warn($"Dropdown with ID {dropdownId} not found for player {player.Nickname}.");
                                return match.Value;
                            }
                            string selection = dropdown.Options[dropdown.SyncSelectionIndexRaw];
                            LogManager.Debug($"Dropdown selection for ID {dropdownId} is: '{selection}'");
                            return selection;
                        }

                    case "player":
                        return GetPlayerPlaceholder(player, id);
                    case "server":
                        return GetServerPlaceholder(id);
                    case "round":
                        return GetRoundPlaceholder(id);
                    case "datetime":
                        return GetDateTimePlaceholder(id);
                    default:
                        LogManager.Warn($"Unrecognized or incomplete placeholder: {match.Value}");
                        return match.Value;
                }
            });
        }
        private static string GetPlayerPlaceholder(Player player, string property)
        {
            switch (property)
            {
                case "name": return player.Nickname;
                case "id": return player.PlayerId.ToString();
                case "position": return player.Position.ToString("F2").Replace(",", "");
                case "role": return player.Role.ToString();
                case "health": return player.Health.ToString(CultureInfo.InvariantCulture);
                case "zone": return player.Zone.ToString();
                case "rotation": return player.Rotation.ToString().Replace(",", "");
                case "userid": return player.UserId;
                case "team": return player.Role.GetTeam().ToString();
                case "maxhealth": return player.MaxHealth.ToString(CultureInfo.InvariantCulture);
                case "stamina": return player.StaminaRemaining.ToString("F2", CultureInfo.InvariantCulture);
                case "group": return player.GroupName ?? "None";
                case "rankcolor": return player.GroupColor ?? "None";
                case "rank": return player.UserGroup.Name ?? "None";
                case "ip": case "address": case "ipaddress": return player.IpAddress;
                case "isalive": return player.IsAlive.ToString();
                case "isdead": return (!player.IsAlive).ToString();
                case "items": return string.Join(", ", player.Items.Select(item => item.Type.ToString()));
                case "itemcount": return player.Items.Count().ToString();
                case "ammo": return player.Ammo.ToString();
                default: return $"{{player.{property}}}";
            }
        }

        private static string GetServerPlaceholder(string property)
        {
            switch (property)
            {
                case "name": return Server.ServerListName;
                case "playercount": return Server.PlayerCount.ToString();
                case "maxplayers": return Server.MaxPlayers.ToString();
                case "port": return Server.Port.ToString();
                case "ip": case "address": case "ipaddress": return Server.IpAddress;
                case "tps": return Server.Tps.ToString("F1");
                default: return $"{{server.{property}}}";
            }
        }

        private static string GetRoundPlaceholder(string property)
        {
            switch (property)
            {
                case "time": return Round.Duration.ToString(@"mm\:ss");
                case "escapees": return (Round.EscapedClassD + Round.EscapedScientists).ToString();
                case "scps": return Round.SurvivingSCPs.ToString();
                default: return $"{{round.{property}}}";
            }
        }
        private static string GetDateTimePlaceholder(string property)
        {
            switch (property)
            {
                case "now": return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                case "date": return DateTime.Now.ToString("yyyy-MM-dd");
                case "time": return DateTime.Now.ToString("HH:mm:ss");
                default: return $"{{datetime.{property}}}";
            }
        }
    }
}
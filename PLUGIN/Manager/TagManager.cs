using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Admin;
using System.Text.RegularExpressions;

namespace Mesharsky_Vip;

public partial class MesharskyVip
{
    private void InitializeTags()
    {
        if (Config?.PluginSettings.UseInternalTags == false)
        {
            Console.WriteLine("[Mesharsky - VIP] Internal Tag Manager disabled (using external plugin)");
            return;
        }

        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawnTags, HookMode.Post);
        AddCommandListener("say", OnPlayerChat);
        AddCommandListener("say_team", OnPlayerChatTeam);
        Console.WriteLine("[Mesharsky - VIP] Tag Manager initialized");
    }

    private HookResult OnPlayerSpawnTags(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot)
            return HookResult.Continue;

        // Apply Scoreboard Tag (Clan Tag)
        var service = GetPlayerVipService(player);
        if (service != null && !string.IsNullOrEmpty(service.ScoreboardTag))
        {
            player.Clan = service.ScoreboardTag;
        }

        return HookResult.Continue;
    }

    private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || player.IsBot)
            return HookResult.Continue;

        var service = GetPlayerVipService(player);
        if (service == null)
            return HookResult.Continue;

        if (string.IsNullOrEmpty(service.ChatTag) && 
            string.IsNullOrEmpty(service.ChatColor) && 
            string.IsNullOrEmpty(service.NameColor))
            return HookResult.Continue;

        var message = info.GetArg(1);
        if (string.IsNullOrWhiteSpace(message) || message.StartsWith("/") || message.StartsWith("!"))
            return HookResult.Continue;

        string formattedTag = "";
        if (!string.IsNullOrEmpty(service.ChatTag))
        {
             formattedTag = $"{service.ChatTag} ";
        }
        
        string nameColor = string.IsNullOrEmpty(service.NameColor) ? "{Team}" : service.NameColor;
        string chatColor = string.IsNullOrEmpty(service.ChatColor) ? "{Default}" : service.ChatColor;
        
        string finalMessage = $"{formattedTag}{nameColor}{player.PlayerName}{ChatColors.Default}: {chatColor}{message}";
        finalMessage = ReplaceColors(finalMessage);
        
        Server.PrintToChatAll(finalMessage);
        
        return HookResult.Handled;
    }

    private HookResult OnPlayerChatTeam(CCSPlayerController? player, CommandInfo info)
    {
         if (player == null || !player.IsValid || player.IsBot)
            return HookResult.Continue;

        var service = GetPlayerVipService(player);
        if (service == null)
             return HookResult.Continue;

        if (string.IsNullOrEmpty(service.ChatTag) && 
            string.IsNullOrEmpty(service.ChatColor) && 
            string.IsNullOrEmpty(service.NameColor))
            return HookResult.Continue;

        var message = info.GetArg(1);
        if (string.IsNullOrWhiteSpace(message) || message.StartsWith("/") || message.StartsWith("!"))
            return HookResult.Continue;

        string formattedTag = "";
        if (!string.IsNullOrEmpty(service.ChatTag))
        {
             formattedTag = $"{service.ChatTag} ";
        }
        
        string nameColor = string.IsNullOrEmpty(service.NameColor) ? "{Team}" : service.NameColor;
        string chatColor = string.IsNullOrEmpty(service.ChatColor) ? "{Default}" : service.ChatColor;
        
        string teamPrefix = "(Team) "; 
        string finalMessage = $"{teamPrefix}{formattedTag}{nameColor}{player.PlayerName}{ChatColors.Default}: {chatColor}{message}";
        finalMessage = ReplaceColors(finalMessage);
        
        var team = player.Team;
        foreach(var p in Utilities.GetPlayers())
        {
            if(p.IsValid && !p.IsBot && p.Team == team)
            {
                p.PrintToChat(finalMessage);
            }
        }
        
        return HookResult.Handled;
    }

    private Service? GetPlayerVipService(CCSPlayerController player)
    {
        if (Config == null) return null;

        if (PlayerCache.TryGetValue(player.SteamID, out var cachedPlayer))
        {
            var activeGroups = cachedPlayer.Groups.Where(g => g.Active).ToList();
            foreach (var group in activeGroups)
            {
                var service = ServiceManager.GetService(group.GroupName);
                if (service != null) return service;
            }
        }
        
        if (Config.NightVip.Enabled && IsNightVipTime())
        {
             var service = ServiceManager.GetService(Config.NightVip.InheritGroup);
             if (service != null) return service;
        }
        
        foreach (var groupConfig in Config.GroupSettings)
        {
            if (AdminManager.PlayerHasPermissions(player, groupConfig.Flag))
            {
                 var service = ServiceManager.GetService(groupConfig.Name);
                 if (service != null) return service;
            }
        }

        return null;
    }

    private string ReplaceColors(string input)
    {
        return input
            .Replace("{Default}", ChatColors.Default.ToString())
            .Replace("{White}", ChatColors.White.ToString())
            .Replace("{DarkRed}", ChatColors.DarkRed.ToString())
            .Replace("{Green}", ChatColors.Green.ToString())
            .Replace("{LightYellow}", ChatColors.LightYellow.ToString())
            .Replace("{LightBlue}", ChatColors.LightBlue.ToString())
            .Replace("{Olive}", ChatColors.Olive.ToString())
            .Replace("{Lime}", ChatColors.Lime.ToString())
            .Replace("{Red}", ChatColors.Red.ToString())
            .Replace("{Purple}", ChatColors.Purple.ToString())
            .Replace("{Grey}", ChatColors.Grey.ToString())
            .Replace("{Yellow}", ChatColors.Yellow.ToString())
            .Replace("{Gold}", ChatColors.Gold.ToString())
            .Replace("{Silver}", ChatColors.Silver.ToString())
            .Replace("{Blue}", ChatColors.Blue.ToString())
            .Replace("{DarkBlue}", ChatColors.DarkBlue.ToString())
            .Replace("{BlueGrey}", ChatColors.BlueGrey.ToString())
            .Replace("{Magenta}", ChatColors.Magenta.ToString())
            .Replace("{LightRed}", ChatColors.LightRed.ToString())
            .Replace("{Team}", "\x03");
    }
}

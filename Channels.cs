using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Commands;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;

namespace Channels
{
    public class Main : RocketPlugin
    {
        int Times;
        public List<ChannelPlayer> Channels = new List<ChannelPlayer>();
        protected override void Load()
        {
            base.Load();
            UnturnedPlayerEvents.OnPlayerChatted += UnturnedPlayerEvents_OnPlayerChatted;
            Rocket.Core.Logging.Logger.Log("[Plugin4U] Channels Loaded!", ConsoleColor.Blue);
            Rocket.Core.Logging.Logger.Log("Make sure to check out our website : Plugin4U.cf ", ConsoleColor.Magenta);
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"globalchat", "You don't have global chat permission!"},
                    {"chat_switch", "You're now chatting in {0}."},
                    {"no_permission", "You don't have permission to chat here!" }
                };
            }
        }

        [RocketCommand("switchchannel", "", "", AllowedCaller.Player)]
        [RocketCommandAlias("sw")]
        public void Execute(IRocketPlayer caller, params string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            foreach (var channel in Channels)
            {
                if (channel.id == player.CSteamID)
                {
                    Channels.Remove(channel);
                }
            }
            Channels.Add(new ChannelPlayer(player.CSteamID, command[0]));
            UnturnedChat.Say(caller, Translate("chat_switch", command[0]));
        }

        private void UnturnedPlayerEvents_OnPlayerChatted(Rocket.Unturned.Player.UnturnedPlayer player, ref UnityEngine.Color color, string message, SDG.Unturned.EChatMode chatMode, ref bool cancel)
        {
            var result = GetChannel(player.CSteamID);
            if(result != null && chatMode == 0 && player.HasPermission("channel." + result.Channel.ToLower()) && result.Channel.ToLower() != "global" && !message.StartsWith("/") && !message.StartsWith("@"))
            {
                cancel = true;
                foreach(var steamplayer in Provider.clients)
                {
                    UnturnedPlayer stm = UnturnedPlayer.FromSteamPlayer(steamplayer);
                    if (stm.HasPermission("channel." + result.Channel.ToLower()))
                    {
                        UnturnedChat.Say(steamplayer.playerID.steamID, "[" + result.Channel + "] " + player.CharacterName + " : " + message);
                        Times = 1;
                    }
                }
            }
            if(result.Channel.ToLower() == "global" && chatMode == 0 && (!message.StartsWith("/") && !message.StartsWith("@")))
            {
                if (player.HasPermission("chat.global"))
                {
                    ChatManager.instance.askChat(player.CSteamID, 0, message);
                    Times = 1;
                }
            }
            if(Times == 0)
            {
                UnturnedChat.Say(player, Translate("no_permission"));
            }

        }

        public ChannelPlayer GetChannel(CSteamID id)
        {
            foreach(var channel in Channels)
            {
                if(channel.id == id)
                {
                    return channel;
                }
            }
            return new ChannelPlayer(id, "global");
        }
    }
    public class ChannelPlayer
    {
        public CSteamID id;
        public string Channel;
        public ChannelPlayer(CSteamID id , string Channel)
        {
            this.id = id;
            this.Channel = Channel;
        }
    }
}

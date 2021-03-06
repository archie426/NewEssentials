﻿using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using NewEssentials.API;
using OpenMod.API.Commands;
using OpenMod.Unturned.Commands;
using SDG.Unturned;

namespace NewEssentials.Commands
{
    [Command("freeze")]
    [CommandDescription("Freeze or unfreeze a player")]
    [CommandSyntax("<player>")]
    public class CFreeze : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IFreezeManager m_FreezeManager;

        public CFreeze(IStringLocalizer stringLocalizer, IFreezeManager freezeManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_FreezeManager = freezeManager;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);

            string searchTerm = Context.Parameters[0];
            if (!PlayerTool.tryGetSteamPlayer(searchTerm, out SteamPlayer player))
                throw new UserFriendlyException(m_StringLocalizer["general:invalid_player", new {Player = searchTerm}]);

            ulong playerSteamID = player.playerID.steamID.m_SteamID;
            if (m_FreezeManager.IsPlayerFrozen(playerSteamID))
            {
                m_FreezeManager.UnfreezePlayer(playerSteamID);
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["freeze:unfrozen",
                    new {Player = player.playerID.characterName}]);
            }
            else
            {
                m_FreezeManager.FreezePlayer(playerSteamID, player.player.transform.position);
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["freeze:frozen",
                    new {Player = player.playerID.characterName}]);
            }
        }
    }
}
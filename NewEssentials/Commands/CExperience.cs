﻿using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Core.Users;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Command = OpenMod.Core.Commands.Command;

namespace NewEssentials.Commands
{
    [UsedImplicitly]
    [Command("experience")]
    [CommandAlias("ex")]
    [CommandDescription("Give yourself or another player experience")]
    [CommandSyntax("<amount> [player]")]
    public class CExperience : Command
    {
        private readonly IPermissionChecker m_PermissionChecker;
        private readonly IStringLocalizer m_StringLocalizer;

        public CExperience(IPermissionChecker permissionChecker, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_PermissionChecker = permissionChecker;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            string permission = "newess.exp";
            if (await m_PermissionChecker.CheckPermissionAsync(Context.Actor, permission) == PermissionGrantResult.Deny)
                throw new NotEnoughPermissionException(Context, permission);

            if (Context.Parameters.Length > 2 || Context.Parameters.Length < 1)
                throw new CommandWrongUsageException(Context);

            //TODO: throw UserFriendlyException on bad input
            uint additionalExperience = await Context.Parameters.GetAsync<uint>(0);
            if (Context.Parameters.Length == 1)
            {
                if (Context.Actor.Type == KnownActorTypes.Console)
                    throw new CommandWrongUsageException(Context);

                UnturnedUser uPlayer = (UnturnedUser) Context.Actor;

                await UniTask.SwitchToMainThread();
                uPlayer.Player.skills.askPay(additionalExperience);
                await uPlayer.PrintMessageAsync(m_StringLocalizer["experience:success", new {Experience = additionalExperience.ToString()}]);
            }
            else
            {
                string searchTerm = Context.Parameters[1];
                if (!PlayerTool.tryGetSteamPlayer(searchTerm, out SteamPlayer player))
                    throw new UserFriendlyException(m_StringLocalizer["general:invalid_player",
                        new {Player = searchTerm}]);

                await UniTask.SwitchToMainThread();
                player.player.skills.askPay(additionalExperience);
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["experience:gave",
                    new {Player = player.playerID.characterName, Experience = additionalExperience.ToString()}]);
            }
        }
    }
}
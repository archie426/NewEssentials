﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace NewEssentials.Commands
{
    [Command("explode")]
    [CommandAlias("boom")]
    [CommandDescription("Create an explosion where you are looking")]
    [CommandSyntax("[radius] [damage]")]
    [CommandActor(typeof(UnturnedUser))]
    public class CExplode : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CExplode(IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 2)
                throw new CommandWrongUsageException(Context);

            float radius = 10f;
            if (Context.Parameters.Length > 0)
            {
                string radiusTerm = Context.Parameters[0];
                if (!float.TryParse(radiusTerm, out radius))
                    throw new UserFriendlyException(m_StringLocalizer["explode:bad_radius", new {Radius = radiusTerm}]);
            }

            float damage = 200f;
            if (Context.Parameters.Length > 1)
            {
                string damageTerm = Context.Parameters[1];
                if(!float.TryParse(damageTerm, out damage))
                    throw new UserFriendlyException(m_StringLocalizer["explode:bad_damage", new {Damage = damageTerm}]);
            }

            UnturnedUser uPlayer = (UnturnedUser) Context.Actor;
            Transform aim = uPlayer.Player.look.aim;
            if (!PhysicsUtility.raycast(new Ray(aim.position, aim.forward), out RaycastHit hit, 512f,
                RayMasks.DAMAGE_SERVER))
                throw new UserFriendlyException(m_StringLocalizer["explode:failure"]);

            if (hit.transform == null)
                throw new UserFriendlyException(m_StringLocalizer["explode:failure"]);

            await UniTask.SwitchToMainThread();
            EffectManager.sendEffect(20, EffectManager.MEDIUM, hit.point);
            DamageTool.explode(hit.point, radius, EDeathCause.KILL, uPlayer.SteamId, damage, damage, damage, damage,
                damage, damage, damage, damage, out List<EPlayerKill> kills);

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["explode:success"]);
        }
    }
}
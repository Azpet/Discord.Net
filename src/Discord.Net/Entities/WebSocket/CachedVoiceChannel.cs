﻿using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Model = Discord.API.Channel;

namespace Discord
{
    internal class CachedVoiceChannel : VoiceChannel, ICachedGuildChannel
    {
        public new DiscordSocketClient Discord => base.Discord as DiscordSocketClient;
        public new CachedGuild Guild => base.Guild as CachedGuild;

        public IReadOnlyCollection<IGuildUser> Members 
            => Guild.VoiceStates.Where(x => x.Value.VoiceChannel.Id == Id).Select(x => Guild.GetUser(x.Key)).ToImmutableArray();

        public CachedVoiceChannel(CachedGuild guild, Model model)
            : base(guild, model)
        {
        }

        public override Task<IGuildUser> GetUserAsync(ulong id) 
            => Task.FromResult(GetUser(id));
        public override Task<IReadOnlyCollection<IGuildUser>> GetUsersAsync() 
            => Task.FromResult(Members);
        public IGuildUser GetUser(ulong id)
        {
            var user = Guild.GetUser(id);
            if (user != null && user.VoiceChannel.Id == Id)
                return user;
            return null;
        }

        public override async Task<IAudioClient> ConnectAsync()
        {
            var audioMode = Discord.AudioMode;
            if (audioMode == AudioMode.Disabled)
                throw new InvalidOperationException($"Audio is not enabled on this client, {nameof(DiscordSocketConfig.AudioMode)} in {nameof(DiscordSocketConfig)} must be set.");

            await Discord.ApiClient.SendVoiceStateUpdateAsync(Guild.Id, Id,
                (audioMode & AudioMode.Incoming) == 0, 
                (audioMode & AudioMode.Outgoing) == 0).ConfigureAwait(false);
            return null;
            //TODO: Block and return
        }

        public CachedVoiceChannel Clone() => MemberwiseClone() as CachedVoiceChannel;

        ICachedChannel ICachedChannel.Clone() => Clone();
    }
}

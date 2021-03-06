using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FMBot.Bot.Configurations;
using FMBot.Bot.Extensions;
using FMBot.Bot.Interfaces;
using FMBot.Bot.Resources;
using FMBot.Bot.Services;
using FMBot.Domain;
using static FMBot.Bot.FMBotUtil;

namespace FMBot.Bot.Commands
{
    public class StaticCommands : ModuleBase
    {
        private readonly EmbedBuilder _embed;
        private readonly EmbedAuthorBuilder _embedAuthor;
        private readonly GuildService _guildService;

        private readonly CommandService _service;
        private readonly IPrefixService _prefixService;
        private readonly UserService _userService;
        private readonly FriendsService _friendService;

        public StaticCommands(
            CommandService service,
            IPrefixService prefixService,
            UserService userService,
            FriendsService friendsService)
        {
            this._service = service;
            this._prefixService = prefixService;
            this._userService = userService;
            this._friendService = friendsService;
            this._guildService = new GuildService();
            this._embed = new EmbedBuilder()
                .WithColor(DiscordConstants.LastFMColorRed);
            this._embedAuthor = new EmbedAuthorBuilder();
        }

        [Command("invite", RunMode = RunMode.Async)]
        [Summary("Info for inviting the bot to a server")]
        [Alias("server")]
        public async Task InviteAsync()
        {
            var SelfID = this.Context.Client.CurrentUser.Id.ToString();

            this._embed.AddField("Invite the bot to your own server with the link below:",
                "https://discordapp.com/oauth2/authorize?client_id=" + SelfID + "&scope=bot&permissions=" +
                Constants.InviteLinkPermissions);

            this._embed.AddField("Join the FMBot server for support and updates:",
                "https://discord.gg/srmpCaa");

            this._embed.AddField("Support us on OpenCollective:",
                "https://opencollective.com/fmbot");

            if (IsBotSelfHosted(this.Context.Client.CurrentUser.Id))
            {
                this._embed.AddField("Note:",
                    "This instance of .fmbot is self-hosted and could differ from the 'official' .fmbot.");
            }

            await this.Context.Channel.SendMessageAsync("", false, this._embed.Build());
            this.Context.LogCommandUsed();
        }

        [Command("info", RunMode = RunMode.Async)]
        [Summary("Please donate if you like this bot!")]
        [Alias("donate", "github", "gitlab", "issues", "bugs")]
        public async Task InfoAsync()
        {
            var selfId = this.Context.Client.CurrentUser.Id.ToString();

            this._embed.AddField("Invite the bot to your own server with the link below:",
                "https://discordapp.com/oauth2/authorize?client_id=" + selfId + "&scope=bot&permissions=" +
                Constants.InviteLinkPermissions);

            this._embed.AddField("Support the bot development and hosting:",
                "https://opencollective.com/fmbot");

            this._embed.AddField("Post issues and feature requests here:",
                "https://github.com/fmbot-discord/fmbot/issues/new/choose");

            this._embed.AddField("View the code on Github:",
                "https://github.com/fmbot-discord/fmbot");

            this._embed.AddField("Or on GitLab:",
                "https://gitlab.com/Bitl/FMBot_Discord");

            this._embed.AddField("Join the FMBot server for support and updates:",
                "https://discord.gg/srmpCaa");

            if (IsBotSelfHosted(this.Context.Client.CurrentUser.Id))
            {
                this._embed.AddField("Note:",
                    "This instance of .fmbot is self-hosted and could differ from the 'official' .fmbot.");
            }

            await this.Context.Channel.SendMessageAsync("", false, this._embed.Build());
            this.Context.LogCommandUsed();
        }

        [Command("status", RunMode = RunMode.Async)]
        [Summary("Displays bot stats.")]
        public async Task StatusAsync()
        {
            var selfUser = this.Context.Client.CurrentUser;

            this._embedAuthor.WithIconUrl(selfUser.GetAvatarUrl());
            this._embedAuthor.WithName(selfUser.Username);
            this._embedAuthor.WithUrl("https://fmbot.xyz/");

            this._embed.WithAuthor(this._embedAuthor);

            var currentProcess = Process.GetCurrentProcess();

            var startTime = DateTime.Now - currentProcess.StartTime;
            var currentMemoryUsage = currentProcess.WorkingSet64;
            var peakMemoryUsage = currentProcess.PeakWorkingSet64;

            var client = this.Context.Client as DiscordShardedClient;

            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            var description = "";
            description += $"**Bot Uptime:** `{startTime.ToReadableString()}`\n";
            description += $"**Server Uptime:** `{GlobalVars.SystemUpTime().ToReadableString()}`\n";
            description += $"**Usercount:** `{(await this._userService.GetTotalUserCountAsync()).ToString()}`\n";
            description += $"**Friendcount:** `{await this._friendService.GetTotalFriendCountAsync()}`\n";
            description += $"**Discord usercount:** `{client.Guilds.Select(s => s.MemberCount).Sum()}`\n";
            description += $"**Servercount:** `{client.Guilds.Count}`\n";
            description += $"**Commands used:** `{Statistics.CommandsExecuted.Value}`\n";
            description += $"**Last.FM API calls:** `{Statistics.LastfmApiCalls.Value}`\n";
            description += $"**Memory usage:** `{currentMemoryUsage.ToFormattedByteString()}` (Peak: `{peakMemoryUsage.ToFormattedByteString()}`)\n";
            description += $"**Average latency:** `{Math.Round(client.Shards.Select(s => s.Latency).Average(), 2) + "ms`"}\n";
            description += $"**Shards:** `{client.Shards.Count}`\n";
            description += $"**Bot version:** `{assemblyVersion}`\n";
            description += $"**Self-hosted:** `{IsBotSelfHosted(this.Context.Client.CurrentUser.Id).ToString()}`\n";

            this._embed.WithDescription(description);

            await this.Context.Channel.SendMessageAsync("", false, this._embed.Build());
            this.Context.LogCommandUsed();
        }

        [Command("help", RunMode = RunMode.Async)]
        [Summary("Quick help summary to get started.")]
        [Alias("bot")]
        public async Task HelpAsync()
        {
            var customPrefix = true;
            var prefix = this._prefixService.GetPrefix(this.Context.Guild?.Id);
            if (prefix == null)
            {
                prefix = ConfigData.Data.Bot.Prefix;
                customPrefix = false;
            }

            this._embed.WithTitle(".fmbot Quick Start Guide");

            var mainCommand = "fm";
            if (!customPrefix)
            {
                mainCommand = "";
            }

            this._embed.AddField($"Main command `{prefix}{mainCommand}`",
                "Displays last scrobbles, and looks different depending on the mode you've set.");

            this._embed.AddField($"Setting up: `{prefix}set lastfmusername`",
                $"For more settings, please use `{prefix}set help`.");

            if (customPrefix)
            {
                this._embed.AddField("Custom prefix:",
                    $"This server has the `{prefix}` prefix.\n" +
                    $"Note that the documentation has the `.fm` prefix everywhere, so you'll have to replace `.fm` with `{prefix}`.");
            }

            this._embed.AddField("For more commands and info, please read the documentation here:",
                "https://fmbot.xyz/commands/");

            if (IsBotSelfHosted(this.Context.Client.CurrentUser.Id))
            {
                this._embed.AddField("Note:",
                    "This instance of .fmbot is self-hosted and could differ from the 'official' .fmbot. \n" +
                    "Keep in mind that the instance might not be fully up to date or other users might not be registered.");
            }

            await this.Context.Channel.SendMessageAsync("", false, this._embed.Build());
            this.Context.LogCommandUsed();
        }


        [Command("fullhelp", RunMode = RunMode.Async)]
        [Summary("Displays this list.")]
        public async Task FullHelpAsync()
        {
            var prefix = ConfigData.Data.Bot.Prefix;

            string description = null;
            var length = 0;

            var builder = new EmbedBuilder();

            foreach (var module in this._service.Modules.OrderByDescending(o => o.Commands.Count()).Where(w =>
                !w.Name.Contains("SecretCommands") && !w.Name.Contains("OwnerCommands") &&
                !w.Name.Contains("AdminCommands") && !w.Name.Contains("GuildCommands")))
            {
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(this.Context);
                    if (result.IsSuccess)
                    {
                        if (!string.IsNullOrWhiteSpace(cmd.Summary))
                        {
                            description += $"{prefix}{cmd.Aliases.First()} - {cmd.Summary}\n";
                        }
                        else
                        {
                            description += $"{prefix}{cmd.Aliases.First()}\n";
                        }
                    }
                }

                if (description.Length < 1024)
                {
                    builder.AddField
                    (module.Name + (module.Summary != null ? " - " + module.Summary : ""),
                        description != null ? description : "");
                }

                length += description.Length;
                description = null;

                if (length < 1990)
                {
                    await this.Context.User.SendMessageAsync("", false, builder.Build());

                    builder = new EmbedBuilder();
                    length = 0;
                }
            }

            if (!this._guildService.CheckIfDM(this.Context))
            {
                await this.Context.Channel.SendMessageAsync("Check your DMs!");
            }
            this.Context.LogCommandUsed();
        }

        private static bool IsBotSelfHosted(ulong botId)
        {
            return !botId.Equals(Constants.BotProductionId) && !botId.Equals(Constants.BotStagingId);
        }
    }
}

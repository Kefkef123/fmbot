﻿using Coolbooru;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FMBot.Data.Entities;
using FMBot.Services;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FMBot.Bot.FMBotModules;
using static FMBot.Bot.FMBotUtil;

namespace FMBot.Bot.Commands
{
    public class SecretCommands : ModuleBase
    {
        private readonly DerpibooruService derpiservice = new DerpibooruService();

        public SecretCommands()
        {
        }

        [Command("fmimage"), Summary("???")]
        public async Task fmimageAsync(string query)
        {
            try
            {
				string FilteredQuery = query + " -explicit -nsfw"
				
                List<CoolItem> imagelist = await derpiservice.GetImages(query);

                var random = new Random();
                int index = random.Next(imagelist.Count);
                var item = imagelist[index];
                var itemEmbed = await CoolStuff.Embed(int.Parse(item.id));

                EmbedBuilder builder = new EmbedBuilder();

                builder.WithAuthor(new EmbedAuthorBuilder
                {
                    Name = itemEmbed.author_name,
                    Url = itemEmbed.author_url
                });

                builder.WithUrl(itemEmbed.provider_url);
                builder.WithDescription("Tags: " + string.Join(", ", itemEmbed.derpibooru_tags));
                builder.WithTitle("An image");
                builder.WithImageUrl("https://" + itemEmbed.thumbnail_url);

                EmbedFooterBuilder efb = new EmbedFooterBuilder();

                efb.Text = "boop";

                builder.WithFooter(efb);

                await Context.Channel.SendMessageAsync("", false, builder.Build());

            }
            catch (Exception e)
            {
                DiscordSocketClient disclient = Context.Client as DiscordSocketClient;
                ExceptionReporter.ReportException(disclient, e);

                await ReplyAsync("oof");
            }
        }
    }
}

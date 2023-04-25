﻿using Discord;
using Discord.Webhook;
using Logging.Net;
using Moonlight.App.Database.Entities;
using Moonlight.App.Events;

namespace Moonlight.App.Services;

public class DiscordNotificationService
{
    private readonly EventSystem Event;
    private readonly ResourceService ResourceService;
    private readonly DiscordWebhookClient Client;
    private readonly string AppUrl;

    public DiscordNotificationService(
        EventSystem eventSystem,
        ConfigService configService,
        ResourceService resourceService)
    {
        Event = eventSystem;
        ResourceService = resourceService;

        var config = configService.GetSection("Moonlight").GetSection("DiscordNotifications");

        if (config.GetValue<bool>("Enable"))
        {
            Logger.Info("Discord notifications enabled");
            
            Client = new(config.GetValue<string>("WebHook"));
            AppUrl = configService.GetSection("Moonlight").GetValue<string>("AppUrl");

            Event.On<User>("supportChat.new", this, OnNewSupportChat);
            Event.On<SupportChatMessage>("supportChat.message", this, OnSupportChatMessage);
            Event.On<User>("supportChat.close", this, OnSupportChatClose);
        }
        else
        {
            Logger.Info("Discord notifications disabled");
        }
    }

    private async Task OnSupportChatClose(User user)
    {
        await SendNotification("", builder =>
        {
            builder.Title = "A new support chat has been marked as closed";
            builder.Color = Color.Red;
            builder.AddField("Email", user.Email);
            builder.AddField("Firstname", user.FirstName);
            builder.AddField("Lastname", user.LastName);
            builder.Url = $"{AppUrl}/admin/support/view/{user.Id}";
        });
    }

    private async Task OnSupportChatMessage(SupportChatMessage message)
    {
        if(message.Sender == null)
            return;
        
        await SendNotification("", builder =>
        {
            builder.Title = "New message in support chat";
            builder.Color = Color.Blue;
            builder.AddField("Message", message.Content);
            builder.Author = new EmbedAuthorBuilder()
                .WithName($"{message.Sender.FirstName} {message.Sender.LastName}")
                .WithIconUrl(ResourceService.Avatar(message.Sender));
            builder.Url = $"{AppUrl}/admin/support/view/{message.Recipient.Id}";
        });
    }

    private async Task OnNewSupportChat(User user)
    {
        await SendNotification("", builder =>
        {
            builder.Title = "A new support chat has been marked as active";
            builder.Color = Color.Green;
            builder.AddField("Email", user.Email);
            builder.AddField("Firstname", user.FirstName);
            builder.AddField("Lastname", user.LastName);
            builder.Url = $"{AppUrl}/admin/support/view/{user.Id}";
        });
    }

    private async Task SendNotification(string content, Action<EmbedBuilder>? embed = null)
    {
        var e = new EmbedBuilder();
        embed?.Invoke(e);

        await Client.SendMessageAsync(
            content, 
            false, 
            new []{e.Build()}, 
            "Moonlight Notification",
            ResourceService.Image("logo.svg")
        );
    }
}
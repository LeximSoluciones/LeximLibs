using System;
using Lexim.Logging.Slack.Models;

namespace Lexim.Logging.Slack
{
    internal class SlackMessageBuilder
    {
        private readonly string _webHookUrl;
        private readonly SlackClient _client;
        private readonly Payload _payload;

        public SlackMessageBuilder(string webHookUrl)
        {
            this._webHookUrl = webHookUrl;
            this._client = new SlackClient();
            this._payload = new Payload();
        }

        public static SlackMessageBuilder Build(string webHookUrl)
        {
            return new SlackMessageBuilder(webHookUrl);
        }

        public SlackMessageBuilder WithMessage(string message)
        {
            this._payload.Text = message;

            return this;
        }

        public SlackMessageBuilder ToChannel(string channel)
        {
            this._payload.Channel = channel;

            return this;
        }

        public SlackMessageBuilder AsUser(string username)
        {
            this._payload.Username = username;

            return this;
        }

        public SlackMessageBuilder WithIcon(string icon)
        {
            if (Uri.TryCreate(icon, UriKind.Absolute, out var uriResult) && uriResult.Scheme == Uri.UriSchemeHttp)
            {
                _payload.IconUrl = icon;
            }
            else
            {
                _payload.IconEmoji = icon;
            }

            return this;
        }

        public SlackMessageBuilder AddAttachment(Attachment attachment)
        {
            this._payload.Attachments.Add(attachment);

            return this;
        }

        public SlackMessageBuilder OnError(Action<Exception> error)
        {
            this._client.Error += error;

            return this;
        }

        public void Send()
        {
            this._client.Send(this._webHookUrl, this._payload.ToJson());
        }
    }
}
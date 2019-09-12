using System;
using System.Linq;
using Lexim.Logging.Slack;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using Lexim.Logging.Slack.Models;
using NLog;
using NLog.Targets;

namespace Lexim.Logging.Slack
{
    [Target("Slack")]
    internal class SlackTarget : TargetWithLayout
    {
        [RequiredParameter]
        public string WebHookUrl { get; set; }

        public SimpleLayout Channel { get; set; }

        public SimpleLayout Username { get; set; }

        public string Icon { get; set; }

        public bool Compact { get; set; }

        protected override void InitializeTarget()
        {
            if (String.IsNullOrWhiteSpace(this.WebHookUrl))
                throw new ArgumentOutOfRangeException("WebHookUrl", "Webhook URL cannot be empty.");

            Uri uriResult;
            if (!Uri.TryCreate(this.WebHookUrl, UriKind.Absolute, out uriResult))
                throw new ArgumentOutOfRangeException("WebHookUrl", "Webhook URL is an invalid URL.");

            if (!String.IsNullOrWhiteSpace(this.Channel.Text)
                && (!this.Channel.Text.StartsWith("#") && !this.Channel.Text.StartsWith("@") && !this.Channel.Text.StartsWith("${")))
                throw new ArgumentOutOfRangeException("Channel", "The Channel name is invalid. It must start with either a # or a @ symbol or use a variable.");

            base.InitializeTarget();
        }

        protected override void Write(AsyncLogEventInfo info)
        {
            try
            {
                this.SendToSlack(info);
            }
            catch (Exception e)
            {
                info.Continuation(e);
            }
        }

        private SlackMessageBuilder CreateMessageBuilder(AsyncLogEventInfo info)
        {
            var slack =
                SlackMessageBuilder
                    .Build(this.WebHookUrl)
                    .OnError(e => info.Continuation(e));

            if (!String.IsNullOrWhiteSpace(this.Channel.Render(info.LogEvent)))
                slack.ToChannel(this.Channel.Render(info.LogEvent));

            if (!String.IsNullOrWhiteSpace(this.Icon))
                slack.WithIcon(this.Icon);

            if (!String.IsNullOrWhiteSpace(this.Username.Render(info.LogEvent)))
                slack.AsUser(this.Username.Render(info.LogEvent));

            return slack;
        }

        private void SendToSlack(AsyncLogEventInfo info)
        {
            var ex = info.LogEvent.Exception;
            if (ex != null)
            {
                var slack = CreateMessageBuilder(info);

                var index = 0;
                var message = "";

                while (ex != null)
                {
                    var append =
                        !string.IsNullOrEmpty(ex.StackTrace)
                            ? $">{GetArrow(index)}`{ex.GetType().FullName}`: *{ex.Message}* ```{ex.StackTrace}```\r\n"
                            : $">{GetArrow(index)}`{ex.GetType().FullName}`: *{ex.Message}*\r\n";

                    if ((message + append).Length > 4000)
                    {
                        slack.WithMessage(message);
                        slack.Send();
                        slack = CreateMessageBuilder(info);
                        message = append;
                    }
                    else
                    {
                        message += append;
                    }

                    ex = ex.InnerException;
                    index++;
                }

                slack.WithMessage(message);
                slack.Send();
            }
            else
            {
                var slack = CreateMessageBuilder(info);
                var message = Layout.Render(info.LogEvent);
                slack.WithMessage(message);
                slack.Send();
            }
        }

        private string GetArrow(int index)
        {
            if (index < 1)
                return "";
            return "`" + new string('-', index * 2) + ">` ";
        }
    }
}
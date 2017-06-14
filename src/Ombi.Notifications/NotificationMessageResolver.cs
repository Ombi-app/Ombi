using System.Collections.Generic;
using Ombi.Store.Entities;

namespace Ombi.Notifications
{
    public class NotificationMessageContent
    {
        public string Subject { get; set; }
        public string Message { get; set; }
    }
    public class NotificationMessageCurlys
    {
        public NotificationMessageCurlys(string requestedUser, string title, string requestedDateTime, string type, string issue)
        {
            RequestedUser = requestedUser;
            Title = title;
            RequestedDate = requestedDateTime;
            Type = type;
            Issue = issue;
        }
        private string RequestedUser { get; }
        private string Title { get; }
        private string RequestedDate { get; }
        private string Type { get; }
        private string Issue { get; }

        public Dictionary<string, string> Curlys => new Dictionary<string, string>
        {
            {nameof(RequestedUser), RequestedUser },
            {nameof(Title), Title },
            {nameof(RequestedDate), RequestedDate },
            {nameof(Type), Type },
            {nameof(Issue), Issue }
        };
    }
    
    public class NotificationMessageResolver
    {
        /// <summary>
        /// The start character '{'
        /// </summary>
        private const char StartChar = (char)123;
        /// <summary>
        /// The end character '}'
        /// </summary>
        private const char EndChar = (char)125;

        /// <summary>
        /// Parses the message.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        public NotificationMessageContent ParseMessage(NotificationTemplates notification, NotificationMessageCurlys c)
        {
            return Resolve(notification.Message, notification.Subject, c.Curlys);
        }

        /// <summary>
        /// Resolves the specified message curly fields.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private NotificationMessageContent Resolve(string body, string subject, IReadOnlyDictionary<string, string> parameters)
        {
            // Find the fields
            var bodyFields = FindCurlyFields(body);
            var subjectFields = FindCurlyFields(subject);

            body = ReplaceFields(bodyFields, parameters, body);
            subject = ReplaceFields(subjectFields, parameters, subject);

            return new NotificationMessageContent { Message = body ?? string.Empty, Subject = subject ?? string.Empty };
        }

        /// <summary>
        /// Finds the curly fields.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private IEnumerable<string> FindCurlyFields(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return new List<string>();
            }
            var insideCurly = false;
            var fields = new List<string>();
            var currentWord = string.Empty;
            var chars = message.ToCharArray();

            foreach (var c in chars)
            {
                if (char.IsWhiteSpace(c))
                {
                    currentWord = string.Empty;
                    continue;
                }

                if (c == StartChar) // Start of curly '{'
                {
                    insideCurly = true;
                    continue;
                }

                if (c == EndChar) // End of curly '}'
                {
                    fields.Add(currentWord); // We have finished the curly, add the word into the list
                    currentWord = string.Empty;
                    insideCurly = false;
                    continue;
                }

                if (insideCurly)
                {
                    currentWord += c.ToString(); // Add the character onto the word.
                }
            }

            return fields;
        }

        /// <summary>
        /// Replaces the fields.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="mainText">The main text.</param>
        /// <returns></returns>
        private string ReplaceFields(IEnumerable<string> fields, IReadOnlyDictionary<string, string> parameters, string mainText)
        {
            foreach (var field in fields)
            {
                string outString;
                if (parameters.TryGetValue(field, out outString))
                {
                    mainText = mainText.Replace($"{{{field}}}", outString);
                }
            }
            return mainText;
        }
    }
}
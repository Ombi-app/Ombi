using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ombi.Store.Entities;

namespace Ombi.Notifications
{
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
            var content = Resolve(notification.Message, notification.Subject, c.Curlys);
            content.Image = c.PosterImage;
            return content;
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
            var bodyFields = FindFields(body, StartChar, EndChar);
            var subjectFields = FindFields(subject, StartChar, EndChar);

            //var conditionalFields = FindFields(body, '<', '>');
            //ProcessConditions(conditionalFields, parameters);

            body = ReplaceFields(bodyFields, parameters, body);
            subject = ReplaceFields(subjectFields, parameters, subject);
            return new NotificationMessageContent { Message = body ?? string.Empty, Subject = subject ?? string.Empty, Data = parameters };
        }

        public IEnumerable<string> ProcessConditions(IEnumerable<string> conditionalFields, IReadOnlyDictionary<string, string> parameters)
        {
            foreach (var f in conditionalFields)
            {
                var field = f.ToLower();
                if (field.StartsWith("if"))
                {
                    var ifPosition = field.IndexOf("if", StringComparison.Ordinal);
                    Console.WriteLine(ifPosition);
                    var identifierStart = field.Substring(ifPosition + 3);
                    Console.WriteLine(identifierStart);
                    var identifierEnd = identifierStart.IndexOf(' ');
                    Console.WriteLine(identifierEnd);

                    var identitifier = identifierStart.Substring(ifPosition, identifierEnd);

                    if (identitifier.Equals("type"))
                    {
                        // Find the operator == or !=
                        var stringWithoutIdentifier = identifierStart.Substring(identitifier.Length + 1);
                        var operatorValue = stringWithoutIdentifier.Substring(0,2);

                        var stringWithoutOperator = stringWithoutIdentifier.Substring(operatorValue.Length + 1);
                        var endPosition = stringWithoutOperator.IndexOf(' ');
                        var comparison = stringWithoutOperator.Substring(0, endPosition);
                        
                        if (operatorValue == "==")
                        {
                            var type = (RequestType)int.Parse(parameters["Type"]);
                            if (comparison.Equals("Movie", StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (type == RequestType.Movie)
                                {
                                    // Get the text
                                    var stringWithoutComparison = stringWithoutOperator.Substring(comparison.Length + 2);
                                    var endString = stringWithoutComparison.IndexOf(' ');
                                    var text = stringWithoutComparison.Substring(0, endString - 1);
                                    field = text;
                                }
                                else
                                {
                                    // Get the text in the ELSE
                                    var stringWithoutComparison = stringWithoutOperator.Substring(comparison.Length + 2);
                                    var elseIndex = stringWithoutComparison.IndexOf("else", StringComparison.CurrentCultureIgnoreCase);
                                    var endIndex = stringWithoutComparison.IndexOf(' ');
                                    if (elseIndex >= 0)
                                    {
                                        var elseString = stringWithoutComparison.Substring(elseIndex, endIndex);
                                        
                                    }
                                    else
                                    {
                                        // No else
                                    }
                                }
                            }
                            else if(comparison.Equals("TvShow", StringComparison.CurrentCultureIgnoreCase) || comparison.Equals("Tv", StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (type == RequestType.TvShow)
                                {

                                }
                            }
                        }
                        else if (operatorValue == "!=")
                        {
                            
                        }
                    }
                    
                }
            }

            return conditionalFields;
        }

        /// <summary>
        /// Finds the curly fields.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private IEnumerable<string> FindFields(string message, char start, char end)
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

                if (c == start) // Start of curly '{'
                {
                    insideCurly = true;
                    continue;
                }

                if (c == end) // End of curly '}'
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
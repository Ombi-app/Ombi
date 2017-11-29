using System;

namespace Ombi.Notifications.Exceptions
{
    public class TemplateMissingException : Exception
    {
        public TemplateMissingException() : base()
        {
            
        }

        public TemplateMissingException(string msg) : base(msg)
        {
            
        }
    }
}
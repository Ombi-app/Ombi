using System;

namespace Ombi.Helpers
{
    public class RuleNotFoundException : Exception
    {
        public RuleNotFoundException(string rule) : base($"rule: {rule} was not found")
        {
            
        }
    }
}
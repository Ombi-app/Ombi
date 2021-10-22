using Ombi.Core.Engine;
namespace Ombi.Core.Rule
{
    public class RuleResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public ErrorCode ErrorCode { get; set; }
    }
}
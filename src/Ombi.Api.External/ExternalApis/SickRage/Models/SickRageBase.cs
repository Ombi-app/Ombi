namespace Ombi.Api.External.ExternalApis.SickRage.Models
{
    public abstract class SickRageBase<T>
    {
        public T data { get; set; }
        public string message { get; set; }
        public string result { get; set; }
    }
}

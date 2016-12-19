namespace Ombi.Api.Models.SickRage
{
    public abstract class SickRageBase<T>
    {
        public T data { get; set; }
        public string message { get; set; }
        public string result { get; set; }
    }
}
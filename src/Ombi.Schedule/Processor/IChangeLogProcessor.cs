using System.Threading.Tasks;

namespace Ombi.Core.Processor
{
    public interface IChangeLogProcessor
    {
        Task<UpdateModel> Process();
    }
}
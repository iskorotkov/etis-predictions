using System.Text;
using System.Threading.Tasks;

namespace EtisPredictions.Preprocessor.Passes
{
    public interface IPass
    {
        Task<string> Do(string source, string destination, Encoding encoding);
    }
}

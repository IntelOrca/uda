using uda.Intermediate;

namespace uda.Strategy
{
    internal interface IDecompileStrategy
    {
        void Process(Function function);
    }
}

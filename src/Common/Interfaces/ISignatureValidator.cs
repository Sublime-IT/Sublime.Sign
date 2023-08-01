namespace Sublime.Sign.Common.Interfaces;

public interface ISignatureValidator<T>
{
    bool Validate(T unsigned, T signature);
}
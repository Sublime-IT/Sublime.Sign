namespace Sublime.Sign.Common.Interfaces;

public interface ISignatureCreator<T>
{
    T Sign(T unsigned);
}


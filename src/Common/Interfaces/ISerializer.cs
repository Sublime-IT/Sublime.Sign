namespace Sublime.Sign.Common.Interfaces;

public interface ISecureSerializer<T> {
    public (string content, string signature) Serialize(T @object);
    T Deserialize((string content, string signature) serialized);
}
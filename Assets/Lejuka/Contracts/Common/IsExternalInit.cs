namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Compiler shim. Unity's .NET profile lacks this type, which C# 9 records and init-only setters require.
    /// </summary>
    internal static class IsExternalInit
    {
    }
}

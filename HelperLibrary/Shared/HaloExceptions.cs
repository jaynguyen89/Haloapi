namespace HelperLibrary.Shared;

public sealed class ParamsNullException(string message): Exception(message);

public sealed class SimplifiedRegionalPhoneNumberNoCommaException(): Exception("The simplified Phone Number must be delimited by comma.");
public sealed class SimplifiedRegionalPhoneNumberTokenException(): Exception("The simplified Phone Number must contain exactly 2 tokens.");
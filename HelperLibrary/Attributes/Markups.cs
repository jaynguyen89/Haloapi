using System.ComponentModel;

namespace HelperLibrary.Attributes;

[AttributeUsage(AttributeTargets.All)]
public sealed class ProblematicAttribute(string message): DescriptionAttribute(message);

[AttributeUsage(AttributeTargets.All)]
public sealed class CautiousAttribute(string message): DescriptionAttribute(message);

[AttributeUsage(AttributeTargets.All)]
public sealed class AttentionAttribute(string message): DescriptionAttribute(message);

[AttributeUsage(AttributeTargets.All)]
public sealed class TodoAttribute(string message): DescriptionAttribute(message);
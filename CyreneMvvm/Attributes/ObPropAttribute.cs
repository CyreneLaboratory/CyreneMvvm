using System;

namespace CyreneMvvm.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ObPropAttribute : Attribute;

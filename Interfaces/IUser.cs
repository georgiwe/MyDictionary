namespace MyDictionary.Interfaces
{
    using System;

    public interface IUser : ICloneable
    {
        string Name { get; }
    }
}

using System;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CustomPrimitiveTypes
{
    public abstract record TypeId<TId> : ITypeId
        where TId : TypeId<TId>
    {
        public Guid IdValue { get; }

        protected TypeId(Guid idValue)
        {
            if (idValue == default) throw new ArgumentException(nameof(idValue));
            IdValue = idValue;
        }

        public static TId CreateNew()
        {
            var guid = Guid.NewGuid();
            return (TId)Activator.CreateInstance(typeof(TId), args: guid)!;
        }

        public static TId CreateExisting(Guid idValue)
        {
            if (idValue == default) throw new ArgumentException(nameof(idValue));
            return (TId)Activator.CreateInstance(typeof(TId), args: idValue)!;
        }

        public static TId Parse(object? value)
        {
            if (value is string stringValue)
            {
                return CreateExisting(new Guid(stringValue));
            }
            else if (value is Guid guidValue)
            {
                return CreateExisting(guidValue);
            }
            else if (value == null)
            {
                return null;
            }
            else
            {
                throw new NotSupportedException($"Cannot parse TypeId from {value.GetType()}!");
            }
        }

        public static bool TryParse(string id, out TId result)
            => (result = Guid.TryParse(id, out var r) ? CreateExisting(r) : null) is not null;

        public sealed override string ToString() => IdValue.ToString();
    }

}


using System;
using System.Collections;
using System.Collections.Generic;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Allows fine grained configuration of DotVVM services.
    /// </summary>
    public class DotvvmServiceCollection : IDotvvmServiceCollection
    {
        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmServiceCollection" /> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        public DotvvmServiceCollection(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator() => Services.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Services.GetEnumerator();

        public void Add(ServiceDescriptor item) => Services.Add(item);

        public void Clear() => Services.Clear();

        public bool Contains(ServiceDescriptor item) => Services.Contains(item);

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => Services.CopyTo(array, arrayIndex);

        public bool Remove(ServiceDescriptor item) => Services.Remove(item);

        public int Count => Services.Count;
        public bool IsReadOnly => Services.IsReadOnly;
        public int IndexOf(ServiceDescriptor item) => Services.IndexOf(item);

        public void Insert(int index, ServiceDescriptor item) => Services.Insert(index, item);
        public void RemoveAt(int index) => Services.RemoveAt(index);

        public ServiceDescriptor this[int index]
        {
            get => Services[index];
            set => Services[index] = value;
        }
    }
}

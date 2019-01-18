namespace SkypeImTranslator.WebRole.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http.Dependencies;
    using Unity;
    using Unity.Exceptions;

    sealed class UnityResolver : IDependencyResolver
    {
        private static readonly IEnumerable<object> EmptyList = new object[0];
        private readonly IUnityContainer _container;

        public UnityResolver(IUnityContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public object GetService(Type serviceType)
        {
            object resolved = null;

            try
            {
                resolved = _container.Resolve(serviceType);
            }
            catch (ResolutionFailedException)
            {
            }

            return resolved;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            IEnumerable<object> resolved = EmptyList;

            try
            {
                return _container.ResolveAll(serviceType);
            }
            catch (ResolutionFailedException)
            {
            }

            return resolved;
        }

        public IDependencyScope BeginScope()
        {
            return new UnityResolver(_container.CreateChildContainer());
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _container.Dispose();
            }
        }
    }
}
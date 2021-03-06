﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Util;
using Microsoft.Extensions.Logging;
using OpenMod.API.Ioc;

namespace OpenMod.Core.Ioc
{
    public static class ServiceRegistrationHelper
    {
        public static IEnumerable<ServiceRegistration> FindFromAssembly<T>(Assembly assembly, ILogger logger = null) where T: ServiceImplementationAttribute
        {
            var types = assembly.GetLoadableTypes()
                .Where(d => d.IsClass && !d.IsInterface && !d.IsAbstract)
                .ToList();

            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<T>();
                if (attribute == null)
                {
                    continue;
                }

                var interfaces = type.GetInterfaces()
                    .Where(d => d.GetCustomAttribute<ServiceAttribute>() != null)
                    .ToArray();

                if (interfaces.Length == 0)
                {
                    logger?.LogWarning($"Type {type.FullName} in assembly {assembly.FullName} has been marked as ServiceImplementation but does not inherit any services!\nDid you forget to add [Service] to your interfaces?");
                    continue;
                }

                yield return new ServiceRegistration
                {
                    Priority = attribute.Priority,
                    ServiceImplementationType = type,
                    ServiceTypes = interfaces,
                    Lifetime = attribute.Lifetime
                };
            }
        }
    }
}
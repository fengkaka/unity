﻿//===============================================================================
// Microsoft patterns & practices
// Unity Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Practices.Unity.InterceptionExtension.Properties;
using Microsoft.Practices.Unity.Utility;

namespace Microsoft.Practices.Unity.InterceptionExtension
{
    /// <summary>
    /// High-level API for performing interception on existing and new objects.
    /// </summary>
    public static class Intercept
    {
        /// <summary>
        /// Returns a <see cref="IInterceptingProxy"/> for type <paramref name="interceptedType"/> which wraps 
        /// the supplied <paramref name="target"/>.
        /// </summary>
        /// <typeparam name="T">The type to intercept.</typeparam>
        /// <param name="target">The instance to intercept.</param>
        /// <param name="interceptor">The <see cref="IInstanceInterceptor"/> to use when creating the proxy.</param>
        /// <param name="interceptionBehaviors">The interception behaviors for the new proxy.</param>
        /// <param name="additionalInterfaces">Any additional interfaces the proxy must implement.</param>
        /// <returns>A proxy for <paramref name="target"/> compatible with <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException">when <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">when <paramref name="interceptor"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">when <paramref name="interceptionBehaviors"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">when <paramref name="additionalInterfaces"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">when <paramref name="interceptor"/> cannot intercept 
        /// <paramref name="interceptedType"/>.</exception>
        public static T ThroughProxy<T>(
            T target,
            IInstanceInterceptor interceptor,
            IEnumerable<IInterceptionBehavior> interceptionBehaviors,
            IEnumerable<Type> additionalInterfaces)
            where T : class
        {
            return (T)ThroughProxy(typeof(T), target, interceptor, interceptionBehaviors, additionalInterfaces);
        }

        /// <summary>
        /// Returns a <see cref="IInterceptingProxy"/> for type <paramref name="interceptedType"/> which wraps 
        /// the supplied <paramref name="target"/>.
        /// </summary>
        /// <param name="interceptedType">The type to intercept.</param>
        /// <param name="target">The instance to intercept.</param>
        /// <param name="interceptor">The <see cref="IInstanceInterceptor"/> to use when creating the proxy.</param>
        /// <param name="interceptionBehaviors">The interception behaviors for the new proxy.</param>
        /// <param name="additionalInterfaces">Any additional interfaces the proxy must implement.</param>
        /// <returns>A proxy for <paramref name="target"/> compatible with <paramref name="interceptedType"/>.</returns>
        /// <exception cref="ArgumentNullException">when <paramref name="interceptedType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">when <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">when <paramref name="interceptor"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">when <paramref name="interceptionBehaviors"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">when <paramref name="additionalInterfaces"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">when <paramref name="interceptor"/> cannot intercept 
        /// <paramref name="interceptedType"/>.</exception>
        public static object ThroughProxy(
            Type interceptedType,
            object target,
            IInstanceInterceptor interceptor,
            IEnumerable<IInterceptionBehavior> interceptionBehaviors,
            IEnumerable<Type> additionalInterfaces)
        {
            Guard.ArgumentNotNull(interceptedType, "interceptedType");
            Guard.ArgumentNotNull(target, "target");
            Guard.ArgumentNotNull(interceptor, "interceptor");
            Guard.ArgumentNotNull(interceptionBehaviors, "interceptionBehaviors");
            Guard.ArgumentNotNull(additionalInterfaces, "additionalInterfaces");

            if (!interceptor.CanIntercept(interceptedType))
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.InterceptionNotSupported,
                        interceptedType.FullName),
                    "interceptedType");
            }

            IEnumerable<Type> allAdditionalInterfaces =
                interceptionBehaviors.SelectMany(ib => GetRequiredInterfaces(ib)).Concat(additionalInterfaces);

            IInterceptingProxy proxy =
                (IInterceptingProxy)interceptor.CreateProxy(interceptedType, target, allAdditionalInterfaces.ToArray());

            foreach (IInterceptionBehavior interceptionBehavior in interceptionBehaviors)
            {
                proxy.AddInterceptionBehavior(interceptionBehavior);
            }

            return proxy;
        }

        /// <summary>
        /// Creates a new instance of type <typeparamref name="T"/> that is intercepted with the behaviors in 
        /// <paramref name="interceptionBehaviors"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to create.</typeparam>
        /// <param name="interceptor">The <see cref="ITypeInterceptor"/> to use when creating the proxy.</param>
        /// <param name="interceptionBehaviors">The interception behaviors for the new proxy.</param>
        /// <param name="additionalInterfaces">Any additional interfaces the instance must implement.</param>
        /// <param name="constructorParameters">The arguments for the creation of the new instance.</param>
        /// <returns>A proxy for <paramref name="target"/> compatible with <paramref name="interceptedType"/>.</returns>
        /// <exception cref="ArgumentNullException">when <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">when <paramref name="interceptor"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">when <paramref name="interceptionBehaviors"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">when <paramref name="additionalInterfaces"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">when <paramref name="interceptor"/> cannot intercept 
        /// <typeparamref name="T"/>.</exception>
        public static T NewInstance<T>(
            ITypeInterceptor interceptor,
            IEnumerable<IInterceptionBehavior> interceptionBehaviors,
            IEnumerable<Type> additionalInterfaces,
            params object[] constructorParameters)
            where T : class
        {
            return (T)NewInstance(typeof(T), interceptor, interceptionBehaviors, additionalInterfaces, constructorParameters);
        }

        /// <summary>
        /// Creates a new instance of type <paramref name="type"/> that is intercepted with the behaviors in 
        /// <paramref name="interceptionBehaviors"/>.
        /// </summary>
        /// <param name="type">The type of the object to create.</param>
        /// <param name="interceptor">The <see cref="ITypeInterceptor"/> to use when creating the proxy.</param>
        /// <param name="interceptionBehaviors">The interception behaviors for the new proxy.</param>
        /// <param name="additionalInterfaces">Any additional interfaces the instance must implement.</param>
        /// <param name="constructorParameters">The arguments for the creation of the new instance.</param>
        /// <returns>A proxy for <paramref name="target"/> compatible with <paramref name="interceptedType"/>.</returns>
        /// <exception cref="ArgumentNullException">when <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">when <paramref name="interceptor"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">when <paramref name="interceptionBehaviors"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">when <paramref name="additionalInterfaces"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">when <paramref name="interceptor"/> cannot intercept 
        /// <paramref name="type"/>.</exception>
        public static object NewInstance(
            Type type,
            ITypeInterceptor interceptor,
            IEnumerable<IInterceptionBehavior> interceptionBehaviors,
            IEnumerable<Type> additionalInterfaces,
            params object[] constructorParameters)
        {
            Guard.ArgumentNotNull(type, "type");
            Guard.ArgumentNotNull(interceptor, "interceptor");
            Guard.ArgumentNotNull(interceptionBehaviors, "interceptionBehaviors");
            Guard.ArgumentNotNull(additionalInterfaces, "additionalInterfaces");

            if (!interceptor.CanIntercept(type))
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.InterceptionNotSupported,
                        type.FullName),
                    "type");
            }

            Type implementationType = type;

            IEnumerable<Type> allAdditionalInterfaces =
                interceptionBehaviors.SelectMany(ib => GetRequiredInterfaces(ib)).Concat(additionalInterfaces);

            Type interceptionType = interceptor.CreateProxyType(implementationType, allAdditionalInterfaces.ToArray());

            IInterceptingProxy proxy =
                (IInterceptingProxy)Activator.CreateInstance(interceptionType, constructorParameters);

            foreach (IInterceptionBehavior interceptionBehavior in interceptionBehaviors)
            {
                proxy.AddInterceptionBehavior(interceptionBehavior);
            }

            return proxy;
        }

        private static IEnumerable<Type> GetRequiredInterfaces(IInterceptionBehavior interceptionBehavior)
        {
            return interceptionBehavior.GetRequiredInterfaces();
        }
    }
}
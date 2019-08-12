using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ProxyFactory
{
    public class ProxyFactory
    {
        private const string ProxySuffix = "Proxy";
        private const string AssemblyName = "ProxyAssembly";
        private const string ModuleName = "ProxyModule";
        private const string HandlerName = "Handler";
        private const string ObjectReferenceName = "ObjectReference";

        private readonly IDictionary<string, Type> typeMap;

        public enum ProxyType
        {
            Inheritance,
            Interfaces
        }

        public static ProxyFactory Instance { get; }

        static ProxyFactory()
        {
            Instance = new ProxyFactory();
        }

        private ProxyFactory()
        {
            typeMap = new ConcurrentDictionary<string, Type>();
        }

        public TImplementationType Create<TImplementationType>(
            IProxyInvocationHandler handler,
            params object[] args)
            where TImplementationType : class
        {
            return Create<TImplementationType, TImplementationType>(handler, ProxyType.Inheritance, args);
        }

        public TBaseType Create<TImplementationType, TBaseType>(
            IProxyInvocationHandler handler,
            ProxyType proxyType,
            params object[] args)
            where TImplementationType : class, TBaseType
            where TBaseType : class
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var objectType = typeof(TImplementationType);
            var typeName = $"{objectType.FullName}{ProxySuffix}";

            if (!typeMap.TryGetValue(typeName, out var type))
            {
                type = BuildType<TImplementationType, TBaseType>(
                    handler,
                    proxyType,
                    objectType,
                    typeName,
                    args);

                typeMap.TryAdd(typeName, type);
            }

            switch (proxyType)
            {
                case ProxyType.Inheritance:
                    args = args
                        .Prepend(handler)
                        .ToArray();
                    break;
                case ProxyType.Interfaces:
                    args = args
                        .Prepend(Activator.CreateInstance(typeof(TImplementationType), args))
                        .Prepend(handler)
                        .ToArray();
                    break;
            }

            return (TBaseType)Activator.CreateInstance(type, args);
        }

        private Type BuildType<TImplementationType, TBaseType>(IProxyInvocationHandler handler, ProxyType proxyType, Type objectType, string typeName, object[] args)
            where TImplementationType : class, TBaseType
            where TBaseType : class
        {
            if (proxyType == ProxyType.Inheritance && (objectType.IsSealed || objectType.IsInterface))
            {
                throw new NotSupportedException($"{objectType.Name} is sealed/ or is a interface");
            }

            var constructorParamTypes = (args.Length > 0) ? args
                .Select(o => o.GetType())
                .ToArray() : new Type[0];

            if (objectType.GetConstructor(constructorParamTypes) is null)
            {
                throw new NotSupportedException($"{objectType.Name} has no matching constructor");
            }

            switch (proxyType)
            {
                case ProxyType.Inheritance:
                    return BuildInheritanceProxyType<TImplementationType>(
                        handler,
                        typeName,
                        constructorParamTypes);

                case ProxyType.Interfaces:
                    return BuildInterfacesProxyType<TImplementationType>(handler, typeName);

                default:
                    return null;
            }
        }

        private Type BuildInterfacesProxyType<TImplementationType>(
            IProxyInvocationHandler handler,
            string dynamicTypeName)
            where TImplementationType : class
        {
            var moduleBuilder = DefineDynamicModule();

            var handlerType = typeof(IProxyInvocationHandler);
            var objectReferenceType = typeof(TImplementationType);
            var baseType = typeof(object);
            var interfaces = objectReferenceType.GetInterfaces();

            var typeBuilder = moduleBuilder.DefineType(
                dynamicTypeName,
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed,
                baseType,
                interfaces);

            var handlerField = typeBuilder
                .DefineField(HandlerName, handlerType, FieldAttributes.Private);

            var objectReferenceFeild = typeBuilder
                .DefineField(ObjectReferenceName, objectReferenceType, FieldAttributes.Private);

            var superConstructor = baseType.GetConstructor(new Type[0]);

            var constructorParamterTypes = new Type[] 
            {
                handlerType,
                objectReferenceType
            };

            var delegateConstructor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                constructorParamterTypes);

            var constructorIL = delegateConstructor.GetILGenerator();

            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Call, superConstructor);

            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_1);
            constructorIL.Emit(OpCodes.Stfld, handlerField);


            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_2);
            constructorIL.Emit(OpCodes.Stfld, objectReferenceFeild);

            constructorIL.Emit(OpCodes.Ret);

            foreach (var interfaceType in interfaces)
            {
                GenerateInterfaceMethods<TImplementationType>(interfaceType, handlerField, objectReferenceFeild, typeBuilder);
            }

            return typeBuilder.CreateType();
        }

        private void GenerateInterfaceMethods<TImplementationType>(
            Type baseType,
            FieldBuilder handlerField,
            FieldBuilder objectReferenceFeild,
            TypeBuilder typeBuilder)
            where TImplementationType : class
        {
            var baseTypeMethods = baseType.GetMethods();

            for (var i = 0; i < baseTypeMethods.Length; i++)
            {
                var methodInfo = baseTypeMethods[i];

                var methodParameters = methodInfo.GetParameters()
                    .Select(mp => mp.ParameterType)
                    .ToArray();

                var numOfParams = methodParameters.Length;

                var methodBuilder = typeBuilder.DefineMethod(
                    methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    CallingConventions.Standard,
                    methodInfo.ReturnType, methodParameters);

                #region Handler Method IL Code
                var methodIL = methodBuilder.GetILGenerator();

                methodIL.Emit(OpCodes.Ldarg_0);
                methodIL.Emit(OpCodes.Ldfld, handlerField);

                methodIL.Emit(OpCodes.Ldstr, methodInfo.Name);

                methodIL.Emit(OpCodes.Callvirt, typeof(IProxyInvocationHandler)
                    .GetMethod(nameof(IProxyInvocationHandler.Invoked)));

                methodIL.Emit(OpCodes.Ldarg_0);
                methodIL.Emit(OpCodes.Ldfld, objectReferenceFeild);

                for (var j = 0; j < numOfParams; j++)
                {
                    methodIL.Emit(OpCodes.Ldarg_S, j + 1);
                }

                //Not Callvirt since this is a call to base method
                methodIL.Emit(OpCodes.Callvirt, baseType.GetMethod(methodInfo.Name, methodParameters));

                methodIL.Emit(OpCodes.Ret);
                #endregion
            }

            var parentTypes = baseType.GetInterfaces();

            if (parentTypes.Any())
            {
                foreach (var parentType in parentTypes)
                {
                    GenerateInterfaceMethods<TImplementationType>(parentType, handlerField, objectReferenceFeild, typeBuilder);
                }
            }
        }

        private Type BuildInheritanceProxyType<TImplementationType>(
            IProxyInvocationHandler handler,
            string dynamicTypeName,
            Type[] constructorParamTypes)
            where TImplementationType : class
        {
            var moduleBuilder = DefineDynamicModule();

            var handlerType = typeof(IProxyInvocationHandler);
            var baseType = typeof(TImplementationType);

            var typeBuilder = moduleBuilder.DefineType(
                dynamicTypeName,
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed,
                baseType);

            var handlerField = typeBuilder.DefineField(HandlerName, handlerType, FieldAttributes.Private);

            var superConstructor = baseType.GetConstructor(constructorParamTypes);

            var constructorParamterTypes = superConstructor
                .GetParameters()
                .Select(cp => cp.ParameterType)
                .Prepend(handlerType)
                .ToArray();

            var delegateConstructor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                constructorParamterTypes);

            var constructorIL = delegateConstructor.GetILGenerator();

            constructorIL.Emit(OpCodes.Ldarg_0);

            // Load constructor paramters with offset of 2
            // Ldarg_S 0 = this (ie Ldarg_0)
            // Ldarg_S 1 = IProxyInvocationHandler<T>
            // From Ldarg_S 2 constructor paramters for base class
            for (var j = 0; j < constructorParamterTypes.Length - 1; j++)
            {
                constructorIL.Emit(OpCodes.Ldarg_S, j + 2);
            }

            constructorIL.Emit(OpCodes.Call, superConstructor);

            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_1);
            constructorIL.Emit(OpCodes.Stfld, handlerField);
            constructorIL.Emit(OpCodes.Ret);

            GenerateInheritedMethods<TImplementationType>(baseType, handlerField, typeBuilder);

            return typeBuilder.CreateType();
        }

        private void GenerateInheritedMethods<TImplementationType>(
            Type baseType,
            FieldBuilder handlerField,
            TypeBuilder typeBuilder)
            where TImplementationType : class
        {
            var baseTypeMethods = baseType.GetMethods();

            for (var i = 0; i < baseTypeMethods.Length; i++)
            {
                var methodInfo = baseTypeMethods[i];
                        
                var methodParameters = methodInfo.GetParameters()
                    .Select(mp => mp.ParameterType)
                    .ToArray();

                var numOfParams = methodParameters.Length;

                var methodBuilder = typeBuilder.DefineMethod(
                    methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    CallingConventions.Standard,
                    methodInfo.ReturnType, methodParameters);

                #region Handler Method IL Code
                var methodIL = methodBuilder.GetILGenerator();

                methodIL.Emit(OpCodes.Ldarg_0);
                methodIL.Emit(OpCodes.Ldfld, handlerField);

                methodIL.Emit(OpCodes.Ldstr, methodInfo.Name);

                methodIL.Emit(OpCodes.Callvirt, typeof(IProxyInvocationHandler)
                    .GetMethod(nameof(IProxyInvocationHandler.Invoked)));

                methodIL.Emit(OpCodes.Ldarg_0);

                for (var j = 0; j < numOfParams; j++)
                {
                    methodIL.Emit(OpCodes.Ldarg_S, j + 1);
                }

                //Not Callvirt since this is a call to base method
                methodIL.Emit(OpCodes.Call, methodInfo);

                methodIL.Emit(OpCodes.Ret);
                #endregion

                if (methodInfo.IsVirtual && methodInfo.DeclaringType == baseType)
                {
                    typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
                }
            }
        }

        private static ModuleBuilder DefineDynamicModule()
        {
            var assemblyName = new AssemblyName
            {
                Name = AssemblyName,
                Version = new Version(1, 0, 0, 0)
            };

            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                assemblyName,
                AssemblyBuilderAccess.RunAndCollect);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule(ModuleName);
            return moduleBuilder;
        }
    }
}

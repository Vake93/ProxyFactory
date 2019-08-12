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

        private readonly IDictionary<string, Type> typeMap;

        public static ProxyFactory Instance { get; }

        static ProxyFactory()
        {
            Instance = new ProxyFactory();
        }

        private ProxyFactory()
        {
            typeMap = new ConcurrentDictionary<string, Type>();
        }

        public T Create<T>(IProxyInvocationHandler<T> handler, params object[] args)
            where T : class
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var objType = typeof(T);
            var typeName = $"{objType.FullName}{ProxySuffix}";

            var constructorParamTypes = (args.Length > 0) ? args
                .Select(o => o.GetType())
                .ToArray() : new Type[0];

            if (!typeMap.TryGetValue(typeName, out var type))
            {
                if (objType.IsSealed || objType.IsInterface)
                {
                    throw new NotSupportedException($"{objType.Name} is sealed/ or is a interface");
                }

                if (objType.GetConstructor(constructorParamTypes) is null)
                {
                    throw new NotSupportedException($"{objType.Name} has no matching constructor");
                }

                type = CreateType(handler, typeName, constructorParamTypes);
                typeMap.TryAdd(typeName, type);
            }

            args = args
                .Prepend(handler)
                .ToArray();

            return (T)Activator.CreateInstance(type, args);
        }

        private Type CreateType<T>(IProxyInvocationHandler<T> handler, string dynamicTypeName, Type[] constructorParamTypes)
            where T : class
        {
            var baseType = typeof(T);
            var handlerType = typeof(IProxyInvocationHandler<T>);

            var assemblyName = new AssemblyName
            {
                Name = AssemblyName,
                Version = new Version(1, 0, 0, 0)
            };

            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                assemblyName,
                AssemblyBuilderAccess.RunAndCollect);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule(ModuleName);

            var typeAttributes =
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;

            var typeBuilder = moduleBuilder.DefineType(
                dynamicTypeName, typeAttributes, baseType);

            var handlerField = typeBuilder.DefineField(
                HandlerName, handlerType, FieldAttributes.Private);

            var superConstructor = baseType.GetConstructor(constructorParamTypes);

            var constructorParamterTypes = superConstructor
                .GetParameters()
                .Select(cp => cp.ParameterType)
                .Prepend(handlerType)
                .ToArray();

            var delegateConstructor = typeBuilder.DefineConstructor(
                MethodAttributes.Public, CallingConventions.Standard, constructorParamterTypes);

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

            GenerateMethods<T>(baseType, handlerField, typeBuilder);

            return typeBuilder.CreateType();
        }

        private void GenerateMethods<T>(Type baseType, FieldBuilder handlerField, TypeBuilder typeBuilder)
            where T : class
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

                methodIL.Emit(
                    OpCodes.Callvirt,
                    typeof(IProxyInvocationHandler<T>).GetMethod(nameof(IProxyInvocationHandler<T>.Invoked)));

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
    }
}

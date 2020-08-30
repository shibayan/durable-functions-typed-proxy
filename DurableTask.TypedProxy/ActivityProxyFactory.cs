using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableTask.TypedProxy
{
    internal static class ActivityProxyFactory
    {
        private static readonly ModuleBuilder DynamicModuleBuilder;
        private static readonly ConcurrentDictionary<Type, Type> TypeMappings = new ConcurrentDictionary<Type, Type>();

        static ActivityProxyFactory()
        {
            var assemblyName = new AssemblyName($"DynamicAssembly_{Guid.NewGuid():N}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            DynamicModuleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
        }

        internal static TActivityInterface Create<TActivityInterface>(IDurableOrchestrationContext context)
        {
            var type = TypeMappings.GetOrAdd(typeof(TActivityInterface), CreateProxyType);

            return (TActivityInterface)Activator.CreateInstance(type, context);
        }

        private static Type CreateProxyType(Type interfaceType)
        {
            ValidateInterface(interfaceType);

            var baseType = typeof(ActivityProxy<>).MakeGenericType(interfaceType);

            var typeName = $"{interfaceType.Name}_{Guid.NewGuid():N}";

            var typeBuilder = DynamicModuleBuilder.DefineType(
                typeName,
                TypeAttributes.Public | TypeAttributes.BeforeFieldInit | TypeAttributes.AnsiClass,
                baseType);

            typeBuilder.AddInterfaceImplementation(interfaceType);

            BuildConstructor(typeBuilder, baseType);
            BuildMethods(typeBuilder, interfaceType, baseType);

            return typeBuilder.CreateTypeInfo();
        }

        private static void ValidateInterface(Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new InvalidOperationException($"{interfaceType.Name} is not an interface.");
            }

            if (interfaceType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Length > 0)
            {
                throw new InvalidOperationException($"Interface '{interfaceType.FullName}' can not define properties.");
            }

            if (interfaceType.GetMethods(BindingFlags.Instance | BindingFlags.Public).Length == 0)
            {
                throw new InvalidOperationException($"Interface '{interfaceType.FullName}' has no defined method.");
            }
        }

        private static void BuildConstructor(TypeBuilder typeBuilder, Type baseType)
        {
            var ctorArgTypes = new[] { typeof(IDurableOrchestrationContext) };

            // Create ctor
            var ctor = typeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                ctorArgTypes);

            var ilGenerator = ctor.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, baseType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, ctorArgTypes, null));
            ilGenerator.Emit(OpCodes.Ret);
        }

        private static void BuildMethods(TypeBuilder typeBuilder, Type interfaceType, Type baseType)
        {
            var methods = interfaceType.GetMethods(BindingFlags.Instance | BindingFlags.Public);

            var activityProxyMethods = baseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

            var callAsyncMethod = activityProxyMethods.First(x => x.Name == nameof(ActivityProxy<object>.CallAsync) && !x.IsGenericMethod);
            var callAsyncGenericMethod = activityProxyMethods.First(x => x.Name == nameof(ActivityProxy<object>.CallAsync) && x.IsGenericMethod);

            var functionNames = LookupFunctionNames(interfaceType);

            foreach (var methodInfo in methods)
            {
                var functionName = functionNames[methodInfo.Name];

                // Check that `FunctionNameAttribute` exists
                if (string.IsNullOrEmpty(functionName))
                {
                    throw new InvalidOperationException("FunctionName is not set.");
                }

                var parameters = methodInfo.GetParameters();

                // check that the number of arguments is one
                if (parameters.Length != 1)
                {
                    throw new InvalidOperationException($"Method '{methodInfo.Name}' is only a single argument can be used for operation input.");
                }

                var returnType = methodInfo.ReturnType;

                // check that return type is Task or Task<T>.
                if (!(returnType == typeof(Task) || returnType.BaseType == typeof(Task)))
                {
                    throw new InvalidOperationException($"Method '{methodInfo.Name}' is only a return type is Task or Task<T>.");
                }

                var proxyMethod = typeBuilder.DefineMethod(
                    methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName | MethodAttributes.Virtual,
                    returnType,
                    new[] { parameters[0].ParameterType });

                typeBuilder.DefineMethodOverride(proxyMethod, methodInfo);

                var ilGenerator = proxyMethod.GetILGenerator();

                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldstr, functionName);

                ilGenerator.Emit(OpCodes.Ldarg_1);

                // ValueType needs boxing.
                if (parameters[0].ParameterType.IsValueType)
                {
                    ilGenerator.Emit(OpCodes.Box, parameters[0].ParameterType);
                }

                ilGenerator.DeclareLocal(returnType);

                ilGenerator.Emit(OpCodes.Call, returnType.IsGenericType ? callAsyncGenericMethod.MakeGenericMethod(returnType.GetGenericArguments()[0]) : callAsyncMethod);

                ilGenerator.Emit(OpCodes.Stloc_0);
                ilGenerator.Emit(OpCodes.Ldloc_0);

                ilGenerator.Emit(OpCodes.Ret);
            }
        }

        private static Dictionary<string, string> LookupFunctionNames(Type interfaceType)
        {
            var implementedTypes = interfaceType.Assembly
                                                .GetTypes()
                                                .Where(x => x.IsClass && !x.IsAbstract && interfaceType.IsAssignableFrom(x))
                                                .ToArray();

            if (!implementedTypes.Any())
            {
                throw new InvalidOperationException($"Cannot find class that implements {interfaceType.FullName}.");
            }

            if (implementedTypes.Length > 1)
            {
                throw new InvalidOperationException("Ambiguous derived class with implemented {interfaceType.FullName}.");
            }

            return implementedTypes[0].GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                      .ToDictionary(x => x.Name, x => x.GetCustomAttribute<FunctionNameAttribute>()?.Name);
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;

namespace FunctionApp67
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

        internal static TActivityInterface Create<TActivityInterface>(DurableOrchestrationContext context)
        {
            var type = TypeMappings.GetOrAdd(typeof(TActivityInterface), CreateProxyType);

            return (TActivityInterface)Activator.CreateInstance(type, context);
        }

        private static Type CreateProxyType(Type interfaceType)
        {
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

        private static void BuildConstructor(TypeBuilder typeBuilder, Type baseType)
        {
            var ctorArgTypes = new[] { typeof(DurableOrchestrationContext) };

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

            foreach (var methodInfo in methods)
            {
                var parameters = methodInfo.GetParameters();

                // check that the number of arguments is zero or one
                if (parameters.Length > 1)
                {
                    throw new InvalidOperationException("Only a single argument can be used for operation input.");
                }

                var returnType = methodInfo.ReturnType;

                // check that return type is void / Task / Task<T>.
                if (returnType != typeof(void) && !(returnType == typeof(Task) || returnType.BaseType == typeof(Task)))
                {
                    throw new InvalidOperationException("Only a return type is void / Task / Task<T>.");
                }

                var proxyMethod = typeBuilder.DefineMethod(
                    methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName | MethodAttributes.Virtual,
                    returnType,
                    parameters.Length == 0 ? null : new[] { parameters[0].ParameterType });

                typeBuilder.DefineMethodOverride(proxyMethod, methodInfo);

                var ilGenerator = proxyMethod.GetILGenerator();

                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldstr, methodInfo.Name);

                if (parameters.Length == 0)
                {
                    ilGenerator.Emit(OpCodes.Ldnull);
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Ldarg_1);

                    // ValueType needs boxing.
                    if (parameters[0].ParameterType.IsValueType)
                    {
                        ilGenerator.Emit(OpCodes.Box, parameters[0].ParameterType);
                    }
                }

                ilGenerator.DeclareLocal(returnType);

                ilGenerator.Emit(OpCodes.Call, returnType.IsGenericType ? callAsyncGenericMethod.MakeGenericMethod(returnType.GetGenericArguments()[0]) : callAsyncMethod);

                ilGenerator.Emit(OpCodes.Stloc_0);
                ilGenerator.Emit(OpCodes.Ldloc_0);

                ilGenerator.Emit(OpCodes.Ret);
            }
        }
    }
}
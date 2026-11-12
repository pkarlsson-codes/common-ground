using System.Reflection;
using CommonGround.API.Infrastructure.ExceptionHandling;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.API.Tests.ExceptionHandling;

public class BusinessRuleErrorMessageMapTests
{
    public class TheByCodeMap
    {
        [Fact]
        public void GivenEveryBusinessRuleViolationFactory_WhenItsCodeIsLookedUp_ThenTheMapHasAMessageForIt()
        {
            var codes = ViolationFactories().Select(CodeOf).ToList();

            codes.Should().NotBeEmpty();
            string.Join(", ", codes.Where(code => !BusinessRuleErrorMessageMap.ByCode.ContainsKey(code))).Should().BeEmpty(
                "every code needs a message in BusinessRuleErrorMessageMap, or staff see a generic error");
        }

        /// <summary>The static methods that build a BusinessRuleViolationException, in every CommonGround assembly the API loads.</summary>
        private static IEnumerable<MethodInfo> ViolationFactories() =>
            CommonGroundAssemblies()
                .SelectMany(a => a.GetTypes())
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(m => m.ReturnType == typeof(BusinessRuleViolationException));

        private static IEnumerable<Assembly> CommonGroundAssemblies()
        {
            var seen = new Dictionary<string, Assembly>(StringComparer.Ordinal);
            var pending = new Stack<Assembly>([typeof(BusinessRuleErrorMessageMap).Assembly]);
            while (pending.Count > 0)
            {
                var assembly = pending.Pop();
                if (!seen.TryAdd(assembly.GetName().Name!, assembly))
                    continue;

                foreach (var reference in assembly.GetReferencedAssemblies().Where(r => r.Name!.StartsWith("CommonGround.", StringComparison.Ordinal)))
                    pending.Push(Assembly.Load(reference));
            }

            return seen.Values;
        }

        /// <summary>Calls the factory with sample arguments; the code doesn't depend on them.</summary>
        private static string CodeOf(MethodInfo factory)
        {
            var arguments = factory.GetParameters().Select(p => SampleOf(p.ParameterType)).ToArray();

            return ((BusinessRuleViolationException)factory.Invoke(null, arguments)!).Code;
        }

        /// <summary>A default for value types and strings; for a smart enum like ApplicationPhase, its first static instance.</summary>
        private static object? SampleOf(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            if (type == typeof(string))
                return string.Empty;

            return type.GetFields(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(f => f.FieldType == type)?.GetValue(null)
                ?? type.GetProperties(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(p => p.PropertyType == type)?.GetValue(null);
        }
    }
}

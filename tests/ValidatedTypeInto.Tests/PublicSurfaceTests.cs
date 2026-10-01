using System.Activities;
using System.Activities.Expressions;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using Vodafone.Robotics.ValidatedTypeInto.Activities;
using ActivityUnderTest = Vodafone.Robotics.ValidatedTypeInto.Activities.ValidatedTypeInto;

namespace ValidatedTypeInto.Tests;

public sealed class PublicSurfaceTests
{
    private static readonly string[] ExpectedArguments =
    {
        "in_intTimeoutL", "in_intTimeoutS", "in_intNumberOfRetries", "in_tspRetryInterval",
        "in_strValueToBeWritten", "in_strPreConditionSelector", "in_strPreConditionSuccessMsg",
        "in_strPreConditionFailedMsg", "in_strTargetSelector", "in_boolAlterIfDisabled",
        "in_boolClickBeforeTyping", "in_boolEmptyField", "in_boolSimulateType",
        "in_strPostConditionSelector", "in_strPostConditionSuccessMsg", "in_strPostConditionFailedMsg"
    };

    [Fact]
    public void Assembly_exposes_exactly_one_concrete_activity()
    {
        var activities = typeof(ActivityUnderTest).Assembly.ExportedTypes
            .Where(type => typeof(Activity).IsAssignableFrom(type) && !type.IsAbstract)
            .ToArray();
        Assert.Equal(new[] { typeof(ActivityUnderTest) }, activities);
    }

    [Fact]
    public void Activity_has_exact_argument_contract_and_required_markers()
    {
        var properties = typeof(ActivityUnderTest).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(InArgument<>))
            .ToArray();

        Assert.Equal(ExpectedArguments.OrderBy(name => name), properties.Select(property => property.Name).OrderBy(name => name));
        foreach (var required in new[] { "in_strValueToBeWritten", "in_strPreConditionSelector", "in_strTargetSelector", "in_strPostConditionSelector" })
            Assert.NotNull(properties.Single(property => property.Name == required).GetCustomAttribute<RequiredArgumentAttribute>());
        Assert.All(properties, property => Assert.NotNull(property.GetCustomAttribute<CategoryAttribute>()));
    }

    [Fact]
    public void Activity_has_the_planned_practical_defaults()
    {
        var activity = new ActivityUnderTest();

        Assert.Equal(30_000, ((Literal<int>)activity.in_intTimeoutL.Expression).Value);
        Assert.Equal(10_000, ((Literal<int>)activity.in_intTimeoutS.Expression).Value);
        Assert.Equal(2, ((Literal<int>)activity.in_intNumberOfRetries.Expression).Value);
        Assert.Equal(TimeSpan.FromSeconds(1), ((Literal<TimeSpan>)activity.in_tspRetryInterval.Expression).Value);
        Assert.True(((Literal<bool>)activity.in_boolAlterIfDisabled.Expression).Value);
        Assert.True(((Literal<bool>)activity.in_boolClickBeforeTyping.Expression).Value);
        Assert.True(((Literal<bool>)activity.in_boolEmptyField.Expression).Value);
        Assert.True(((Literal<bool>)activity.in_boolSimulateType.Expression).Value);
    }

    [Fact]
    public void Metadata_registers_only_the_activity_and_viewmodel()
    {
        var assembly = typeof(ActivityUnderTest).Assembly;
        using var stream = assembly.GetManifestResourceStream("Vodafone.Robotics.ValidatedTypeInto.Resources.ActivitiesMetadata.json");
        Assert.NotNull(stream);
        using var json = JsonDocument.Parse(stream!);
        var entries = json.RootElement.GetProperty("activities").EnumerateArray().ToArray();
        Assert.Single(entries);
        Assert.Equal(typeof(ActivityUnderTest).FullName, entries[0].GetProperty("fullName").GetString());
        Assert.NotNull(assembly.GetType(entries[0].GetProperty("viewModelType").GetString()!));
    }
}

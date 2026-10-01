using System.Activities;
using Vodafone.Robotics.ValidatedTypeInto.Activities;

namespace ConsumerSmoke;

public static class ActivityConsumer
{
    public static Activity Create(string targetSelector, string value) =>
        new ValidatedTypeInto
        {
            in_strTargetSelector = targetSelector,
            in_strValueToBeWritten = value
        };
}

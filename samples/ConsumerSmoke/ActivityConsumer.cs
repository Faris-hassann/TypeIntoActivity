using System.Activities;
using Vodafone.Robotics.ValidatedTypeInto.Activities;

namespace ConsumerSmoke;

public static class ActivityConsumer
{
    public static Activity Create(string preSelector, string targetSelector, string postSelector, string value) =>
        new ValidatedTypeInto
        {
            in_strPreConditionSelector = preSelector,
            in_strTargetSelector = targetSelector,
            in_strPostConditionSelector = postSelector,
            in_strValueToBeWritten = value
        };
}

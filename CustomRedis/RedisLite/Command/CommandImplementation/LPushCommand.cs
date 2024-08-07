﻿using RedisLite.Command.Utility;
using RedisLite.Persistance;

namespace RedisLite.Command.CommandImplementation;

internal class LPushCommand : Command
{
    public LPushCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 2;

    public override string CommandName => "lpush";

    public override Task<object?> ExecuteAsync()
    {
        try
        {
            var key = Arguments[0];
            var numberOfArguments = Arguments.Count - 1;
            var argumentsToAddToList = Arguments.GetRange(1, numberOfArguments);
            argumentsToAddToList.Reverse();

            var value = PersistanceStore.GetValue(key) as PersistanceObject;
            if (value == null)
            {
                PersistanceStore.SetKey(key, new PersistanceObject(argumentsToAddToList));
                return TaskFromResultMapper.MapFromResult(numberOfArguments);
            }

            var list = (List<string>)value.PersistedData;
            list.InsertRange(0, argumentsToAddToList);
            var result = PersistanceStore.SetKey(key, new PersistanceObject(list));

            return TaskFromResultMapper.MapFromResult(list.Count);
        }
        catch (Exception)
        {
            return TaskFromResultMapper.MapFromResult(new Exception("The value is not a list."));
        }
    }
}

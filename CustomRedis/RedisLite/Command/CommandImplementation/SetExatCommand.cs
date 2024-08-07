﻿using RedisLite.Command.Utility;
using RedisLite.Persistance;

namespace RedisLite.Command.CommandImplementation;

internal class SetExatCommand : Command
{
    public SetExatCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 4;
    public override string CommandName => "set eaxt";
    public override Task<object?> ExecuteAsync()
    {
        try
        {
            var seconds = long.Parse(Arguments[Arguments.Count - 1]);
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(seconds);
            var persistanceData = new PersistanceObject(Arguments[1], dateTimeOffset);
            PersistanceStore.SetKey(Arguments[0], persistanceData);
            return TaskFromResultMapper.MapFromResult("OK");
        }
        catch (Exception ex)
        {
            return TaskFromResultMapper.MapFromResult(ex);
        }
    }
}

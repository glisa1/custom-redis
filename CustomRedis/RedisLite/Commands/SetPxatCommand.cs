﻿using RedisLite.Persistance;

namespace RedisLite.Commands;

internal class SetPxatCommand : Command
{
    public SetPxatCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 4;
    public override string CommandName => "set pxat";
    public override object Execute()
    {
        var milliseconds = long.Parse(Arguments[Arguments.Count - 1]);
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
        var persistanceData = new PersistanceObject(Arguments[1], dateTimeOffset);
        return PersistanceStore.SetKey(Arguments[0], persistanceData);
    }
}

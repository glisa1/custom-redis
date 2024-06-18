﻿using RedisLite.Persistance;

namespace RedisLite.Commands;

internal class SetExatCommand : Command
{
    public SetExatCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 4;
    public override string CommandName => "set eaxt";
    public override object Execute()
    {
        var seconds = long.Parse(Arguments[Arguments.Count - 1]);
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(seconds);
        var persistanceData = new PersistanceObject(Arguments[1], dateTimeOffset);
        return PersistanceStore.SetKey(Arguments[0], persistanceData);
    }
}

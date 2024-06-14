﻿using RedisLite.Persistance;

namespace RedisLite.Commands;

internal class SetExCommand : Command
{
    public SetExCommand(List<string> args)
        : base(args)
    {   
    }

    public override int NumberOfExpectedArguments => 4;
    public override string CommandName => "set ex";
    public override object Execute()
    {
        var persistanceData = new PersistanceObject(Arguments[1], seconds: int.Parse(Arguments[Arguments.Count - 1]));
        return PersistanceStore.SetKey(Arguments[0], persistanceData);
    }
}

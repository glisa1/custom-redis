﻿using RedisLite.Persistance;

namespace RedisLite.Commands;

internal class SetPxCommand : Command
{
    public SetPxCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 4;
    public override string CommandName => "set px";
    public override Task<object> ExecuteAsync()
    {
        try
        {
            var persistanceData = new PersistanceObject(Arguments[1], miliseconds: int.Parse(Arguments[Arguments.Count - 1]));
            PersistanceStore.SetKey(Arguments[0], persistanceData);
            return Task.FromResult((object)"OK");
        }
        catch (Exception ex)
        {
            return Task.FromResult((object)ex);
        }
    }
}

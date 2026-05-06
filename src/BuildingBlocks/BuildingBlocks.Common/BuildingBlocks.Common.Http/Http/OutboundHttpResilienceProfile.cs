namespace BuildingBlocks.Common.Http;

public enum OutboundHttpResilienceProfile
{
    Command = 0,
    CommandNoRetry = 1,
    ReadOnly = 2
}


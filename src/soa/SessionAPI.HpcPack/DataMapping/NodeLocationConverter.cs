namespace Microsoft.Hpc.Scheduler.Session.HpcPack.DataMapping
{
    public static class NodeLocationConverter
    {
        public static Microsoft.Hpc.Scheduler.Session.Data.NodeLocation FromHpcNodeLocation(Microsoft.Hpc.Scheduler.Properties.NodeLocation location)
        {
            return (Data.NodeLocation)location;
        }
    }
}

namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO
{
    public static class CloudQueueCmdExtension
    {
        public static ParameterUnpacker GetUnpacker(this CloudQueueCmdDto dto)
        {
            return new ParameterUnpacker(dto.Parameters);
        }
    }
}

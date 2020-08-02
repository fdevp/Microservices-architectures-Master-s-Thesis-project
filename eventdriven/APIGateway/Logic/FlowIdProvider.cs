namespace APIGateway
{
    public static class FlowIdProvider
    {
        private static long _latestFlowId = 1;

        public static string Create() => (_latestFlowId++).ToString();
    }
}
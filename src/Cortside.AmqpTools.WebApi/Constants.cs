namespace Cortside.AmqpTools.WebApi {
    /// <summary>
    /// Constanst for webapi
    /// </summary>
    public static class Constants {
        /// <summary>
        /// Authorization constants
        /// </summary>
        public static class Authorization {
            /// <summary>
            /// Permission constants
            /// </summary>
            public static class Permissions {
                /// <summary>
                /// Get queues permission
                /// </summary>
                public const string GetQueues = "GetQueues";

                /// <summary>
                /// Shovel queues permission
                /// </summary>
                public const string ShovelQueues = "ShovelQueues";

                /// <summary>
                /// Delete a message
                /// </summary>
                public const string DeleteMessage = "DeleteMessage";
            }
        }
    }
}

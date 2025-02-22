﻿using System.Collections.Generic;

namespace Cortside.AmqpTools.WebApi.Models.Responses {
    /// <summary>
    /// Authorization model
    /// </summary>
    public class AuthorizationModel {
        /// <summary>
        /// Permissions
        /// </summary>
        public List<string> Permissions { get; set; }
    }
}

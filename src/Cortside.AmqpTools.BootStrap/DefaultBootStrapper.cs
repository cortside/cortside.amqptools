using System.Collections.Generic;
using Cortside.Common.BootStrap;
using Cortside.AmqpTools.BootStrap.Installers;

namespace Cortside.AmqpTools.BootStrap {
    public class DefaultApplicationBootStrapper : BootStrapper {
        public DefaultApplicationBootStrapper() {
            installers = new List<IInstaller> {
                new DomainServiceInstaller()
            };
        }
    }
}

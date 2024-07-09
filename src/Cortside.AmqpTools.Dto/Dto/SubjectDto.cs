using System;

namespace Cortside.AmqpTools.Dto.Dto {

    public class SubjectDto {
        public Guid SubjectId { get; set; }

        public string Name { get; set; }

        public string GivenName { get; set; }

        public string FamilyName { get; set; }

        public string UserPrincipalName { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}

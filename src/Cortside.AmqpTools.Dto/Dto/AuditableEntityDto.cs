using System;

namespace Cortside.AmqpTools.Dto.Dto {
    public class AuditableEntityDto {
        public DateTime CreatedDate { get; set; }

        public SubjectDto CreatedSubject { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public SubjectDto LastModifiedSubject { get; set; }
    }
}

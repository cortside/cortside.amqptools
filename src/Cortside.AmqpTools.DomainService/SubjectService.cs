using System.Threading.Tasks;
using Cortside.AmqpTools.Dto.Dto;

namespace Cortside.AmqpTools.DomainService {
    public class SubjectService : ISubjectService {

        public SubjectService() {
        }
        public  Task SaveAsync(SubjectDto subject) {
            return Task.CompletedTask;
        }
    }
}

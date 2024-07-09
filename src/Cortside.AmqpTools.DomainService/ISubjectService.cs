using System.Threading.Tasks;
using Cortside.AmqpTools.Dto.Dto;

namespace Cortside.AmqpTools.DomainService {
    public interface ISubjectService {
        Task SaveAsync(SubjectDto subject);
    }
}

using Application.Shared.Results;
using Application.TaskSystem.Model;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;

namespace Application.TaskSystem.Commands.CreateTaskAssignmentSubmissionDetails
{

    // Komut: Toplu güncelleme komutu
    public class CreateTaskAssignmentSubmissionDetailsCommand : IRequest<ResultSingle<TaskSubmissionDto>>
    {
        public int AppUserId { get; set; } // Güncellemeyi yapan kullanıcı ID'si
        public int AssignmentId { get; set; } // Görev ataması ID'si
        public List<TaskSubmissionUpdateDto> Updates { get; set; } // Güncellemeler listesi
    }

    // Handler: Komutu işleyen sınıf
    public class CreateTaskAssignmentSubmissionDetailsCommandHandler : IRequestHandler<CreateTaskAssignmentSubmissionDetailsCommand, ResultSingle<TaskSubmissionDto>>
    {
        private readonly IRepository<TaskSubmission> _taskSubmissionRepository;
        private readonly IRepository<TaskAssignment> _taskAssignmentRepository;
        private readonly IMapper _mapper;

        public CreateTaskAssignmentSubmissionDetailsCommandHandler(IRepository<TaskSubmission> taskSubmissionRepository, IRepository<TaskAssignment> taskAssignmentRepository, IMapper mapper)
        {
            _taskSubmissionRepository = taskSubmissionRepository;
            _taskAssignmentRepository = taskAssignmentRepository;
            _mapper = mapper;
        }

        public async Task<ResultSingle<TaskSubmissionDto>> Handle(CreateTaskAssignmentSubmissionDetailsCommand request, CancellationToken cancellationToken)
        {
            // Güncelleme verilerinin varlığını kontrol et
            if (request.Updates == null || request.Updates.Count == 0)
            {
                return new ResultSingle<TaskSubmissionDto>(null, false, $"Güncellenecek veriler sağlanmamış.");
            }


            var assignmentUser = _taskAssignmentRepository.Get(ts => ts.Id == request.AssignmentId && ts.CreatedBy == request.AppUserId);

            if (assignmentUser == null)
            {
                return new ResultSingle<TaskSubmissionDto>(null, false, $"Yetkisiz kullanıcı.!");
            }

            try
            {
                foreach (var update in request.Updates)
                {
                    // Mevcut bir TaskSubmission var mı kontrol et
                    var existingSubmission = _taskSubmissionRepository.Get(
                        ts => ts.TaskAssignmentId == request.AssignmentId && ts.CreatedBy == update.UserId
                    );

                    if (existingSubmission != null)
                    {
                        // Mevcut ise güncelle
                        existingSubmission.Grade = update.Grade;
                        existingSubmission.Feedback = update.Feedback;
                        existingSubmission.UpdatedBy = request.AppUserId;
                        existingSubmission.UpdatedTime = DateTime.UtcNow;
                        _taskSubmissionRepository.Update(existingSubmission);
                    }
                    else
                    {
                        // Mevcut değilse yeni oluştur
                        var newSubmission = new TaskSubmission
                        {
                            TaskAssignmentId = request.AssignmentId,
                            CreatedBy = update.UserId,
                            Grade = update.Grade,
                            Feedback = update.Feedback,
                            CreatedTime = DateTime.UtcNow,
                            UpdatedTime = DateTime.UtcNow,
                            UpdatedBy = request.AppUserId
                        };
                        _taskSubmissionRepository.InsertWithoutCommit(newSubmission);
                    }
                }

                // Tüm değişiklikleri commit et
                await _taskSubmissionRepository.CommitAsync(cancellationToken);

                return new ResultSingle<TaskSubmissionDto>(null, true, "Görev atamaları başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return new ResultSingle<TaskSubmissionDto>(null, false, $"Güncelleme sırasında bir hata oluştu: {ex.Message}");
            }
        }
    }
}

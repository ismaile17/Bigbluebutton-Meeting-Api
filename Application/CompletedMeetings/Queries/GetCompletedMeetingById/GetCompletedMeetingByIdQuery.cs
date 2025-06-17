using Application.CompletedMeetings.Model;
using Application.Shared.Results;
using AutoMapper;
using Infrastructure.Persistence;
using MediatR;

namespace Application.CompletedMeetings.Queries.GetCompletedMeetingById
{
    public class GetCompletedMeetingByIdQuery:IRequest<ResultSingle<CompletedMeetingDto>>
    {
        public int CompletedMeetingId { get; set; }
        public int AppUserId { get; set; }
    }

    public class GetCompletedMeetingByIdQueryHandler : IRequestHandler<GetCompletedMeetingByIdQuery, ResultSingle<CompletedMeetingDto>>
    {
        private readonly IRepository<Domain.Entities.CompletedMeeting> _completedMeetingRepository;
        private readonly IMapper _mapper;

        public GetCompletedMeetingByIdQueryHandler(IRepository<Domain.Entities.CompletedMeeting> completedMeetingRepository, IMapper mapper)
        {
            _completedMeetingRepository = completedMeetingRepository;
            _mapper = mapper;
        }

        public async Task<ResultSingle<CompletedMeetingDto>> Handle(GetCompletedMeetingByIdQuery request, CancellationToken cancellationToken)
        {
            var data = _completedMeetingRepository.GetById(request.CompletedMeetingId);

            if(data == null)
            {
                return Result.Fail<CompletedMeetingDto>(null, $"ID'si {request.CompletedMeetingId} olan toplantı bulunamadı.");
            }
            else
            {
                var completedMeetingDto = _mapper.Map<CompletedMeetingDto>(data);

                if (completedMeetingDto.PublicOrPrivate == "0")
                {
                    completedMeetingDto.GuestLink = null;
                }

                return Result.Ok(completedMeetingDto, $"Başarılı");
            }
        }
    }
}

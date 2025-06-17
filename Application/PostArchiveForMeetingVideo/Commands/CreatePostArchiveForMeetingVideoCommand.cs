using Application.PostArchiveForMeetingVideo.Model;
using Application.Shared.Results;
using AutoMapper;
using Infrastructure.Persistence;
using MediatR;

namespace Application.PostArchiveForMeetingVideo.Commands
{
    public class CreatePostArchiveForMeetingVideoCommand : IRequest<ResultSingle<PostArchiveForMeetingVideoDto>>
    {
        public string Name { get; set; } //password
        public string MeetingId { get; set; }
    }

    public class CreatePostArchiveForMeetingVideoCommandHandler : IRequestHandler<CreatePostArchiveForMeetingVideoCommand, ResultSingle<PostArchiveForMeetingVideoDto>>
    {
        private readonly IRepository<Domain.Entities.PostArchiveForMeetingVideo> _postArchiveRepository;
        private readonly IMapper _mapper;
        private readonly TelegramService _telegramService;

        public CreatePostArchiveForMeetingVideoCommandHandler(IRepository<Domain.Entities.PostArchiveForMeetingVideo> postArchiveRepository, IMapper mapper, TelegramService telegramService)
        {
            _postArchiveRepository = postArchiveRepository;
            _mapper = mapper;
            _telegramService = telegramService;
        }

        public async Task<ResultSingle<PostArchiveForMeetingVideoDto>> Handle(CreatePostArchiveForMeetingVideoCommand request, CancellationToken cancellationToken)
        {
            string password = "36fe8f0d-87c4-4667-a2f0-3503e2eba7b3";

            if (request.Name != password)
            {
                return Result.Fail<PostArchiveForMeetingVideoDto>(null, $"Kayıt edilemedi");
            }
            var postArchive = new Domain.Entities.PostArchiveForMeetingVideo
            {
                MeetingId = request.MeetingId
            };

            _postArchiveRepository.InsertWithoutCommit(postArchive);
            var result = await _postArchiveRepository.CommitAsync(cancellationToken);
            if (result == -1)
            {
                return Result.Fail<PostArchiveForMeetingVideoDto>(null, $"Kayıt edilemedi");
            }

            _ = _telegramService.SendMessageAsync($"AWS UPDATE! ID: {postArchive.MeetingId}");


            var data = await Task.Run(() => _mapper.Map<Domain.Entities.PostArchiveForMeetingVideo, PostArchiveForMeetingVideoDto>(postArchive));
            return Result.Ok<PostArchiveForMeetingVideoDto>(data, $"OK");


        }
    }
}

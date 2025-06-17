using Application.LearningDashboards.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities.Learning;
using Infrastructure.Persistence;
using MediatR;
using Shared.Enum;

namespace Application.LearningDashboards.Commands.CreateLearningDashboard
{
    public class CreateLearningDashboardCommand : IRequest<ResultSingle<LearningDashboardDto>>
    {
        public string version { get; set; }
        public string meeting_id { get; set; }
        public string internal_meeting_id { get; set; }
        public BBBData data { get; set; }
    }

    public class CreateLearningDashboardCommandHandler : IRequestHandler<CreateLearningDashboardCommand, ResultSingle<LearningDashboardDto>>
    {
        private readonly IRepository<LearningMeeting> _learningMeetingRepository;
        private readonly IRepository<Domain.Entities.CompletedMeeting> _complatedMeeting;
        private readonly IMapper _mapper;
        private readonly TelegramService _telegramService;

        public CreateLearningDashboardCommandHandler(IRepository<LearningMeeting> learningMeetingRepository, IRepository<Domain.Entities.CompletedMeeting> complatedMeeting, IMapper mapper, TelegramService telegramService)
        {
            _learningMeetingRepository = learningMeetingRepository;
            _complatedMeeting = complatedMeeting;
            _mapper = mapper;
            _telegramService = telegramService;
        }

        public async Task<ResultSingle<LearningDashboardDto>> Handle(CreateLearningDashboardCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.meeting_id) || string.IsNullOrEmpty(request.internal_meeting_id) || request.data == null)
            {
                return Result.Fail<LearningDashboardDto>(null, "Geçersiz veri");
            }

            var completedMeeting = _complatedMeeting.GetMany(m => m.BBBMeetingId == request.meeting_id)
                .FirstOrDefault();

            if (completedMeeting == null)
            {
                _ = _telegramService.SendMessageAsync($"!!!!!LEARNING DASHBOARD oluşturulamadı: CompletedMeeting bulunamadı! Meeting ID: {request.meeting_id}");

                return Result.Fail<LearningDashboardDto>(null, "HATA!!!!.");
            }

            var meeting = new LearningMeeting
            {
                Version = request.version,
                MeetingId = request.meeting_id,
                InternalMeetingId = request.internal_meeting_id,
                IsBreakout = bool.Parse(request.data.metadata.is_breakout),
                MeetingName = request.data.metadata.meeting_name,
                Duration = request.data.duration,
                Start = request.data.start,
                Finish = request.data.finish,
                Attendees = request.data.attendees.Select(a => new Domain.Entities.Learning.Attendee
                {
                    ExtUserId = a.ext_user_id,
                    Name = a.name,
                    Moderator = a.moderator,
                    Duration = a.duration,
                    EngagementChats = a.engagement.chats,
                    EngagementTalks = a.engagement.talks,
                    EngagementRaisehand = a.engagement.raisehand,
                    EngagementEmojis = a.engagement.emojis,
                    EngagementPollVotes = a.engagement.poll_votes,
                    EngagementTalkTime = a.engagement.talk_time,
                    Sessions = a.sessions.Select(s => new Domain.Entities.Learning.AttendeeSession
                    {
                        JoinTime = s.joins.FirstOrDefault().timestamp,
                        LeaveTime = s.lefts.FirstOrDefault()?.timestamp ?? s.joins.FirstOrDefault().timestamp
                    }).ToList()
                }).ToList(),
                Files = request.data.files.Select(f => new Domain.Entities.Learning.LearningFile
                {
                    FileName = f
                }).ToList(),
                Polls = request.data.polls.Select(p => new Domain.Entities.Learning.Poll
                {
                    PollId = p.id,
                    Type = p.type,
                    Question = p.question,
                    Published = p.published,
                    Start = p.start,
                    PollVotes = p.votes?.Select(v => new Domain.Entities.Learning.PollVote
                    {
                        UserId = v.Key,
                        Vote = v.Value
                    }).ToList() ?? new List<Domain.Entities.Learning.PollVote>()
                }).ToList()
            };

            _learningMeetingRepository.InsertWithoutCommit(meeting);
            var result = await _learningMeetingRepository.CommitAsync(cancellationToken);

            if (result == -1)
            {
                return Result.Fail<LearningDashboardDto>(null, "Kayıt edilemedi");
            }

            var completedUpdate = _complatedMeeting.GetMany(m => m.BBBMeetingId == request.meeting_id)
                .FirstOrDefault();

            if (completedUpdate != null)
            {
                completedUpdate.CreateStartOrFinish = CompletedMeetingCreateStartOrFinish.Finish;
                _complatedMeeting.UpdateWithoutCommit(completedUpdate);
            }

            if (completedUpdate == null)
            {
                _ = _telegramService.SendMessageAsync($"LEARNİNG DASHBOARD DA completedUpdate ÇALIŞMADI!!: {request.meeting_id}");
            }

            var data = new LearningDashboardDto
            {
                Id = meeting.Id.ToString(),
                MeetingId = meeting.MeetingId,
                InternalMeetingId = meeting.InternalMeetingId,
                Version = meeting.Version,
                Data = null
            };

            _ = _telegramService.SendMessageAsync($"Yeni bir Learning Dashboard oluşturuldu! ID: {data.Id}");


            return Result.Ok<LearningDashboardDto>(data, "Learning Dashboard oluşturuldu");
        }
    }
}
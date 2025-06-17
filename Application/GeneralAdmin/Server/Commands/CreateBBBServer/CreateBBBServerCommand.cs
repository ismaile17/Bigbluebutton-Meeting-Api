using Application.GeneralAdmin.GAServer.Model;
using Application.Shared.Results;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;

namespace Application.GeneralAdmin.GAServer.Commands.CreateGAServer
{
    public class CreateBBBServerCommand:IRequest<ResultSingle<BBBServerDto>>
    {
        public int AppUserId { get; set; }
        public bool? MainServer { get; set; }=false;
        public string? ServerName { get; set; }
        public string? ServerDetail { get; set; }
        public string? Notes { get; set; }
        public string? BuyPrice { get; set; }
        public string? BuyCompany { get; set; }
        public string? ServerLocation { get; set; }
        public string? IpAdress { get; set; }
        public string? ServerApiUrl { get; set; }
        public string? SharedSecret { get; set; }
    }


    public class CreateBBBServerCommandHandler : IRequestHandler<CreateBBBServerCommand, ResultSingle<BBBServerDto>>
    {
        private readonly IRepository<BBBServer> _bbbServer;
        private readonly IMapper _mapper;

        public CreateBBBServerCommandHandler(IRepository<BBBServer> bbbServer, IMapper mapper)
        {
            _bbbServer = bbbServer;
            _mapper = mapper;
        }

        public async Task<ResultSingle<BBBServerDto>> Handle(CreateBBBServerCommand request, CancellationToken cancellationToken)
        {
            var server = new BBBServer
            {
                CreatedBy = request.AppUserId,
                MainServer = request.MainServer,
                ServerName = request.ServerName,
                ServerDetail = request.ServerDetail,
                Notes = request.Notes,
                BuyPrice = request.BuyPrice,
                BuyCompany = request.BuyCompany,
                ServerLocation = request.ServerLocation,
                IpAdress = request.IpAdress,
                ServerApiUrl = request.ServerApiUrl,
                SharedSecret = request.SharedSecret
            };

            _bbbServer.InsertWithoutCommit(server);
            var result = await _bbbServer.CommitAsync(cancellationToken);
            if(result == -1)
            {
                return Result.Fail<BBBServerDto>(null, $"Kayıt edilemedi");
            }

            if (request.AppUserId != 2)
            {
                return Result.Fail<BBBServerDto>(null, $"Admin değilsiniz.");
            }

            var data = await Task.Run(() => _mapper.Map<BBBServer, BBBServerDto>(server));
            return Result.Ok<BBBServerDto>(data, $"Server kayıt edildi.");
        }
    }
}

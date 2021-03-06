using AutoMapper;
using Jp.Application.Interfaces;
using Jp.Application.ViewModels;
using Jp.Domain.Core.Bus;
using Jp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jp.Domain.Commands.PersistedGrant;

namespace Jp.Application.Services
{
    public class PersistedGrantAppService : IPersistedGrantAppService
    {
        private IMapper _mapper;
        private IEventStoreRepository _eventStoreRepository;
        private readonly IPersistedGrantRepository _persistedGrantRepository;
        private readonly IUserService _userService;
        public IMediatorHandler Bus { get; set; }

        public PersistedGrantAppService(IMapper mapper,
            IMediatorHandler bus,
            IEventStoreRepository eventStoreRepository,
            IPersistedGrantRepository persistedGrantRepository,
            IUserService userService)
        {
            _mapper = mapper;
            Bus = bus;
            _eventStoreRepository = eventStoreRepository;
            _persistedGrantRepository = persistedGrantRepository;
            _userService = userService;
        }

        public async Task<IEnumerable<PersistedGrantViewModel>> GetPersistedGrants()
        {
            var resultado = await _persistedGrantRepository.GetGrants();
            var subjects = await _userService.GetByIdAsync(resultado.Select(s => s.SubjectId).ToArray());
            return resultado.Select(s => new PersistedGrantViewModel(s.Key, s.Type, s.SubjectId, s.ClientId, s.CreationTime, s.Expiration, s.Data, subjects.FirstOrDefault(f => f.Id.ToString().ToLower() == s.SubjectId.ToLower())?.Email, subjects.FirstOrDefault(f => f.Id.ToString().ToLower() == s.SubjectId.ToLower())?.Picture));
        }

        public Task Remove(RemovePersistedGrantViewModel model)
        {
            // kiss
            var command = _mapper.Map<RemovePersistedGrantCommand>(model);
            return Bus.SendCommand(command);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
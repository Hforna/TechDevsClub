using AutoMapper;
using Profile.Application.Requests;
using Profile.Application.Responses;
using Profile.Domain.Aggregates;
using Profile.Domain.Repositories;
using Profile.Domain.Services;
using Profile.Domain.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application.ApplicationServices
{
    public interface ISkillService
    {
        public Task<List<SkillResponse>> GetSkills();
        public Task RemoveUserSkills(RemoveUserSkillsRequest request);
    }


    public class SkillService : ISkillService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uof;
        private readonly ITokenService _tokenService;
        private readonly IRequestService _requestService;

        public SkillService(IMapper mapper, IUnitOfWork uof, 
            ITokenService tokenService, IRequestService requestService)
        {
            _mapper = mapper;
            _requestService = requestService;
            _uof = uof;
            _tokenService = tokenService;
        }

        public async Task<List<SkillResponse>> GetSkills()
        {
            var skills = await _uof.SkillRepository.GetAllSkills();

            if (skills is null)
                return [];

            return _mapper.Map<List<SkillResponse>>(skills);
        }

        public async Task RemoveUserSkills(RemoveUserSkillsRequest request)
        {
            var user = await _tokenService.GetUserByToken();

            user.RemoveSkillsByNames(request.Skills.Select(d => d.Name).ToList());

            _uof.GenericRepository.Update<User>(user);
            await _uof.Commit();
        }
    }
}

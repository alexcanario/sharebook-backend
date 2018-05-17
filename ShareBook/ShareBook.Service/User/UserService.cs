﻿using System;
using System.Threading.Tasks;
using AutoMapper;
using ShareBook.Data;
using ShareBook.Data.Common;
using ShareBook.Data.Entities.User.Out;
using ShareBook.Data.Model;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.VM.Common;
using ShareBook.VM.User.In;
using ShareBook.VM.User.Out;

namespace ShareBook.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;


        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResultServiceVM> CreateUser(UserInVM userInVM)
        {
            User user = Mapper.Map<User>(userInVM);

            ResultService resultService = new ResultService(new UserValidation().Validate(user));

            _unitOfWork.BeginTransaction();

            if (resultService.Success)
            {
                await _userRepository.InsertAsync(user);
                _unitOfWork.Commit();
            }

            return Mapper.Map<ResultServiceVM>(resultService);
        }

        public async Task<UserOutByIdVM> GetUserByEmailAndPasswordAsync(UserInVM userInVM)
        {
            User user = Mapper.Map<User>(userInVM);

            ResultService resultService = new ResultService(new UserValidation().Validate(user));

            UserOutById userOutByIdVM = await _userRepository.GetByEmailAndPasswordAsync(user);

            return Mapper.Map<UserOutByIdVM>(userOutByIdVM);
        }

        public Task<UserOutByIdVM> GetUserById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}

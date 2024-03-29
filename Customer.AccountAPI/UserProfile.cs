﻿using AutoMapper;
using Customer.AccountAPI.Models;
using Customer.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Customer.Data;
using Customer.OrderFacade.Models;

namespace Customer.AccountAPI
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<CustomerDto, CustomerRepoModel>();
            CreateMap<CustomerRepoModel, CustomerDto>();
            CreateMap<CustomerRepoModel, Data.Customer>();
            CreateMap<Data.Customer, CustomerRepoModel>();
            CreateMap<CustomerDto, OrderingCustomerDto>();
            CreateMap<OrderingCustomerDto, CustomerDto>();
        }
    }
}

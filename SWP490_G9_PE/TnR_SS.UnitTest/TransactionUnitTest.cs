﻿using System;
using Moq;
using System.Collections.Generic;
using System.Linq;
using TnR_SS.Domain.Entities;
using TnR_SS.Domain.Supervisor;
using AutoMapper;
using TnR_SS.Domain.UnitOfWork;
using System.Linq.Expressions;
using TnR_SS.Domain.ApiModels;
using Xunit;
using System.Threading.Tasks;
using TnR_SS.Domain.ApiModels.PurchaseModal;
using TnR_SS.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TnR_SS.Domain.ApiModels.TransactionModel;
using TnR_SS.Domain.ApiModels.TransactionDetailModel;

namespace TnR_SS.UnitTest
{
    public class TransactionUnitTest
    {
        TnR_SSSupervisor _supervisor;
        IMapper _mapper;
        Mock<IUnitOfWork> _umock;
        DateTime date;
        List<Purchase> purchases;
        List<PurchaseDetail> purchaseDetails;
        List<ClosePurchaseDetail> closePurchaseDetails;
        List<FishType> fishTypes;
        List<Basket> baskets;
        List<PondOwner> pondOwners;
        List<Drum> drums;
        List<Truck> trucks;
        List<Buyer> buyers;
        List<Transaction> transactions;
        List<TransactionDetail> transactionDetails;
        public TransactionUnitTest()
        {
            Setup();
            MockSetup();
        }
        [Theory(DisplayName = "Transaction Controller: Test Get All")]
        [InlineData("")]
        [InlineData("1000000")]
        [InlineData("20/20/2021")]
        [InlineData("20-20-2021")]
        [InlineData("02022021")]
        [InlineData("29022021")]
        [InlineData("31022021")]
        [InlineData("922021")]
        public async Task TestGetAll(string date)
        {
            Mock<ITnR_SSSupervisor> mock = new Mock<ITnR_SSSupervisor>();
            mock.Setup(m => m.CreatePurchaseAsync(It.IsAny<PurchaseCreateReqModel>()));
            TransactionController purchase = new TransactionController(mock.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim("ID", "1"),
                        }, "mock"))
                    }
                }
            };
            var rs = await purchase.GetAll(date);
            if (date == "02022021")
            {
                Assert.Equal("Lấy thông tin hóa đơn thành công !!", rs.Message);
            }
            else
            {
                Assert.Equal("Lỗi format date !!!", rs.Message);
            }
        }
        [Theory(DisplayName = "Transaction Supervisor: Test Create Transaction")]
        [InlineData(1)]
        [InlineData(2)]
        public async Task TestCreateTransactionAsync(int userId)
        {
            TraderCreateTransactionReqModel model = new TraderCreateTransactionReqModel();
            if (userId == 1)
            {
                Exception ex = await Assert.ThrowsAsync<Exception>(async () => await _supervisor.TraderCreateTransactionAsync(model, userId));
                Assert.Equal("Đơn bán ngày đã có sẵn, tiếp tục mua thôi <3", ex.Message);
            }
            else
            {
                await _supervisor.TraderCreateTransactionAsync(model, userId);
            }
        }
        [Theory(DisplayName = "Transaction Supervisor: Test Create Transaction")]
        [InlineData(1)]
        [InlineData(2)]
        public async Task TestGetAllTransactionAsync(int userId)
        {
            TraderCreateTransactionReqModel model = new TraderCreateTransactionReqModel();
            if (userId == 1)
            {
                Exception ex = await Assert.ThrowsAsync<Exception>(async () => await _supervisor.TraderCreateTransactionAsync(model, userId));
                Assert.Equal("Đơn bán ngày đã có sẵn, tiếp tục mua thôi <3", ex.Message);
            }
            else
            {
                await _supervisor.TraderCreateTransactionAsync(model, userId);
            }
        }
        [Theory(DisplayName = "Transaction Supervisor: Test Get All Transaction Follow Date")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(1000)]
        public async Task TestGetAllTransactionFollowDateAsync(int id)
        {
            if (id == 1)
            {
                List<GetGeneralTransactionFollowDateResModel> res = await _supervisor.GetAllTransactionFollowDateAsync(id);
                Assert.Single(res);
            }
            else if (id == 2)
            {
                List<GetGeneralTransactionFollowDateResModel> res = await _supervisor.GetAllTransactionFollowDateAsync(id);
                Assert.Single(res);
            }
            else
            {
                Exception ex = await Assert.ThrowsAsync<Exception>(async () => await _supervisor.GetAllTransactionFollowDateAsync(id));
                Assert.Equal("Tài khoản không hợp lệ", ex.Message);
            }
        }
        [Theory(DisplayName = "Transaction Supervisor: Test Weight Recorder Create Transaction Detail")]
        [InlineData(1, 2, 1, 1)]
        [InlineData(1, 3, 1, 2)]
        [InlineData(1, 1000, 1, 2)]
        [InlineData(1000, 2, 1, 2)]
        [InlineData(1, 2, 1000, 2)]
        [InlineData(1, 2, 1, 1000)]
        public async Task TestWeightRecorderCreateTransactionDetailAsync(int fishTypeId, int transId, int buyerId, int id)
        {
            CreateTransactionDetailReqModel model = new CreateTransactionDetailReqModel()
            {
                FishTypeId = fishTypeId,
                TransId = transId,
                BuyerId = buyerId,
                SellPrice = 1000,
                Weight = 10,
                Date = date,
            };
            if (transId == 3)
            {
                Exception ex = await Assert.ThrowsAsync<Exception>(async () => await _supervisor.CreateTransactionDetailAsync(model, id));
                Assert.Equal("Đã chốt sổ với thương lái này, không thể tạo thêm !!", ex.Message);
            }
            else if (transId == 1000)
            {
                Exception ex = await Assert.ThrowsAsync<Exception>(async () => await _supervisor.CreateTransactionDetailAsync(model, id));
                Assert.Equal("Hãy tạo hóa đơn trước !!", ex.Message);
            }
            else if (buyerId == 1000)
            {
                Exception ex = await Assert.ThrowsAsync<Exception>(async () => await _supervisor.CreateTransactionDetailAsync(model, id));
                Assert.Equal("Người mua không tồn tại !!", ex.Message);
            }
            else if (fishTypeId == 1000)
            {
                Exception ex = await Assert.ThrowsAsync<Exception>(async () => await _supervisor.CreateTransactionDetailAsync(model, id));
                Assert.Equal("Loại cá không đúng !!", ex.Message);
            }
            else if (id == 1000)
            {
                Exception ex = await Assert.ThrowsAsync<Exception>(async () => await _supervisor.CreateTransactionDetailAsync(model, id));
                Assert.Equal("Người mua không tồn tại !!", ex.Message);
            }
            else
            {
                ArgumentNullException ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => await _supervisor.CreateTransactionDetailAsync(model, id));
                Assert.IsType<ArgumentNullException>(ex);
            }
        }
        [Theory(DisplayName = "Transaction Supervisor: Test Weight Recorder Update Transaction Detail")]
        [InlineData(1, 1, 1, 1)]
        [InlineData(1, 1, 1, 2)]
        [InlineData(1, 1000, 1, 1)]
        [InlineData(1000, 1, 1, 1)]
        [InlineData(1, 1, 1000, 1)]
        [InlineData(1, 1, 1, 1000)]
        public async Task TestWeightRecorderUpdateTransactionDetailAsync(int fishTypeId, int transId, int buyerId, int id)
        {
            UpdateTransactionDetailReqModel model = new UpdateTransactionDetailReqModel()
            {
                ID = transId,
                FishTypeId = fishTypeId,
                BuyerId = buyerId,
                SellPrice = 1000,
                Weight = 10,
                IsPaid = true
            };
            if (transId == 1000)
            {
                Exception ex = await Assert.ThrowsAsync<Exception>(async () => await _supervisor.UpdateTransactionDetailAsync(model, id));
                Assert.Equal("Không tìm thấy đơn mua !!", ex.Message);
            }
            else if (buyerId == 1000)
            {
                Exception ex = await Assert.ThrowsAsync<Exception>(async () => await _supervisor.UpdateTransactionDetailAsync(model, id));
                Assert.Equal("Không tìm thấy người mua !!", ex.Message);
            }
            else if (fishTypeId == 1000)
            {
                Exception ex = await Assert.ThrowsAsync<Exception>(async () => await _supervisor.UpdateTransactionDetailAsync(model, id));
                Assert.Equal("Loại cá không đúng !!", ex.Message);
            }
            else if (id == 1000)
            {
                Exception ex = await Assert.ThrowsAsync<Exception>(async () => await _supervisor.UpdateTransactionDetailAsync(model, id));
                Assert.Equal("Không tìm thấy người mua !!", ex.Message);
            }
            else if (id == 2)
            {
                Exception ex = await Assert.ThrowsAsync<Exception>(async () => await _supervisor.UpdateTransactionDetailAsync(model, id));
                Assert.Equal("Không tìm thấy người mua !!", ex.Message);
            }
            else
            {
                await _supervisor.UpdateTransactionDetailAsync(model, id);
            }
        }
        [Theory(DisplayName = "Transaction Supervisor: Test Get Payment For Buyers Async")]
        [InlineData(1)]
        [InlineData(1000)]
        public async Task TestGetPaymentForBuyersAsync(int id)
        {
            if (id == 1)
            {
                List<PaymentForBuyer> res = await _supervisor.GetPaymentForBuyersAsync(id, date);
                Assert.Single(res);
            }
            else
            {
                List<PaymentForBuyer> res = await _supervisor.GetPaymentForBuyersAsync(id, date);
                Assert.Empty(res);
            }
        }
        [Theory(DisplayName = "Transaction Supervisor: Test Get Payment For Buyers Async")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(1000)]
        public async Task TestPaymentForBuyersAsync(int buyer)
        {
            FinishPaymentBuyerReqModel model = new FinishPaymentBuyerReqModel() { BuyerId = buyer, Date = date };
            if (buyer == 1)
            {
                await _supervisor.PaymentForBuyersAsync(model, 1);
            }
            else if (buyer == 2)
            {
                Exception ex = await Assert.ThrowsAsync<Exception>(async () => await _supervisor.PaymentForBuyersAsync(model, 1));
                Assert.Equal("Người này chưa mua gì cả !!!", ex.Message);
            }
            else
            {
                Exception ex = await Assert.ThrowsAsync<Exception>(async () => await _supervisor.PaymentForBuyersAsync(model, 1));
                Assert.Equal("Không có thông tin người mua này !!!", ex.Message);
            }
        }
        void MockSetup()
        {
            _umock = new Mock<IUnitOfWork>();
            _umock.Setup(u => u.Purchases.GetAll(It.IsAny<Expression<Func<Purchase, bool>>>(),
                It.IsAny<Func<IQueryable<Purchase>, IOrderedQueryable<Purchase>>>(),
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>())).Returns((
                Expression<Func<Purchase, bool>> filter,
                Func<IQueryable<Purchase>, IOrderedQueryable<Purchase>> orderBy,
                string includeProperties,
                int? skip, int? take
                ) => purchases.Where(filter.Compile()).ToList());
            _umock.Setup(u => u.PurchaseDetails.GetAll(It.IsAny<Expression<Func<PurchaseDetail, bool>>>(),
                It.IsAny<Func<IQueryable<PurchaseDetail>, IOrderedQueryable<PurchaseDetail>>>(),
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .Returns((
                Expression<Func<PurchaseDetail, bool>> filter,
                Func<IQueryable<PurchaseDetail>, IOrderedQueryable<PurchaseDetail>> orderBy,
                string includeProperties,
                int? skip, int? take
                ) => purchaseDetails.Where(filter.Compile()).ToList());
            _umock.Setup(u => u.Transactions.GetAll(It.IsAny<Expression<Func<Transaction, bool>>>(),
                It.IsAny<Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>>>(),
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .Returns((
                Expression<Func<Transaction, bool>> filter,
                Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>> orderBy,
                string includeProperties,
                int? skip, int? take
                ) => transactions.Where(filter.Compile()).ToList());
            _umock.Setup(u => u.TransactionDetails.GetAll(It.IsAny<Expression<Func<TransactionDetail, bool>>>(),
                It.IsAny<Func<IQueryable<TransactionDetail>, IOrderedQueryable<TransactionDetail>>>(),
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .Returns((
                Expression<Func<TransactionDetail, bool>> filter,
                Func<IQueryable<TransactionDetail>, IOrderedQueryable<TransactionDetail>> orderBy,
                string includeProperties,
                int? skip, int? take
                ) => transactionDetails.Where(filter.Compile()).ToList());
            _umock.Setup(u => u.Buyers.GetAll(It.IsAny<Expression<Func<Buyer, bool>>>(),
                It.IsAny<Func<IQueryable<Buyer>, IOrderedQueryable<Buyer>>>(),
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .Returns((
                Expression<Func<Buyer, bool>> filter,
                Func<IQueryable<Buyer>, IOrderedQueryable<Buyer>> orderBy,
                string includeProperties,
                int? skip, int? take
                ) => buyers.Where(filter.Compile()).ToList());

            _umock.Setup(u => u.ClosePurchaseDetails.GetAllByPurchase(It.IsAny<Purchase>()))
                .Returns((Purchase purchase) => closePurchaseDetails.Where(pd => pd.PurchaseId == purchase.ID).ToList());
            _umock.Setup(u => u.Drums.GetDrumsByPurchaseDetail(It.IsAny<PurchaseDetail>())).Returns(drums);
            _umock.Setup(u => u.Drums.GetDrumsByClosePurchaseDetail(It.IsAny<ClosePurchaseDetail>())).Returns(drums);
            _umock.Setup(u => u.Transactions.GetAllTransactionsByDate(It.IsAny<int>(), It.IsAny<DateTime?>()))
                .Returns((int id, DateTime? date) => transactions.Where(t => (t.TraderId == id || t.WeightRecorderId == id) && t.Date == date).ToList());
            _umock.Setup(u => u.TransactionDetails.GetAllByListTransaction(It.IsAny<List<int>>())).Returns((List<int> list) => list.Count > 0 ? transactionDetails : new List<TransactionDetail>());

            foreach (Purchase purchase in purchases)
            {
                _umock.Setup(m => m.Purchases.FindAsync(It.Is<int>(id => id == purchase.ID))).ReturnsAsync(purchase);
            }
            foreach (PondOwner pondOwner in pondOwners)
            {
                _umock.Setup(m => m.PondOwners.FindAsync(It.Is<int>(id => id == pondOwner.ID))).ReturnsAsync(pondOwner);
            }
            foreach (FishType fishType in fishTypes)
            {
                _umock.Setup(m => m.FishTypes.FindAsync(It.Is<int>(id => id == fishType.ID))).ReturnsAsync(fishType);
            }
            foreach (Basket basket in baskets)
            {
                _umock.Setup(m => m.Baskets.FindAsync(It.Is<int>(id => id == basket.ID))).ReturnsAsync(basket);
            }
            foreach (Truck truck in trucks)
            {
                _umock.Setup(m => m.Trucks.FindAsync(It.Is<int>(id => id == truck.ID))).ReturnsAsync(truck);
            }
            foreach (Buyer buyer in buyers)
            {
                _umock.Setup(m => m.Buyers.FindAsync(It.Is<int>(id => id == buyer.ID))).ReturnsAsync(buyer);
            }
            foreach (Transaction transaction in transactions)
            {
                _umock.Setup(m => m.Transactions.FindAsync(It.Is<int>(id => id == transaction.ID))).ReturnsAsync(transaction);
            }
            foreach (TransactionDetail transactionDetail in transactionDetails)
            {
                _umock.Setup(m => m.TransactionDetails.FindAsync(It.Is<int>(id => id == transactionDetail.ID))).ReturnsAsync(transactionDetail);
            }
            _umock.Setup(m => m.UserInfors.FindAsync(It.Is<int>(id => id == 1))).ReturnsAsync(new UserInfor() { Id = 1 });
            _umock.Setup(m => m.UserInfors.FindAsync(It.Is<int>(id => id == 2))).ReturnsAsync(new UserInfor() { Id = 2 });
            _umock.Setup(m => m.UserInfors.GetRolesAsync(It.Is<int>(id => id == 1))).ReturnsAsync(new List<string>() { "Trader" });
            _umock.Setup(m => m.UserInfors.GetRolesAsync(It.Is<int>(id => id == 2))).ReturnsAsync(new List<string>() { "WeightRecorder" });
            _umock.Setup(m => m.UserInfors.GetRolesAsync(It.Is<UserInfor>(id => id.Id == 1))).ReturnsAsync(new List<string>() { "Trader" });
            _umock.Setup(m => m.UserInfors.GetRolesAsync(It.Is<UserInfor>(id => id.Id == 2))).ReturnsAsync(new List<string>() { "WeightRecorder" });
            _umock.Setup(m => m.UserInfors.GetRolesAsync(It.Is<int>(id => !(id == 2 || id == 1)))).ReturnsAsync(new List<string>() { });
            _supervisor = new TnR_SSSupervisor(_mapper, _umock.Object);
        }
        void Setup()
        {
            date = DateTime.Now;
            purchases = new List<Purchase>()
            {
                new Purchase()
                {
                    ID=1,
                    PayForPondOwner=50000,
                    Date=date,
                    Commission=1,
                    SentMoney=1000,
                    PondOwnerID=1,
                    isCompleted=PurchaseStatus.Pending,
                    TraderID=1
                },
                new Purchase()
                {
                    ID=2,
                    PayForPondOwner=60000,
                    Date=date.AddDays(-1),
                    Commission=2,
                    SentMoney=2000,
                    PondOwnerID=1,
                    isCompleted=PurchaseStatus.Completed,
                    TraderID=1
                }
            };
            purchaseDetails = new List<PurchaseDetail>()
            {
                new PurchaseDetail()
                {
                    ID=1,
                    FishTypeID=1,
                    Weight=100,
                    BasketId=1,
                    PurchaseId=1
                },
                new PurchaseDetail()
                {
                    ID=2,
                    FishTypeID=1,
                    Weight=120,
                    BasketId=1,
                    PurchaseId=1
                }
            };
            closePurchaseDetails = new List<ClosePurchaseDetail>()
            {
                new ClosePurchaseDetail()
                {
                    ID=1,
                    FishTypeId=2,
                    Weight=120,
                    BasketId=1,
                    PurchaseId=2
                }
            };
            pondOwners = new List<PondOwner>(){
                new PondOwner()
                {
                    ID = 1,
                    Name = "Anh Duc",
                    Address = "Ha Noi",
                    PhoneNumber = "0912345678",
                    TraderID = 1
                },
                new PondOwner()
                {
                    ID = 2,
                    Name = "Duc Quan",
                    Address = "Ha Noi",
                    PhoneNumber = "0912345671",
                    TraderID = 1
                }
            };
            baskets = new List<Basket>()
            {
                new Basket()
                {
                    ID = 1,
                    TraderID = 1,
                    Type = "Tron",
                    Weight = 3,
                }
            };
            drums = new List<Drum>()
            {
                new Drum()
                {
                    ID=1,
                    TruckID=1,
                    Number="123456",
                    Type="1",
                    MaxWeight=10,
                }
            };
            trucks = new List<Truck>()
            {
                new Truck()
                {
                    ID=1,
                    LicensePlate="123456",
                    Name="1",
                    TraderID=1,
                }
            };
            baskets = new List<Basket>()
            {
                new Basket()
                {
                    ID=1,
                    Type="To",
                    Weight=10,
                    TraderID=1,
                }
            };
            fishTypes = new List<FishType>()
            {
                new FishType()
                {
                    ID = 1,
                    FishName = "Tram",
                    MinWeight = 1,
                    MaxWeight = 10,
                    Price = 100,
                    Description = "",
                    TraderID = 1,
                    Date = date,
                    PurchaseID = 1
                }
            };
            buyers = new List<Buyer>()
            {
                new Buyer()
                {
                    ID=1,
                    Address="HD",
                    Name="Tam",
                    SellerId=1,
                    PhoneNumber="0912345678"
                },
                new Buyer()
                {
                    ID=2,
                    Address="HD",
                    Name="Tien",
                    SellerId=1,
                    PhoneNumber="0912345671"
                }
            };
            transactions = new List<Transaction>()
            {
                new Transaction()
                {
                    ID=1,
                    TraderId=1,
                    Date= new DateTime(date.Year,date.Month,date.Day),
                    isCompleted=TransactionStatus.Pending,
                    WeightRecorderId=null,
                    CommissionUnit=2
                },
                new Transaction()
                {
                    ID=2,
                    TraderId=1,
                    Date= new DateTime(date.Year,date.Month,date.Day),
                    isCompleted=TransactionStatus.Pending,
                    WeightRecorderId=2,
                    CommissionUnit=2
                },
                new Transaction()
                {
                    ID=3,
                    TraderId=1,
                    Date= new DateTime(date.Year,date.Month,date.Day),
                    isCompleted=TransactionStatus.Completed,
                    WeightRecorderId=2,
                    CommissionUnit=2
                }
            };
            transactionDetails = new List<TransactionDetail>()
            {
                new TransactionDetail()
                {
                    ID=1,
                    FishTypeId=1,
                    TransId=1,
                    BuyerId=1,
                    IsPaid=false,
                    SellPrice=1000,
                    Weight=10
                }
            };
            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile(new MapperProfile())));
        }
    }
}

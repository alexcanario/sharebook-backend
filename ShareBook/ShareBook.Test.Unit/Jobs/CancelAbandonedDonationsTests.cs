﻿using Moq;
using Sharebook.Jobs;
using ShareBook.Service;
using System.Threading.Tasks;
using Xunit;
using ShareBook.Repository;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Test.Unit.Mocks;
using ShareBook.Domain.Common;
using System.Linq;
using System;

namespace ShareBook.Test.Unit.Jobs
{
    public class CancelAbandonedDonationsTests
    {
        private readonly Mock<IJobHistoryRepository> _mockJobHistoryRepository = new();
        private readonly Mock<IBookService> _mockBookService = new();
        private readonly Mock<IBookUserService> _mockBookUserService = new();
        private readonly Mock<IConfiguration> _mockConfiguration = new();

        public CancelAbandonedDonationsTests()
        {
            // Mocking 4 books. 2 Of them are ready to be cancelled.
            int maxLateDonationDaysAutoCancel = 90;
            List<Book> allBooks = new List<Book> {
                    BookMock.GetLordTheRings(),
                    BookMock.GetLordTheRings(),
                    BookMock.GetLordTheRings(),
                    BookMock.GetLordTheRings(),
                };
            var booksToCancel = allBooks.Take(2);
            foreach (var book in booksToCancel) 
                book.ChooseDate = DateTime.Now.AddDays((maxLateDonationDaysAutoCancel + 2) * -1);

            _mockConfiguration.SetupGet(s => s[It.IsAny<string>()]).Returns(maxLateDonationDaysAutoCancel.ToString());
            _mockBookService.Setup(s => s.GetBooksChooseDateIsLateAsync()).ReturnsAsync(allBooks);
            _mockBookUserService.Setup(s => s.CancelAsync(It.IsAny<BookCancelationDTO>())).ReturnsAsync(() => new Result<Book>(BookMock.GetLordTheRings()));
        }

        [Fact]
        public async Task Cancelling2Of4AbandonedBooks()
        {
            CancelAbandonedDonations job = new CancelAbandonedDonations(_mockJobHistoryRepository.Object, _mockBookService.Object, _mockBookUserService.Object, _mockConfiguration.Object);
            
            JobHistory result = await job.WorkAsync();
            
            Assert.True(result.IsSuccess);
            Assert.Equal("Encontradas 2 doações abandonadas com mais de 90 dias de atraso.\n\nDoação do livro Lord of the Rings foi cancelada.\nDoação do livro Lord of the Rings foi cancelada.\n", result.Details);
            Assert.Equal(CancelAbandonedDonations.JobName, result.JobName);
            
            _mockBookService.Verify(c => c.GetBooksChooseDateIsLateAsync(), Times.Once);
            _mockBookUserService.Verify(c => c.CancelAsync(It.IsAny<BookCancelationDTO>()), Times.Exactly(2));
            _mockBookService.VerifyNoOtherCalls();
            _mockBookUserService.VerifyNoOtherCalls();
        }
    }
}

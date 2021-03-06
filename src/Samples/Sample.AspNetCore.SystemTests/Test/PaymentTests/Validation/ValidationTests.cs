﻿using Atata;
using NUnit.Framework;
using Sample.AspNetCore.SystemTests.Services;
using Sample.AspNetCore.SystemTests.Test.Helpers;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.Exceptions;
using SwedbankPay.Sdk.Payments;
using SwedbankPay.Sdk.Payments.SwishPayments;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Sample.AspNetCore.SystemTests.Test.PaymentTests.Validation
{
    public class ValidationTests : Base.PaymentTests
    {
        public ValidationTests(string driverAlias)
            : base(driverAlias)
        {
        }


        [Test]
        [TestCaseSource(nameof(TestData), new object[] { true, null })]
        public void FieldValidationCard(Product[] products)
        {
            GoToPayexCardPaymentFrame(products)
                .CreditCardNumber.Set("abc")
                .ExpiryDate.Set("abcd")
                .Cvc.Set("abc")
                .CreditCardNumber.Click()
                .ValidationIcons[x => x.CreditCardNumber].Should.BeVisible()
                .ValidationIcons[x => x.ExpiryDate].Should.BeVisible()
                .CreditCardNumber.Set(TestDataService.CreditCardNumber)
                .ExpiryDate.Set(TestDataService.CreditCardExpiratioDate)
                .Cvc.Set(TestDataService.CreditCardCvc)
                .ValidationIcons[x => x.CreditCardNumber].Should.Not.BeVisible()
                .ValidationIcons[x => x.ExpiryDate].Should.Not.BeVisible();
        }


        [Test]
        [TestCaseSource(nameof(TestData), new object[] { true, null })]
        public void FieldValidationInvoice(Product[] products)
        {
            GoToPayexInvoicePaymentFrame(products)
                .PersonalNumber.Set("abc")
                .Email.Set("abc")
                .PhoneNumber.Set("abc")
                .ZipCode.Set("abc")
                .PersonalNumber.Click()
                .ValidationIcons[x => x.PersonalNumber].Should.BeVisible()
                .ValidationIcons[x => x.Email].Should.BeVisible()
                .ValidationIcons[x => x.PhoneNumber].Should.BeVisible()
                .ValidationIcons[x => x.ZipCode].Should.BeVisible()
                .PersonalNumber.Set(TestDataService.PersonalNumberShort)
                .Email.Set(TestDataService.Email)
                .PhoneNumber.Set(TestDataService.PhoneNumber)
                .ZipCode.Set(TestDataService.ZipCode)
                .ValidationIcons[x => x.PersonalNumber].Should.Not.BeVisible()
                .ValidationIcons[x => x.Email].Should.Not.BeVisible()
                .ValidationIcons[x => x.PhoneNumber].Should.Not.BeVisible()
                .ValidationIcons[x => x.ZipCode].Should.Not.BeVisible();
        }


        [Test]
        [TestCaseSource(nameof(TestData), new object[] { true, null })]
        public void FieldValidationSwish(Product[] products)
        {
            GoToPayexSwishPaymentFrame(products);
        }

        [Test]
        public void ValidateExceptionFromApi()
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("https://api.externalintegration.payex.com") };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "xxxxx");
            var swedbankPayClient = new SwedbankPayClient(httpClient);
            var payeeRef = DateTime.Now.Ticks.ToString();
            var amount = Amount.FromDecimal(1600);
            var vatAmount = Amount.FromDecimal(0);
            var phoneNumber = "+46739000001";
            var swishRequest = new SwishPaymentRequest(new CurrencyCode("SEK"),
                new List<Price>
                {
                    new Price(amount, PriceType.Swish, vatAmount)
                }, "Test Purchase", payeeRef, "GetUserAgent()",
                CultureInfo.GetCultureInfo("sv-SE"),
                new Urls(new List<Uri> { new Uri("http://api.externalintegration.payex.com") },
                    new Uri("http://api.externalintegration.payex.com"),
                    new Uri("http://api.externalintegration.payex.com")),
                new PayeeInfo(Guid.NewGuid(), payeeRef), new PrefillInfo(new Msisdn(phoneNumber)));

            var error = Assert.ThrowsAsync<HttpResponseException>(() => swedbankPayClient.Payments.SwishPayments.Create(swishRequest));

            Assert.AreEqual(1, error.Data.Keys.Count);
        }
    }
}
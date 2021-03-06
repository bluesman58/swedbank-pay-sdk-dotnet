﻿using System;

namespace SwedbankPay.Sdk.Payments.VippsPayments
{
    public class VippsPaymentAuthorizationResponse
    {
        public VippsPaymentAuthorizationResponse(Uri payment, VippsPaymentAuthorization authorization)
        {
            Payment = payment;
            Authorization = authorization;
        }


        public VippsPaymentAuthorization Authorization { get; }

        public Uri Payment { get; }
    }
}
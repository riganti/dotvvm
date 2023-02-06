﻿using System;
using DotVVM.Framework.CodeAnalysis;

namespace DotVVM.Framework.Binding.HelperNamespace
{
    public static class DateTimeExtensions
    {

        /// <summary>
        /// Converts the date (assuming it is in UTC) to browser's local time.
        /// </summary>
        [UnsupportedCallSite(CallSiteType.ServerSide, "When evaluated on the server, no conversion is made as we don't know the browser timezone.")]
        public static DateTime ToBrowserLocalTime(this DateTime value) => value;

        /// <summary>
        /// Converts the date (assuming it is in UTC) to browser's local time.
        /// </summary>
        [UnsupportedCallSite(CallSiteType.ServerSide, "When evaluated on the server, no conversion is made as we don't know the browser timezone.")]
        public static DateTime? ToBrowserLocalTime(this DateTime? value) => value;

    }
}

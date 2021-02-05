using System;
using Xunit;

namespace FixPriceBruteForce.Tests
{
    public class FixPrice_getNormalizePhoneNumber_Test
    {
        [Theory]
        [InlineData(79995396765)]
        [InlineData(89995396765)]
        [InlineData(9995396765)]
        public void getNormalizePhoneNumber_ValidPhoneNumberAsLong_ReturnValidPhoneNumber(long phone)
        {
            FixPriceBruter fixprice = new FixPriceBruter();

            string actual = fixprice.getNormalizePhoneNumber(phone);

            string expected = "+7 (999) 539-67-65";

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(0)]
        [InlineData(69995396765)]
        [InlineData(null)]
        public void getNormalizePhoneNumber_InvalidPhoneNumberAsLong_ReturnNull(long? phone)
        {
            FixPriceBruter fixprice = new FixPriceBruter();

            string actual = fixprice.getNormalizePhoneNumber(phone.GetValueOrDefault());

            Assert.Null(actual);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace FixPriceBruteForce
{
    
    interface Bruter
    {
        void Brute();
    }
    public class FixPriceBruter : Bruter
    {
        public void Brute()
        {
            throw new NotImplementedException();
        }

        public string getNormalizePhoneNumber(long phonenumber)
        {
            if (phonenumber.Equals(null)) return null;
            string phone = phonenumber.ToString();

            if (phone.Length == 11)
            {
                bool beginWith7 = phone[0] == '7';
                bool beginWith8 = phone[0] == '8';
                if(beginWith7||beginWith8)
                {
                    return $"+7 ({phone[1]}{phone[2]}{phone[3]}) {phone[4]}{phone[5]}{phone[6]}-{phone[7]}{phone[8]}-{phone[9]}{phone[10]}";
                }
                else
                {
                    return null;
                }
            }
            else if (phone.Length == 10)
            {
                return $"+7 ({phone[0]}{phone[1]}{phone[2]}) {phone[3]}{phone[4]}{phone[5]}-{phone[6]}{phone[7]}-{phone[8]}{phone[9]}";
            }
            else
            {
                return null;
            }
        }




    }
}

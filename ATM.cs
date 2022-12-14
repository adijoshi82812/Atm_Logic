using System;
using System.Collections.Generic;

namespace ATM {
    public class UserInfo {
        public long account_number { get; set; }
        public string name { get; set; }
        public uint passcode { get; set; }
        public bool has_verified { get; set; }
        public uint amount { get; set; }
    }
    
    public class Program {
        private const uint passcode = 1111;
        private static List<UserInfo> user_info = new List<UserInfo>();
        
        public static void Main(String[] args) {
            Console.WriteLine("ATM");
            
            int choice;
            while(true) {
                Console.WriteLine("----------------");
                Console.Write("[1] Add a user ");
                Console.Write("[2] Deposit ");
                Console.Write("[3] View Balance ");
                Console.Write("[4] Verify: ");
                choice = Convert.ToInt32(Console.ReadLine());
                
                switch(choice) {
                    case 1:
                        try {
                            bool success = validate_admin();
                            if(success) {
                                add_user();
                            }
                        } catch (Exception e) {
                            Console.WriteLine(e.Message);
                        }
                        break;
                        
                    case 2:
                        try {
                            Console.WriteLine("----------------------");
                            Console.Write("Enter account number: ");
                            long account_number = (long)Convert.ToDouble(Console.ReadLine());
                            deposit(account_number);
                        } catch(Exception e) {
                            Console.WriteLine(e.Message);
                        }
                        break;
                        
                    case 3:
                        try {
                            Console.WriteLine("------------------");
                            Console.Write("Enter account number: ");
                            long account_number = (long)Convert.ToDouble(Console.ReadLine());
                            view_balance(account_number);
                        } catch(Exception e) {
                            Console.WriteLine(e.Message);
                        }
                        break;
                        
                    case 4:
                        try {
                            verify();
                        } catch(Exception e) {
                            Console.WriteLine(e.Message);
                        }
                        break;
                        
                    default:
                        break;
                }
            }
        }
        
        public static bool validate_admin() {
            Console.WriteLine("----------------------");
            Console.Write("Enter admin passcode: ");
            uint passcode = (uint)Convert.ToInt32(Console.ReadLine());
            isFourDigits(passcode);
            
            if(passcode != Program.passcode) {
                throw new Exception("Wrong passcode");
                return false;
            } else {
                return true;
            }
        }
        
        public static void isFourDigits(uint passcode) {
            int digits = 0;
            while(passcode != 0) {
                digits += 1;
                passcode /= 10;
            }
            
            if(digits != 4) {
                throw new Exception("Not 4 digits");
            }
        }
        
        public static void add_user() {
            Console.Write("Enter name: ");
            string name = Console.ReadLine();
            
            Random r = new Random();
            uint passcode = (uint)r.Next(1000, 10000);
            
            long account_number = generate_account_number();
            
            user_info.Add(
                new UserInfo() {
                    account_number = account_number,
                    name = name, 
                    passcode = passcode,
                    has_verified = false,
                    amount = 1000
                }
            );
            
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Account Details: ");
            Console.WriteLine("Account Number: {0}", account_number);
            Console.WriteLine("Account Holder Name: {0}", name);
            Console.WriteLine("Temp passcode: {0}", passcode);
        }
        
        public static long generate_account_number() {
            long account_number = LongRandom(1000000000, 10000000000, new Random());
            
            foreach(UserInfo uf in user_info) {
                if(uf.account_number == account_number) {
                    generate_account_number();
                    break;
                }
            }
            
            return account_number;
        }
        
        public static long LongRandom(long min, long max, Random rand) {
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);
        
            return (Math.Abs(longRand % (max - min)) + min);
        }
        
        public static bool deposit(long account_number) {
            int exists = 0;
            bool is_verified = false;
            UserInfo uf2 = new UserInfo();
            foreach(UserInfo uf in user_info) {
                if(uf.account_number == account_number) {
                    exists = 1;
                    is_verified = uf.has_verified;
                    uf2 = uf;
                    break;
                }
            }
            
            if(exists != 1) {
                throw new Exception("Accout not found");
                return false;
            } 
            
            if (!is_verified) {
                bool success = verify(uf2);
                if(!success) {
                    Console.WriteLine("Wrong temp passcode");
                    Console.WriteLine("-------------------");
                    deposit(account_number);
                    return true;
                }
            } else {
                verify_passcode(uf2);
            }
            
            Console.Write("Enter amount: ");
            uint amount = (uint)Convert.ToInt32(Console.ReadLine());
            bool successful_deposit = send_deposit(uf2, amount);
            
            if(successful_deposit) {
                Console.WriteLine("Deposited {0} to {1}", amount, uf2.account_number);
            } else {
                throw new Exception("Something went wrong");
            }
            
            return true;
        }
        
        public static bool verify(UserInfo uf) {
            Console.Write("Enter your temp passcode: ");
            uint temp_passcode = (uint)Convert.ToInt32(Console.ReadLine());
            
            if(uf.passcode == temp_passcode) {
                while(true){
                    Console.Write("Enter new passcode: ");
                    uint passcode = (uint)Convert.ToInt32(Console.ReadLine());
                    
                    Console.Write("Enter new passcode again: ");
                    uint passcodeAgain = (uint)Convert.ToInt32(Console.ReadLine());
                    
                    isFourDigits(passcode);
                    
                    if(passcode != passcodeAgain) {
                        Console.WriteLine("Passcodes different");
                        Console.WriteLine("-------------------");
                        continue;
                    } else {
                        uf.passcode = passcode;
                        uf.has_verified = true;
                        Console.WriteLine("You have been verified");
                        Console.WriteLine("----------------------");
                        return true;
                    }
                }
            } else {
                return false;
            }
        }
        
        public static bool verify_passcode(UserInfo uf) {
            Console.Write("Enter your passcode: ");
            uint passcode = (uint)Convert.ToInt32(Console.ReadLine());
            
            if(passcode != uf.passcode) {
                Console.WriteLine("Wrong passcode");
                Console.WriteLine("--------------");
                verify_passcode(uf);
                return true;
            } else {
                return true;
            }
        }
        
        public static bool send_deposit(UserInfo uf, uint amount) {
            uint amount_before = uf.amount;
            uf.amount = uf.amount + amount;
            
            if(uf.amount > amount_before) {
                return true;
            } else {
                return false;
            }
        }
        
        public static void view_balance(long account_number) {
            int exists = 0;
            bool is_verified = false;
            UserInfo uf2 = new UserInfo();
            foreach(UserInfo uf in user_info) {
                if(uf.account_number == account_number) {
                    exists = 1;
                    is_verified = uf.has_verified;
                    uf2 = uf;
                    break;
                }
            }
            
            if(!is_verified) {
                Console.WriteLine("Please verify yourself");
                goto END;
            }
            
            ONE:
            if(exists != 1) {
                Console.WriteLine("Account not found");
            } else {
                Console.Write("Enter your passcode: ");
                uint passcode = (uint)Convert.ToInt32(Console.ReadLine());
                
                if(passcode != uf2.passcode) {
                    Console.WriteLine("Wrong passcode");
                    Console.WriteLine("--------------");
                    goto ONE;
                } else {
                    Console.WriteLine("Account Number: {0}", uf2.account_number);
                    Console.WriteLine("Account Holder Name: {0}", uf2.name);
                    Console.WriteLine("Account Balance: {0}", uf2.amount);
                }
            }
            
            END:;
        }
        
        public static void verify() {
            Console.WriteLine("------------------");
            Console.Write("Enter account number: ");
            long account_number = (long)Convert.ToDouble(Console.ReadLine());
            
            int exists = 0;
            UserInfo uf2 = new UserInfo();
            bool is_verified = false;
            foreach(UserInfo uf in user_info) {
                if(uf.account_number == account_number) {
                    exists = 1;
                    is_verified = uf.has_verified;
                    uf2 = uf;
                    break;
                }
            }
            
            ONE:
            if(exists != 1) {
                throw new Exception("Account does not exists");
            } else if(is_verified) {
                throw new Exception("Account already verified");
            } else {
                Console.WriteLine("----------------------");
                Console.Write("Enter your temp passcode: ");
                uint temp_passcode = (uint)Convert.ToInt32(Console.ReadLine());
                
                if(temp_passcode != uf2.passcode) {
                    Console.WriteLine("Wrong temp passcode");
                    goto ONE;
                } 
                else
                {
                    TWO:
                    Console.Write("Enter new passcode: ");
                    uint passcode = (uint)Convert.ToInt32(Console.ReadLine());
                    
                    Console.Write("Enter passcode again: ");
                    uint passcode_verify = (uint)Convert.ToInt32(Console.ReadLine());
                    
                    if(passcode != passcode_verify) {
                        Console.WriteLine("Passcodes does not match");
                        Console.WriteLine("------------------------");
                        goto TWO;
                    } else {
                        uf2.passcode = passcode;
                        uf2.has_verified = true;
                        goto END;
                    }
                }
            }
            END:
            Console.WriteLine("{0} verified and updated", uf2.account_number);
        }
    }
}
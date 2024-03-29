﻿using VendingMachine.DefaultContinueFunctions;
using VendingMachine.SQLcomunication;

namespace VendingMachine.ProdClientObj
{
    internal class ClientObj: DefaultOptions
    {
        private DateTime startOfTransactions = DateTime.Now;
        public decimal userFunds = 0;
        public decimal moneySpend = 0;
        private bool firstPurchase = true;
        private Dictionary<int, (int amount, string name) > purchases = new Dictionary<int, (int amount, string name)>();


        /// <summary>
        /// Used at the beginning of application to set user funds. Checks if funds are in correct format and in not negative amount using: 'FundChecker()' method.
        /// </summary>
        public void SetUserFunds()
        {
            FundChecker("Pleas input how much money you have: "); 
        }


        /// <summary>
        /// Used to add funds for client. Checks if funds are in correct format and are not numbers lower than zero using: 'FundChecker()' method.
        /// </summary>
        public void AddFunds()
        {
            FundChecker("How much $ would you like to add to your funds? : ");
        }


        /// <summary>
        /// Check if client tries to input negative number or something that is not a number.
        /// </summary>
        /// <param name="mesage">Used to show message client will see before asking for his input.</param>
        private void FundChecker(string mesage)
        {
            while (true)
            {
                Console.Clear();
                Console.Write(mesage);
                try
                {
                    decimal newUserFunds = decimal.Parse(Console.ReadLine());

                    if (newUserFunds < 0)
                    {
                        Console.Clear();
                        Console.WriteLine("You cannot input negative decimal number. Try again.\n");
                        ToContinue();
                        

                    }
                    else { this.userFunds += newUserFunds; break; }
                }
                catch (Exception e)
                {
                    Console.Clear();
                    Console.WriteLine("You did not input decimal number. Try again. \n");
                    ToContinue();



                }

            }
        }


        /// <summary>
        /// Changes client funds in: 'userFunds' and: 'moneySpend' values based on 'money' paramiter. Adds: 'productID' to dictionary called: 'purchases' if it is not on it yet.
        /// Sets key as: 'productID' and values as: amount = 1, name: 'name' paramiter.
        /// If: 'productID' is in dictionary it will increase its amount by 1.
        /// Sends data to sql database to increase money in machine by: 'money' paramiter.
        /// </summary>
        /// <param name="money">Amount of money client gonna spend on product. </param>
        /// <param name="productID">ID/place of a product that was brought. </param>
        /// <param name="name">Name of a product that was brought. </param>
        public void TransactonMechanism(decimal money, int productID, string name) {

            userFunds -= money;
            moneySpend += money;

            string moneySpentFormatted = money.ToString().Replace(',', '.');
            ISQLcomunication.SendData($"UPDATE [Money In Machine] SET [Current Money In Machine] = [Current Money In Machine] + {moneySpentFormatted} WHERE [Money ID] = 1");
            SendTransactionsInfo();
            if (purchases.ContainsKey(productID))
            {
                int currentAmount = purchases[productID].amount;
                currentAmount++;
                string currentName = purchases[productID].name;

                purchases[productID] = (currentAmount, currentName);
            }
            else
            {
                purchases.Add(productID, (1, name));
            }

            
        }

        /// <summary>
        /// Shows all products client has brought. If client has brought nothig it will show: 'Nothing. '.
        /// </summary>
        private void PurchasesList()
        {
            int i = 1;
            Console.WriteLine($"You have brought: \n");
            if (purchases.Count() == 0) { Console.WriteLine("Nothing. \n"); }
            foreach (var product in purchases)
            {
                
                Console.WriteLine($"{i}. {product.Value.name} in amount of: {product.Value.amount}. \n");
                i++;
                                  
            }
        }

        /// <summary>
        /// Method to send and update data to sql database column: 'Transactions' based on: 'moneySpend' value.
        /// </summary>

        public void SendTransactionsInfo()
        {

            DateTime endTime = DateTime.Now;
            TimeSpan timeOfTransaction = endTime - startOfTransactions;
            string moneySpentFormatted = this.moneySpend.ToString().Replace(',', '.');
            if (firstPurchase == true)
            {
                
                ISQLcomunication.SendData($"INSERT INTO Transactions VALUES ('{startOfTransactions.ToString("yyyy-MM-dd")}', '{timeOfTransaction}', {moneySpentFormatted})");
                firstPurchase = false;            
            }
            else if(firstPurchase == false)
            {
                
                ISQLcomunication.SendData($"UPDATE Transactions SET Profits = {moneySpentFormatted}, [Lenght Of Transaction] = '{timeOfTransaction}' WHERE [Transaction ID] = (SELECT MAX([Transaction ID]) FROM Transactions)");
            }
        }


        /// <summary>
        /// Shows what client has brought, what he spend, sends new date to sql database using: 'SentTransactionsInfo()' method and ends application.
        /// </summary>

        public void EndOperationsClient()
        {
            Console.Clear ();

            Console.WriteLine($"Thank you for using our vending machine. \n");

                                PurchasesList();

            Console.WriteLine($"You have spend: {moneySpend} $.");
            Console.WriteLine(" ");
            Console.WriteLine("To shut down application press any key. \n");
            Console.ReadKey();
            Environment.Exit (0);
        }
        
       
    }
}

using System;

namespace CarsApp
{
    // A purchase or rental order of one or more cars
    public class Order
    {
        public const int MAX_CARS_IN_ORDER = 10;

        // VFDN-115: order fields
        private int orderId;
        private string customerUsername;
        private int dealershipId;
        private Car[] cars;
        private int carCount;
        private string transactionType; // Purchase, Rental
        private string status;          // Pending, Approved, Rejected, Cancelled
        private DateTime orderDate;

        // VFDN-116: constructor - a new order starts as Pending with no cars
        public Order(int orderId, string customerUsername, int dealershipId, string transactionType)
        {
            this.orderId = orderId;
            this.customerUsername = customerUsername;
            this.dealershipId = dealershipId;
            this.transactionType = transactionType;
            this.cars = new Car[MAX_CARS_IN_ORDER];
            this.carCount = 0;
            this.status = "Pending";
            this.orderDate = DateTime.Now;
        }

        // VFDN-117: Get methods
        public int GetOrderId()
        {
            return orderId;
        }

        public string GetCustomerUsername()
        {
            return customerUsername;
        }

        public int GetDealershipId()
        {
            return dealershipId;
        }

        public string GetTransactionType()
        {
            return transactionType;
        }

        public string GetStatus()
        {
            return status;
        }

        public DateTime GetOrderDate()
        {
            return orderDate;
        }

        public int GetCarCount()
        {
            return carCount;
        }

        public Car GetCar(int index)
        {
            if (index < 0 || index >= carCount)
            {
                return null;
            }
            return cars[index];
        }

        // VFDN-117: status update - accepts only the four allowed values
        public bool SetStatus(string newStatus)
        {
            if (newStatus == "Pending" || newStatus == "Approved" ||
                newStatus == "Rejected" || newStatus == "Cancelled")
            {
                status = newStatus;
                return true;
            }
            return false;
        }

        // VFDN-120: adds a car to the order, returns false when the order is full
        public bool AddCar(Car car)
        {
            if (carCount >= MAX_CARS_IN_ORDER)
            {
                return false;
            }
            cars[carCount] = car;
            carCount++;
            return true;
        }

        // VFDN-120: sum of the prices of all the cars in the order
        public double GetTotalPrice()
        {
            double total = 0;
            for (int i = 0; i < carCount; i++)
            {
                total = total + cars[i].GetPrice();
            }
            return total;
        }
    }
}

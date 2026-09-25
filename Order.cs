using System;

namespace CarsApp
{
    // Design 6.4: a customer's request to buy or rent up to 3 cars from one dealership.
    // Always created as Pending. Never changes the status of a car - that is the system's job.
    public class Order
    {
        public const int MAX_CARS_PER_ORDER = 3;

        private int orderNumber;
        private User customer;
        private Car[] cars;
        private int carCount;
        private string orderType; // Sale or Rental
        private DateTime orderDate;
        private string status;    // Pending / Approved / Rejected / Cancelled

        public Order(int orderNumber, User customer, string orderType)
        {
            this.orderNumber = orderNumber;
            this.customer = customer;
            this.orderType = orderType;
            this.cars = new Car[MAX_CARS_PER_ORDER];
            this.carCount = 0;
            this.orderDate = DateTime.Now;
            this.status = CarDealerShipSystem.ORDER_PENDING;
        }

        public int GetOrderNumber()
        {
            return orderNumber;
        }

        public User GetCustomer()
        {
            return customer;
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

        // Adds a car while building the order. Returns false when the order is full.
        public bool AddCar(Car car)
        {
            if (car == null || carCount >= MAX_CARS_PER_ORDER)
            {
                return false;
            }
            cars[carCount] = car;
            carCount++;
            return true;
        }

        // All the cars in an order belong to the same dealership, so the first car decides
        public CarDealership GetDealership()
        {
            if (carCount == 0)
            {
                return null;
            }
            return cars[0].GetDealership();
        }

        public string GetOrderType()
        {
            return orderType;
        }

        public DateTime GetOrderDate()
        {
            return orderDate;
        }

        public double GetTotalPrice()
        {
            double total = 0;
            for (int i = 0; i < carCount; i++)
            {
                total = total + cars[i].GetPrice();
            }
            return total;
        }

        public string GetStatus()
        {
            return status;
        }

        public bool IsPending()
        {
            return status == CarDealerShipSystem.ORDER_PENDING;
        }

        public bool BelongsTo(User user)
        {
            return customer == user;
        }

        // ===== Status transitions (only from Pending) =====

        public bool Approve()
        {
            if (!IsPending())
            {
                return false;
            }
            status = CarDealerShipSystem.ORDER_APPROVED;
            return true;
        }

        public bool Reject()
        {
            if (!IsPending())
            {
                return false;
            }
            status = CarDealerShipSystem.ORDER_REJECTED;
            return true;
        }

        public bool Cancel()
        {
            if (!IsPending())
            {
                return false;
            }
            status = CarDealerShipSystem.ORDER_CANCELLED;
            return true;
        }

        // "Order #1025 | 17/08/2026 | Sale | 2 cars | Pending" - no customer name and no total
        public override string ToString()
        {
            return "Order #" + orderNumber + " | " + orderDate.ToString("dd/MM/yyyy") + " | " + orderType
                   + " | " + carCount + " cars | " + status;
        }
    }
}

namespace CarsApp
{
    // Design 6.3: a single vehicle in a dealership's inventory.
    // The status changes only through the four transition methods - there is no public SetStatus.
    public class Car
    {
        private int id;
        private string category;
        private string manufacturer;
        private string model;
        private int year;
        private int mileage;
        private string licenseNumber;
        private double price;
        private string dealType; // Sale / Rental / Both
        private string location;
        private string status;   // Available / Reserved / Sold / Rented
        private CarDealership dealership;

        // Always created as Available
        public Car(int id, string category, string manufacturer, string model, int year, int mileage,
                   string licenseNumber, double price, string dealType, string location, CarDealership dealership)
        {
            this.id = id;
            this.category = category;
            this.manufacturer = manufacturer;
            this.model = model;
            this.year = year;
            this.mileage = mileage;
            this.licenseNumber = licenseNumber;
            this.price = price;
            this.dealType = dealType;
            this.location = location;
            this.dealership = dealership;
            this.status = CarDealerShipSystem.STATUS_AVAILABLE;
        }

        public int GetId()
        {
            return id;
        }

        public string GetCategory()
        {
            return category;
        }

        public string GetManufacturer()
        {
            return manufacturer;
        }

        public string GetModel()
        {
            return model;
        }

        public int GetYear()
        {
            return year;
        }

        public int GetMileage()
        {
            return mileage;
        }

        public string GetLicenseNumber()
        {
            return licenseNumber;
        }

        public double GetPrice()
        {
            return price;
        }

        public string GetDealType()
        {
            return dealType;
        }

        public string GetLocation()
        {
            return location;
        }

        public string GetStatus()
        {
            return status;
        }

        public CarDealership GetDealership()
        {
            return dealership;
        }

        // ===== Setters with validation (return false and keep the old value when invalid) =====

        public bool SetManufacturer(string manufacturer)
        {
            if (CarDealerShipSystem.IsBlank(manufacturer))
            {
                return false;
            }
            this.manufacturer = manufacturer;
            return true;
        }

        public bool SetModel(string model)
        {
            if (CarDealerShipSystem.IsBlank(model))
            {
                return false;
            }
            this.model = model;
            return true;
        }

        public bool SetYear(int year)
        {
            if (!CarDealerShipSystem.IsValidYear(year))
            {
                return false;
            }
            this.year = year;
            return true;
        }

        public bool SetMileage(int mileage)
        {
            if (mileage < 0)
            {
                return false;
            }
            this.mileage = mileage;
            return true;
        }

        public bool SetCategory(string category)
        {
            if (CarDealerShipSystem.IsBlank(category))
            {
                return false;
            }
            this.category = category;
            return true;
        }

        public bool SetLocation(string location)
        {
            if (CarDealerShipSystem.IsBlank(location))
            {
                return false;
            }
            this.location = location;
            return true;
        }

        public bool SetPrice(double price)
        {
            if (price <= 0)
            {
                return false;
            }
            this.price = price;
            return true;
        }

        public bool IsAvailable()
        {
            return status == CarDealerShipSystem.STATUS_AVAILABLE;
        }

        // A car with dealType Both supports both Sale and Rental
        public bool SupportsDealType(string type)
        {
            return dealType == type || dealType == CarDealerShipSystem.DEAL_BOTH;
        }

        // ===== Status transitions (each one checks the source status) =====

        // Available -> Reserved
        public bool MarkAsReserved()
        {
            if (status != CarDealerShipSystem.STATUS_AVAILABLE)
            {
                return false;
            }
            status = CarDealerShipSystem.STATUS_RESERVED;
            return true;
        }

        // Reserved -> Sold
        public bool MarkAsSold()
        {
            if (status != CarDealerShipSystem.STATUS_RESERVED)
            {
                return false;
            }
            status = CarDealerShipSystem.STATUS_SOLD;
            return true;
        }

        // Reserved -> Rented
        public bool MarkAsRented()
        {
            if (status != CarDealerShipSystem.STATUS_RESERVED)
            {
                return false;
            }
            status = CarDealerShipSystem.STATUS_RENTED;
            return true;
        }

        // Reserved -> Available. Sold and Rented are final.
        public bool MakeAvailable()
        {
            if (status != CarDealerShipSystem.STATUS_RESERVED)
            {
                return false;
            }
            status = CarDealerShipSystem.STATUS_AVAILABLE;
            return true;
        }

        // "Car #14 | SUV | Toyota Corolla (2024) | 12000 KM | 145000 NIS | Sale | Haifa | Available"
        public override string ToString()
        {
            return "Car #" + id + " | " + category + " | " + manufacturer + " " + model + " (" + year + ") | "
                   + mileage + " KM | " + price + " NIS | " + dealType + " | " + location + " | " + status;
        }
    }
}

using System.Collections.Generic;

namespace HamaraCommerce.Data.Catalog
{
    public class DepartmentDef
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-box";
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public List<SubcategoryDef> Subcategories { get; set; } = new();
    }

    public class SubcategoryDef
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-tag";
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public int? ExistingId { get; set; } // If maps to an existing category
    }

    public static class MarketplaceTaxonomy
    {
        public static List<DepartmentDef> GetDepartments()
        {
            return new List<DepartmentDef>
            {
                new DepartmentDef
                {
                    Name = "Electronic Devices",
                    Slug = "electronic-devices",
                    Icon = "fa-mobile-screen",
                    Description = "Flagship smartphones, tablets, high-performance laptops & gaming consoles",
                    DisplayOrder = 1,
                    Subcategories = new List<SubcategoryDef>
                    {
                        new SubcategoryDef { Name = "Mobile Phones", Slug = "mobile-phones", Icon = "fa-mobile-screen-button", Description = "5G flagship smartphones, mid-rangers and budget phones", DisplayOrder = 1, ExistingId = 2 },
                        new SubcategoryDef { Name = "Tablets", Slug = "tablets", Icon = "fa-tablet-screen-button", Description = "iPads, Android tablets and creative stylus devices", DisplayOrder = 2 },
                        new SubcategoryDef { Name = "Laptops", Slug = "laptops-computers", Icon = "fa-laptop", Description = "Ultrabooks, business laptops and gaming powerhouses", DisplayOrder = 3, ExistingId = 3 },
                        new SubcategoryDef { Name = "Desktop Computers", Slug = "desktop-computers", Icon = "fa-desktop", Description = "All-in-One PCs, workstations and custom desktop rigs", DisplayOrder = 4 },
                        new SubcategoryDef { Name = "Gaming Consoles", Slug = "gaming-consoles", Icon = "fa-gamepad", Description = "PlayStation 5, Xbox Series X, Nintendo Switch and handhelds", DisplayOrder = 5 },
                        new SubcategoryDef { Name = "Cameras", Slug = "cameras", Icon = "fa-camera", Description = "Mirrorless cameras, vlogging cameras and action cams", DisplayOrder = 6 }
                    }
                },
                new DepartmentDef
                {
                    Name = "Electronic Accessories",
                    Slug = "electronic-accessories",
                    Icon = "fa-headphones",
                    Description = "Audio gear, chargers, cables, PC peripherals and smart wearables",
                    DisplayOrder = 2,
                    Subcategories = new List<SubcategoryDef>
                    {
                        new SubcategoryDef { Name = "Mobile Accessories", Slug = "mobile-accessories", Icon = "fa-battery-half", Description = "Fast chargers, power banks, cases and protectors", DisplayOrder = 1, ExistingId = 15 },
                        new SubcategoryDef { Name = "Computer Accessories", Slug = "computer-accessories", Icon = "fa-keyboard", Description = "Mechanical keyboards, ergonomic mice and monitor arms", DisplayOrder = 2, ExistingId = 16 },
                        new SubcategoryDef { Name = "Audio", Slug = "audio", Icon = "fa-headphones-simple", Description = "Wireless earbuds, noise-cancelling headphones and speakers", DisplayOrder = 3 },
                        new SubcategoryDef { Name = "Wearables", Slug = "wearables", Icon = "fa-clock", Description = "Smartwatches, fitness trackers and smart bands", DisplayOrder = 4 },
                        new SubcategoryDef { Name = "Storage Devices", Slug = "storage-devices", Icon = "fa-hard-drive", Description = "Portable SSDs, NVMe drives, USB flash drives and memory cards", DisplayOrder = 5 },
                        new SubcategoryDef { Name = "Printers", Slug = "printers", Icon = "fa-print", Description = "EcoTank all-in-one printers, laser printers and photo printers", DisplayOrder = 6 },
                        new SubcategoryDef { Name = "Network Equipment", Slug = "network-equipment", Icon = "fa-network-wired", Description = "Wi-Fi 6 routers, mesh systems and gigabit ethernet switches", DisplayOrder = 7 }
                    }
                },
                new DepartmentDef
                {
                    Name = "TV & Home Appliances",
                    Slug = "tv-home-appliances",
                    Icon = "fa-tv",
                    Description = "4K Smart TVs, inverter air conditioners, refrigerators and kitchen tech",
                    DisplayOrder = 3,
                    Subcategories = new List<SubcategoryDef>
                    {
                        new SubcategoryDef { Name = "Televisions", Slug = "tvs-entertainment", Icon = "fa-tv", Description = "4K QLED, OLED, Google TVs and soundbars", DisplayOrder = 1, ExistingId = 1 },
                        new SubcategoryDef { Name = "Refrigerators", Slug = "refrigerators", Icon = "fa-snowflake", Description = "Inverter direct-cool, frost-free and deep freezers", DisplayOrder = 2 },
                        new SubcategoryDef { Name = "Air Conditioners", Slug = "air-conditioners", Icon = "fa-wind", Description = "Inverter split ACs, heat & cool series and floor standing ACs", DisplayOrder = 3 },
                        new SubcategoryDef { Name = "Washing Machines", Slug = "washing-machines", Icon = "fa-soap", Description = "Front-load, top-load fully automatic and twin-tub washers", DisplayOrder = 4 },
                        new SubcategoryDef { Name = "Kitchen Appliances", Slug = "kitchen-appliances", Icon = "fa-blender", Description = "Digital air fryers, microwave ovens, blenders and food processors", DisplayOrder = 5, ExistingId = 8 },
                        new SubcategoryDef { Name = "Small Home Appliances", Slug = "home-appliances", Icon = "fa-plug", Description = "Garment steamers, dry & steam irons and vacuum cleaners", DisplayOrder = 6, ExistingId = 14 }
                    }
                },
                new DepartmentDef
                {
                    Name = "Health & Beauty",
                    Slug = "health-beauty",
                    Icon = "fa-spa",
                    Description = "Dermatologist-approved skincare, fragrances, hair care and wellness monitors",
                    DisplayOrder = 4,
                    Subcategories = new List<SubcategoryDef>
                    {
                        new SubcategoryDef { Name = "Skincare", Slug = "beauty-personal-care", Icon = "fa-pump-soap", Description = "Facial cleansers, serums, sunscreens and moisturizers", DisplayOrder = 1, ExistingId = 7 },
                        new SubcategoryDef { Name = "Makeup", Slug = "makeup", Icon = "fa-wand-magic-sparkles", Description = "Foundations, concealers, lipsticks and eye palettes", DisplayOrder = 2 },
                        new SubcategoryDef { Name = "Hair Care", Slug = "hair-care", Icon = "fa-scissors", Description = "Herbal shampoos, conditioners, hair serums and color", DisplayOrder = 3 },
                        new SubcategoryDef { Name = "Fragrances", Slug = "fragrances", Icon = "fa-spray-can-sparkles", Description = "Pakistani & designer perfumes, attars and body mists", DisplayOrder = 4 },
                        new SubcategoryDef { Name = "Personal Care", Slug = "personal-care", Icon = "fa-hand-holding-heart", Description = "Body wash, soaps, oral hygiene and hair removal", DisplayOrder = 5 },
                        new SubcategoryDef { Name = "Health Equipment", Slug = "health-wellness", Icon = "fa-heart-pulse", Description = "Digital BP monitors, glucometers, oximeters and nebulizers", DisplayOrder = 6, ExistingId = 18 }
                    }
                },
                new DepartmentDef
                {
                    Name = "Babies & Toys",
                    Slug = "babies-toys",
                    Icon = "fa-baby-carriage",
                    Description = "Newborn baby care, diapers, safe feeding bottles, educational games and toys",
                    DisplayOrder = 5,
                    Subcategories = new List<SubcategoryDef>
                    {
                        new SubcategoryDef { Name = "Baby Clothing", Slug = "baby-clothing", Icon = "fa-shirt", Description = "Organic cotton rompers, sleepsuits and party frocks", DisplayOrder = 1 },
                        new SubcategoryDef { Name = "Feeding", Slug = "feeding", Icon = "fa-bottle-water", Description = "Anti-colic feeding bottles, breast pumps, sterilizers and bowls", DisplayOrder = 2 },
                        new SubcategoryDef { Name = "Diapers", Slug = "diapers", Icon = "fa-baby", Description = "Tape & pant diapers, sensitive wipes and changing mats", DisplayOrder = 3 },
                        new SubcategoryDef { Name = "Baby Gear", Slug = "kids-babies", Icon = "fa-car", Description = "Strollers, high chairs, rocker bouncers and car seats", DisplayOrder = 4, ExistingId = 20 },
                        new SubcategoryDef { Name = "Toys", Slug = "toys-games", Icon = "fa-puzzle-piece", Description = "Diecast model cars, Lego sets, action figures and dolls", DisplayOrder = 5, ExistingId = 9 },
                        new SubcategoryDef { Name = "Learning Games", Slug = "learning-games", Icon = "fa-brain", Description = "Montessori shape sorters, wooden puzzles and STEM kits", DisplayOrder = 6 }
                    }
                },
                new DepartmentDef
                {
                    Name = "Groceries & Pets",
                    Slug = "groceries-pets",
                    Icon = "fa-basket-shopping",
                    Description = "Daily kitchen staples, premium basmati rice, tea, snacks and pet nutrition",
                    DisplayOrder = 6,
                    Subcategories = new List<SubcategoryDef>
                    {
                        new SubcategoryDef { Name = "Beverages", Slug = "grocery-beverages", Icon = "fa-mug-hot", Description = "Danedar black teas, green teas, coffee and syrups", DisplayOrder = 1, ExistingId = 11 },
                        new SubcategoryDef { Name = "Snacks", Slug = "snacks", Icon = "fa-cookie-bite", Description = "Potato crisps, nimco, premium biscuits and chocolates", DisplayOrder = 2 },
                        new SubcategoryDef { Name = "Cooking Essentials", Slug = "cooking-essentials", Icon = "fa-seedling", Description = "Basmati rice, pure ghee, cooking oil, spices and pulses", DisplayOrder = 3 },
                        new SubcategoryDef { Name = "Household Supplies", Slug = "household-supplies", Icon = "fa-broom", Description = "Detergent powder, dishwashing liquid and surface cleaners", DisplayOrder = 4 },
                        new SubcategoryDef { Name = "Pet Food", Slug = "pet-food", Icon = "fa-bone", Description = "Nutritious dry & wet cat food, dog kibble and bird feed", DisplayOrder = 5 },
                        new SubcategoryDef { Name = "Pet Accessories", Slug = "pet-accessories", Icon = "fa-paw", Description = "Litter trays, feeding bowls, collars, leashes and beds", DisplayOrder = 6 }
                    }
                },
                new DepartmentDef
                {
                    Name = "Home & Lifestyle",
                    Slug = "home-lifestyle",
                    Icon = "fa-couch",
                    Description = "Luxury bedding, ergonomic office furniture, cookware, decor and bestselling books",
                    DisplayOrder = 7,
                    Subcategories = new List<SubcategoryDef>
                    {
                        new SubcategoryDef { Name = "Furniture", Slug = "furniture-decor", Icon = "fa-chair", Description = "Mesh office chairs, study desks and coffee tables", DisplayOrder = 1, ExistingId = 10 },
                        new SubcategoryDef { Name = "Home Decor", Slug = "home-decor", Icon = "fa-palette", Description = "Islamic wall art, ceramic vases, lamps and mirrors", DisplayOrder = 2 },
                        new SubcategoryDef { Name = "Bedding", Slug = "home-living", Icon = "fa-bed", Description = "Pure cotton bedsheets, comforter sets and zero-twist towels", DisplayOrder = 3, ExistingId = 19 },
                        new SubcategoryDef { Name = "Kitchen & Dining", Slug = "kitchen-dining", Icon = "fa-utensils", Description = "Non-stick cookware, dinner sets, cutlery and airtight containers", DisplayOrder = 4 },
                        new SubcategoryDef { Name = "Lighting", Slug = "lighting", Icon = "fa-lightbulb", Description = "LED bulbs, smart desk lamps and emergency lights", DisplayOrder = 5 },
                        new SubcategoryDef { Name = "Tools & DIY", Slug = "tools-diy", Icon = "fa-wrench", Description = "Cordless drills, toolboxes, measuring tapes and multimeters", DisplayOrder = 6 },
                        new SubcategoryDef { Name = "Books & Stationery", Slug = "books-stationery", Icon = "fa-book-open", Description = "Urdu literature, self-help bestsellers, diaries and pens", DisplayOrder = 7, ExistingId = 13 }
                    }
                },
                new DepartmentDef
                {
                    Name = "Women’s Fashion",
                    Slug = "womens-fashion-dept",
                    Icon = "fa-person-dress",
                    Description = "Authentic Pakistani designer lawn, pret wear, western attire, shoes and bags",
                    DisplayOrder = 8,
                    Subcategories = new List<SubcategoryDef>
                    {
                        new SubcategoryDef { Name = "Eastern Wear", Slug = "womens-fashion", Icon = "fa-shirt", Description = "Embroidered 3-piece lawn suits, stitched pret and kurtis", DisplayOrder = 1, ExistingId = 17 },
                        new SubcategoryDef { Name = "Western Wear", Slug = "womens-western-wear", Icon = "fa-vest", Description = "Button-down shirts, hoodies, cardigans and denim", DisplayOrder = 2 },
                        new SubcategoryDef { Name = "Shoes", Slug = "womens-shoes", Icon = "fa-shoe-prints", Description = "Formal block heels, khussas, sandals and comfort flats", DisplayOrder = 3 },
                        new SubcategoryDef { Name = "Bags", Slug = "womens-bags", Icon = "fa-bag-shopping", Description = "Crossbody bags, structured totes and casual backpacks", DisplayOrder = 4 },
                        new SubcategoryDef { Name = "Jewellery", Slug = "womens-jewellery", Icon = "fa-gem", Description = "Zirconia pendant sets, kundan jhumkas and bangles", DisplayOrder = 5 },
                        new SubcategoryDef { Name = "Accessories", Slug = "womens-accessories", Icon = "fa-glasses", Description = "Silk hijabs, pashmina shawls and sunglasses", DisplayOrder = 6 }
                    }
                },
                new DepartmentDef
                {
                    Name = "Men’s Fashion",
                    Slug = "mens-fashion-dept",
                    Icon = "fa-shirt",
                    Description = "Traditional unstitched fabrics, kurtas, formal shirts, jeans and footwear",
                    DisplayOrder = 9,
                    Subcategories = new List<SubcategoryDef>
                    {
                        new SubcategoryDef { Name = "Eastern Wear", Slug = "mens-fashion", Icon = "fa-shirt", Description = "Wash & wear unstitched suits, designer kurtas and latha", DisplayOrder = 1, ExistingId = 4 },
                        new SubcategoryDef { Name = "Western Wear", Slug = "mens-western-wear", Icon = "fa-tshirt", Description = "Crewneck tees, Oxford casual shirts and stretch denim", DisplayOrder = 2 },
                        new SubcategoryDef { Name = "Shoes", Slug = "shoes-footwear", Icon = "fa-shoe-prints", Description = "Peshawari chappals, formal derbies and running sneakers", DisplayOrder = 3, ExistingId = 5 },
                        new SubcategoryDef { Name = "Watches", Slug = "mens-watches-cat", Icon = "fa-clock", Description = "Stainless steel chronographs and military quartz watches", DisplayOrder = 4 },
                        new SubcategoryDef { Name = "Bags", Slug = "mens-bags", Icon = "fa-briefcase", Description = "Leather executive briefcases and travel duffle bags", DisplayOrder = 5 },
                        new SubcategoryDef { Name = "Accessories", Slug = "mens-accessories", Icon = "fa-wallet", Description = "Genuine cow leather belts, ties and cufflink sets", DisplayOrder = 6 }
                    }
                },
                new DepartmentDef
                {
                    Name = "Watches, Bags & Jewellery",
                    Slug = "watches-bags-jewellery",
                    Icon = "fa-gem",
                    Description = "Authentic chronographs, smart timepieces, designer handbags and fine jewellery",
                    DisplayOrder = 10,
                    Subcategories = new List<SubcategoryDef>
                    {
                        new SubcategoryDef { Name = "Men’s Watches", Slug = "mens-watches", Icon = "fa-clock", Description = "G-Shock shock-resistant, automatic and chronograph timepieces", DisplayOrder = 1 },
                        new SubcategoryDef { Name = "Women’s Watches", Slug = "womens-watches", Icon = "fa-clock", Description = "Rose gold mesh watches, ceramic and diamond-accent dials", DisplayOrder = 2 },
                        new SubcategoryDef { Name = "Smart Watches", Slug = "smart-watches", Icon = "fa-stopwatch", Description = "AMOLED Bluetooth calling watches and GPS sports fitness watches", DisplayOrder = 3 },
                        new SubcategoryDef { Name = "Handbags", Slug = "handbags", Icon = "fa-bag-shopping", Description = "Logo satchels, push-lock crossbodies and designer shoulder bags", DisplayOrder = 4 },
                        new SubcategoryDef { Name = "Wallets", Slug = "wallets", Icon = "fa-wallet", Description = "RFID-blocking bifold leather wallets and zip travel organizers", DisplayOrder = 5 },
                        new SubcategoryDef { Name = "Jewellery", Slug = "watches-jewellery", Icon = "fa-ring", Description = "Sterling silver rings, bridal necklace sets and gold plated bangles", DisplayOrder = 6, ExistingId = 6 }
                    }
                },
                new DepartmentDef
                {
                    Name = "Sports & Outdoors",
                    Slug = "sports-outdoors",
                    Icon = "fa-baseball-bat-ball",
                    Description = "English willow cricket gear, fitness treadmills, camping equipment and cycles",
                    DisplayOrder = 11,
                    Subcategories = new List<SubcategoryDef>
                    {
                        new SubcategoryDef { Name = "Fitness Equipment", Slug = "fitness-equipment", Icon = "fa-dumbbell", Description = "Motorized treadmills, rubber hex dumbbells and hand grips", DisplayOrder = 1 },
                        new SubcategoryDef { Name = "Sportswear", Slug = "sportswear", Icon = "fa-person-running", Description = "Cricket white kits, training tracksuits and dri-fit jerseys", DisplayOrder = 2 },
                        new SubcategoryDef { Name = "Team Sports", Slug = "sports-fitness", Icon = "fa-futbol", Description = "Grade 1 cricket bats, leather balls, batting pads and footballs", DisplayOrder = 3, ExistingId = 12 },
                        new SubcategoryDef { Name = "Cycling", Slug = "cycling", Icon = "fa-bicycle", Description = "Shimano gear mountain bikes, road bicycles and bike pumps", DisplayOrder = 4 },
                        new SubcategoryDef { Name = "Camping", Slug = "camping", Icon = "fa-campground", Description = "Waterproof instant cabin tents, sleeping bags and camping stoves", DisplayOrder = 5 },
                        new SubcategoryDef { Name = "Outdoor Recreation", Slug = "outdoor-recreation", Icon = "fa-mountain", Description = "Binoculars, Swiss pocket knives and vacuum water bottles", DisplayOrder = 6 }
                    }
                },
                new DepartmentDef
                {
                    Name = "Automotive & Motorbike",
                    Slug = "automotive-motorbike",
                    Icon = "fa-motorcycle",
                    Description = "Car electronics, dash cams, synthetic engine oils, helmets and riding gear",
                    DisplayOrder = 12,
                    Subcategories = new List<SubcategoryDef>
                    {
                        new SubcategoryDef { Name = "Car Accessories", Slug = "car-accessories", Icon = "fa-car", Description = "12V tyre inflators, waterproof body covers and rubber floor mats", DisplayOrder = 1 },
                        new SubcategoryDef { Name = "Car Electronics", Slug = "car-electronics", Icon = "fa-car-battery", Description = "Touchscreen multimedia head units, dash cams and car speakers", DisplayOrder = 2 },
                        new SubcategoryDef { Name = "Car Care", Slug = "car-care", Icon = "fa-spray-can", Description = "Carnauba paste wax, car wash shampoo and microfiber towels", DisplayOrder = 3 },
                        new SubcategoryDef { Name = "Motorcycle Accessories", Slug = "motorcycle-accessories", Icon = "fa-helmet-safety", Description = "ISI certified flip-up helmets, riding gloves and bike disc locks", DisplayOrder = 4 },
                        new SubcategoryDef { Name = "Oils & Fluids", Slug = "oils-fluids", Icon = "fa-oil-can", Description = "Synthetic engine oils 5W-30/10W-40, brake fluids and radiator coolants", DisplayOrder = 5 },
                        new SubcategoryDef { Name = "Tools", Slug = "automotive-tools", Icon = "fa-screwdriver-wrench", Description = "Hydraulic floor jacks, rim wrenches, jumper cables and 12V vacuums", DisplayOrder = 6 }
                    }
                }
            };
        }
    }
}

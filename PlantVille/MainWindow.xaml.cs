using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlantVille
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Seed> seed_inventory;
        private List<Plant> garden = new List<Plant>();
        private List<Plant> inventory = new List<Plant>();
        public int money = 100;
        public int land_plot = 15;

        public MainWindow()
        {
            InitializeComponent();

            seed_inventory = new List<Seed>()
        {
          new Seed("strawberry", 2, 3, new TimeSpan(0, 0, 30)),
          new Seed("spinach", 5, 10, new TimeSpan(0, 1, 0)),
          new Seed("pears", 3, 6, new TimeSpan(0, 3, 0))
        };
            load_data();
            load_emporium();
            load_garden();
        }
        private void load_data()
        {
            if (!File.Exists("data.txt"))
                return;
            using (StreamReader streamReader = new StreamReader("data.txt"))
            {
                while (!streamReader.EndOfStream)
                {
                    Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(streamReader.ReadLine());
                    garden = JsonConvert.DeserializeObject<List<Plant>>(dictionary["garden"].ToString());
                    inventory = JsonConvert.DeserializeObject<List<Plant>>(dictionary["inventory"].ToString());
                    money = Convert.ToInt32(dictionary["money"]);
                }   
            }
        }

        private void btn_seeds_Click(object sender, RoutedEventArgs e)
        {
            grid_garden.Visibility = Visibility.Hidden;
            grid_emporium.Visibility = Visibility.Visible;
            grid_inventory.Visibility = Visibility.Hidden;
            load_emporium();
        }

        private void load_garden()
        {
            lb_garden.Items.Clear();
            if (garden.Count < 1)
            {
                lb_garden.Items.Add("No plants in garden.");
            }
            else
            {
                foreach (Plant plant in garden)
                    lb_garden.Items.Add(string.Format("{0} ({1})", plant.Seed.Name, this.harvestTimeLeftMessage(plant)));
            }
            update_status();
        }

        private string harvestTimeLeftMessage(Plant plant)
        {
            int num = MainWindow.HarvestTimeLeft(plant);
            if (num > 0)
                return string.Format("{0} seconds left", num);
            if (num > -150)
                return "harvest";
            plant.IsSpoiled = true;
            return "spoiled";
        }

        private static int HarvestTimeLeft(Plant plant) => plant.HarvestTime.Add(plant.Seed.HarvestDuration).Subtract(DateTime.Now).Seconds;

        private void load_emporium()
        {
            lb_emporium.Items.Clear();
            foreach (Seed seed in seed_inventory)
                lb_emporium.Items.Add(string.Format("{0} ${1}", seed.Name, seed.SeedPrice));
            update_status();
        }

        private void update_status() => lbl_status.Content = string.Format("Money: ${0}\nLand: {1}", money, (land_plot - garden.Count));

        private void btn_garden_Click(object sender, RoutedEventArgs e)
        {
           grid_garden.Visibility = Visibility.Visible;
           grid_emporium.Visibility = Visibility.Hidden;
           grid_inventory.Visibility = Visibility.Hidden;
           load_garden();
        }

        private void lb_emporium_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Seed seed = seed_inventory[lb_emporium.SelectedIndex];
       
            if (seed.SeedPrice > money)
               MessageBox.Show("You don't have enough money."); 
            else if (land_plot - garden.Count < 1)
            {
                MessageBox.Show("You dont have enough land to plant another crop."); 
            }
            else
            {
                money -= seed.SeedPrice;
                garden.Add(new Plant(seed));
                MessageBox.Show($"You purchased {seed.Name}"); 
                update_status();
            }
        }

        private void btn_harvest_Click(object sender, RoutedEventArgs e)
        {
            int num1 = 0;
            foreach (Plant plant in this.garden.ToList<Plant>())
            {
                if (MainWindow.HarvestTimeLeft(plant) <= 0 && !plant.IsSpoiled)
                {
                    inventory.Add(plant);
                    garden.Remove(plant);
                    ++num1;
                }
                else if (plant.IsSpoiled)
                    garden.Remove(plant);
            }
            if (num1 > 0)
            {
                MessageBox.Show(string.Format("Harvested {0} plants.", num1));
            }
            else
            {
                MessageBox.Show("Nothing to harvest.");
            }
            this.load_garden();
        }

        private void btn_inventory_Click(object sender, RoutedEventArgs e)
        {
            grid_emporium.Visibility = Visibility.Hidden;
            grid_garden.Visibility = Visibility.Hidden;
            grid_inventory.Visibility = Visibility.Visible;
            load_inventory();
        }

        private void load_inventory()
        {
            lb_inventory.Items.Clear();
            foreach (Plant plant in inventory)
                lb_inventory.Items.Add(string.Format("{0} ${1}", plant.Seed.Name, plant.Seed.SeedPrice));
            if (inventory.Count == 0)
                lb_inventory.Items.Add("No fruits or vegetables harvested.");
            this.update_status();
        }

        private void btn_sell_Click(object sender, RoutedEventArgs e)
        {
            if (inventory.Count == 0 && MessageBox.Show("Are you sure you want to go to the farmer's market without any inventory?", "Lose money for no reason?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            int num1 = -10;
            foreach (Plant plant in inventory)
                num1 += plant.Seed.HarvestPrice;
            money += num1;
            inventory.Clear();
            MessageBox.Show(string.Format("Cleared inventory. Made ${0}", num1));
            load_inventory();
        }

        private void Window_Closing(object sender, CancelEventArgs e) => File.WriteAllText("data.txt", JsonConvert.SerializeObject(new Dictionary<string, object>()
    {
      {
        "garden",
         garden
      },
      {
        "inventory",
        inventory
      },
      {
        "money",
         money
      }
    }));

        private void lb_garden_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int selectedIndex = lb_garden.SelectedIndex;
            Plant plant = garden[selectedIndex];
            MessageBox.Show(string.Format("{0} harvested.", garden[selectedIndex].Seed.Name));
            inventory.Add(plant);
            garden.Remove(plant);
            load_garden();
        }

    }
}

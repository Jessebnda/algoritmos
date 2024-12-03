using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using primeravancefinal.Properties;
using System.Globalization;
using System.Threading;




namespace primeravancefinal
{

    public partial class Form1 : Form
    {

        // Define city coordinates
        private Dictionary<string, Point> cityCoordinates = new Dictionary<string, Point>
        {
            { "Indigo Plateau", new Point(52, 106) },
            { "Victory Road", new Point(57, 144) },      
            { "Lavender Town", new Point(328, 167) },
            { "Pewter City", new Point(94, 125) },
            { "Viridian Forest", new Point(92,162 ) },
            { "Viridian City", new Point(91, 226) },
            { "Pallet Town", new Point(90, 287) },
            { "Cinnabar Island", new Point(92, 365) },
            { "Mt.Moon", new Point(171, 108) },
            { "Celadon City", new Point(190, 167) },
            { "Fuschia City", new Point(209, 327) },
            { "Cerulean City", new Point(250,107 ) },
            { "Saffron City", new Point(250, 167) },
            { "Vermillion City", new Point(250, 247) },
            { "Bill's house", new Point(289, 69) },
            { "Rock Tunnel", new Point(329, 128) },
            { "SeaFoam Island", new Point(157, 366) },
        };

        //Dictionary<string, List<(string, int)>> cityGraph = new Dictionary<string, List<(string, int)>>();


        // Initialize city graph
        private Dictionary<string, List<(string city, int distance, int time, int cost)>> cityGraph =
            new Dictionary<string, List<(string city, int distance, int time, int cost)>>();


        // Selected options for transport mode and search criteria
        private string selectedTransportMode = "Auto rentado"; // Default transport mode
        private string selectedSearchCriterion = "Distancia";  // Default search criterion

        // Define a tolerance for proximity checks
        private const int Tolerance = 20;

        public Form1()
        {
            InitializeComponent();

            //MouseClick event for the PictureBox
            this.pictureBox1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseClick);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Populate Ciudad de Inicio ComboBox
            InitializeCityGraph(); // Initialize graph 
            PopulateComboBox3();
            PopulateComboBox4();
            PopulateRouteComboBoxes();
        }

        //  una prueba tranqui con valores por default
        private void InitializeCityGraph()
        {
            // Ensure every city has an entry in cityGraph
            foreach (var city in cityCoordinates.Keys)
            {
                if (!cityGraph.ContainsKey(city))
                {
                    cityGraph[city] = new List<(string city, int distance, int time, int cost)>();
                }
            }

            // Initialize graph with calculated distance, time, and cost
            foreach (var cityA in cityCoordinates.Keys)
            {
                foreach (var cityB in cityCoordinates.Keys)
                {
                    if (cityA != cityB)
                    {
                        // Calculate straight-line distance
                        Point pointA = cityCoordinates[cityA];
                        Point pointB = cityCoordinates[cityB];
                        int distance = (int)Math.Sqrt(Math.Pow(pointB.X - pointA.X, 2) + Math.Pow(pointB.Y - pointA.Y, 2));

                        // Calculate time and cost for auto and public transport
                        int autoTime = distance / 60;  // Assume 60 km/h for auto
                        int autoCost = distance * 5;   // $5 per km for auto
                        int publicTime = distance / 30;  // Assume 30 km/h for public transport
                        int publicCost = distance * 2;   // $2 per km for public transport

                        // Add the connection to the graph
                        cityGraph[cityA].Add((cityB, distance, autoTime, autoCost));
                        cityGraph[cityB].Add((cityA, distance, publicTime, publicCost)); // Bidirectional
                    }
                }
            }
        }

        // Dijkstra's Algorithm for finding the shortest path
        private (List<string> path, int distance) FindShortestPath(string startCity, string endCity)
        {
            // Priority queue for Dijkstra's Algorithm
            var priorityQueue = new SortedSet<(int cost, string city)>();
            var distances = new Dictionary<string, int>();
            var previousCities = new Dictionary<string, string>();

            // Initialize distances to infinity
            foreach (var city in cityGraph.Keys)
            {
                distances[city] = int.MaxValue;
            }

            distances[startCity] = 0;
            priorityQueue.Add((0, startCity));

            while (priorityQueue.Count > 0)
            {
                var current = priorityQueue.Min;
                var currentCost = current.cost;
                var currentCity = current.city;

                priorityQueue.Remove(priorityQueue.Min);

                if (currentCity == endCity)
                {
                    break; // Destination reached
                }

                foreach (var neighborTuple in cityGraph[currentCity])
                {
                    var neighbor = neighborTuple.city;
                    var distance = neighborTuple.distance;
                    var time = neighborTuple.time;
                    var cost = neighborTuple.cost;

                    // Decide which parameter to optimize
                    int newCost = currentCost;
                    if (selectedSearchCriterion == "Distancia")
                    {
                        newCost += distance;
                    }
                    else if (selectedSearchCriterion == "Tiempo")
                    {
                        newCost += time;
                    }
                    else if (selectedSearchCriterion == "Costo")
                    {
                        newCost += cost;
                    }

                    if (newCost < distances[neighbor])
                    {
                        priorityQueue.Remove((distances[neighbor], neighbor)); // Remove old value
                        distances[neighbor] = newCost;
                        previousCities[neighbor] = currentCity;
                        priorityQueue.Add((newCost, neighbor));
                    }
                }
            }

            // Reconstruct the path
            var path = new List<string>();
            string currentPathCity = endCity;

            while (currentPathCity != null && previousCities.ContainsKey(currentPathCity))
            {
                path.Add(currentPathCity);
                currentPathCity = previousCities[currentPathCity];
            }

            path.Add(startCity);
            path.Reverse();

            return (path, distances[endCity]);
        }
        // end Dijkstra 





        // start busqueda/informacion tab
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            // Check which mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                PlaceMarker(e.Location, "Start");
            }
            else if (e.Button == MouseButtons.Right)
            {
                PlaceMarker(e.Location, "Destination");
            }
        }
        private void PlaceMarker(Point location, string markerType)
        {
            PictureBox marker = new PictureBox
            {
                Size = new Size(10, 10),
                BackColor = markerType == "Start" ? Color.Green : Color.Red,
                Location = new Point(location.X - 5, location.Y - 5), // Center  marker
                BorderStyle = BorderStyle.FixedSingle
            };

            // Add the marker to PictureBox
            pictureBox1.Controls.Add(marker);

            // Checks if the marker is near a city
            string nearestCity = GetNearestCity(location);
            if (nearestCity != null)
            {
                MessageBox.Show($"{markerType} marker placed near {nearestCity} at ({location.X}, {location.Y})", "Marker Added");
            }
            else
            {
                MessageBox.Show($"{markerType} marker placed at ({location.X}, {location.Y}) with no nearby city", "Marker Added");
            }
        }

        //  GetNearestCity method aqui
        private string GetNearestCity(Point markerLocation)
        {
            foreach (var city in cityCoordinates)
            {
                double distance = Math.Sqrt(
                    Math.Pow(markerLocation.X - city.Value.X, 2) +
                    Math.Pow(markerLocation.Y - city.Value.Y, 2)
                );

                if (distance <= Tolerance)
                {
                    return city.Key; // Return city name
                }
            }

            return null; // No city found within tolerance

        }//end busqueda tab




        //start ciudad tab
        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void label18_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {

        }

        //boton 4 altas de ciudad
        private void button4_Click(object sender, EventArgs e)
        {
            // Get input values
            string cityName = textBox7.Text.Trim();
            int xCoordinate = (int)numericUpDown1.Value;
            int yCoordinate = (int)numericUpDown2.Value;

            // Validate inputs
            if (string.IsNullOrEmpty(cityName))
            {
                MessageBox.Show("City name cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cityCoordinates.ContainsKey(cityName))
            {
                MessageBox.Show("City name already exists. Please choose a different name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Add the city to the dictionary
            cityCoordinates.Add(cityName, new Point(xCoordinate, yCoordinate));

            // Create  dot (city marker)
            PictureBox cityDot = new PictureBox
            {
                Size = new Size(10, 10), 
                BackColor = Color.Brown, 
                Location = new Point(xCoordinate - 5, yCoordinate - 5), // Center the dot at the coordinates
                BorderStyle = BorderStyle.FixedSingle, 
                Tag = cityName // Associate the city name with the dot
            };

           //makes dot ciruclar
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(0, 0, cityDot.Width, cityDot.Height);
            cityDot.Region = new Region(path);

            // Add the dot to map (PictureBox)
            pictureBox1.Controls.Add(cityDot);

            // Create label (nombre ciudad)
            Label cityLabel = new Label
            {
                Text = cityName, 
                AutoSize = true, 
                ForeColor = Color.Black, 
                BackColor = Color.Transparent, 
                Location = new Point(xCoordinate + 5, yCoordinate - 8), // Position the label near the dot
                Font = new Font("Arial", 8, FontStyle.Regular), 
                Tag = cityName // Associate the city name with the label
            };

            // Add label to the map (PictureBox)
            pictureBox1.Controls.Add(cityLabel);

            // prints to user the info 
            MessageBox.Show($"City '{cityName}' added successfully at coordinates ({xCoordinate}, {yCoordinate})!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Clear the input fields after adding
            textBox7.Clear();
            numericUpDown1.Value = 0;
            numericUpDown2.Value = 0;

            // Populate Ciudad de Inicio ComboBox
            PopulateComboBox3();
            PopulateComboBox4();

        }//end alta ciudad button 4



        //start baja de ciudad button
        private void button5_Click(object sender, EventArgs e)
        {
            // Get the city name from the input field
            string cityName = textBox7.Text.Trim();

            // Validate input
            if (string.IsNullOrEmpty(cityName))
            {
                MessageBox.Show("Please enter the name of the city to remove.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if the city exists
            if (!cityCoordinates.ContainsKey(cityName))
            {
                MessageBox.Show($"City '{cityName}' does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Remove the city from the dictionary
            cityCoordinates.Remove(cityName);

            // Remove the visual elements (dot and label) from the map
            List<Control> controlsToRemove = new List<Control>();

            foreach (Control control in pictureBox1.Controls)
            {
                if (control.Tag != null && control.Tag.ToString() == cityName)
                {
                    controlsToRemove.Add(control); // Add the matching control to the list
                }
            }

            // Remove all matching controls from the PictureBox
            foreach (Control control in controlsToRemove)
            {
                pictureBox1.Controls.Remove(control);
            }

            // Prints info to user
            MessageBox.Show($"City '{cityName}' has been removed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Clear the input field
            textBox7.Clear();

            // Populate Ciudad de Inicio ComboBox
            PopulateComboBox3();
            PopulateComboBox4();
        }// end button 5 bajas ciudad 



        // start button 6 cambio datos de ciudad
        private void button6_Click(object sender, EventArgs e)
        {
            // Get input values
            string oldCityName = textBox7.Text.Trim(); 
            string newCityName = textBox11.Text.Trim(); 
            int newXCoordinate = (int)numericUpDown1.Value; 
            int newYCoordinate = (int)numericUpDown2.Value; 

            // Validate input
            if (string.IsNullOrEmpty(oldCityName))
            {
                MessageBox.Show("Please enter the current name of the city to modify.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!cityCoordinates.ContainsKey(oldCityName))
            {
                MessageBox.Show($"City '{oldCityName}' does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if the new name conflicts with an existing city
            if (!string.IsNullOrEmpty(newCityName) && oldCityName != newCityName && cityCoordinates.ContainsKey(newCityName))
            {
                MessageBox.Show($"City name '{newCityName}' already exists. Please choose a different name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get the current coordinates if not updating them
            Point updatedCoordinates = new Point(newXCoordinate, newYCoordinate);

            // Remove the old entry from the dictionary
            cityCoordinates.Remove(oldCityName);

            // Add the updated entry to the dictionary
            cityCoordinates[newCityName] = updatedCoordinates;

            // Update visual elements (dot and label)
            Control dotToUpdate = null;
            Control labelToUpdate = null;

            foreach (Control control in pictureBox1.Controls)
            {
                if (control.Tag != null && control.Tag.ToString() == oldCityName)
                {
                    if (control is PictureBox)
                        dotToUpdate = control; // Identify the dot
                    else if (control is Label)
                        labelToUpdate = control; // Identify the label
                }
            }

            if (dotToUpdate != null)
            {
                // Update dot position and tag
                dotToUpdate.Location = new Point(updatedCoordinates.X - 5, updatedCoordinates.Y - 5);
                dotToUpdate.Tag = newCityName;
            }

            if (labelToUpdate != null)
            {
                // Update label text, position, and tag
                labelToUpdate.Text = newCityName;
                labelToUpdate.Location = new Point(updatedCoordinates.X + 5, updatedCoordinates.Y - 8);
                labelToUpdate.Tag = newCityName;
            }

            // Prints info
            MessageBox.Show($"City '{oldCityName}' has been updated to '{newCityName}' at coordinates ({updatedCoordinates.X}, {updatedCoordinates.Y}).", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Clear the input fields after updating
            textBox7.Clear(); 
            textBox11.Clear(); 
            numericUpDown1.Value = 0; 
            numericUpDown2.Value = 0; 

        }// end button 6 cambio datos de ciudad

        private void label31_Click(object sender, EventArgs e)
        {
            //label actualizar nombre
        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            //actualizar nombre de ciudad textbox
           

        }// end ciudad tab




        //start info tab

        // method to populate Ciudad de Inicio
        private void PopulateComboBox3()
        {
            // Clear existing items in the ComboBox
            comboBox3.Items.Clear();

            // Add city names from the cityCoordinates dictionary
            foreach (var city in cityCoordinates.Keys)
            {
                comboBox3.Items.Add(city);
            }
        }

        private void PopulateComboBox4()
        {
            comboBox4.Items.Clear(); // Clear previous items

            foreach (var city in cityCoordinates.Keys)
            {
                comboBox4.Items.Add(city); // Add each city name to the ComboBox
            }
        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                selectedTransportMode = "Auto rentado";
                MessageBox.Show($"Transport Mode selected: {selectedTransportMode}", "Selection Confirmed");
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            {
                selectedTransportMode = "Transporte publico";
                MessageBox.Show($"Transport Mode selected: {selectedTransportMode}", "Selection Confirmed");
            }
        }

        private void label23_Click(object sender, EventArgs e)
        {

        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                selectedSearchCriterion = "Tiempo";
                MessageBox.Show($"Search Criterion selected: {selectedSearchCriterion}", "Selection Confirmed");
                

            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                selectedSearchCriterion = "Distancia";
                MessageBox.Show($"Search Criterion selected: {selectedSearchCriterion}", "Selection Confirmed");

            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
            {
                selectedSearchCriterion = "Costo";
                MessageBox.Show($"Search Criterion selected: {selectedSearchCriterion}", "Selection Confirmed");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string startCity = comboBox3.Text; // Ciudad de Inicio
            string endCity = comboBox4.Text;  // Ciudad donde termina

            if (string.IsNullOrEmpty(startCity) || string.IsNullOrEmpty(endCity))
            {
                MessageBox.Show("Please select both a start and an end city.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!cityGraph.ContainsKey(startCity) || !cityGraph.ContainsKey(endCity))
            {
                MessageBox.Show("One or both cities are not connected in the graph.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Find the shortest path
            var (path, distance) = FindShortestPath(startCity, endCity);

            if (distance == int.MaxValue)
            {
                MessageBox.Show("No path exists between the selected cities.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Define speeds and costs based on transport mode
            int speed = selectedTransportMode == "Auto rentado" ? 80 : 40; // Auto = 80 km/h, Public = 40 km/h
            int costPerKm = selectedTransportMode == "Auto rentado" ? 5 : 2; // Auto = 5 pesos/km, Public = 2 pesos/km

            // Calculate time and cost
            int time = (int)Math.Ceiling((double)distance / speed * 60); // Time in minutes
            int cost = distance * costPerKm; // Cost in pesos

            // Populate list boxes
            listBox1.Items.Clear(); // Transport mode
            listBox1.Items.Add($"Transport:\n {selectedTransportMode}");

            listBox2.Items.Clear(); // Start city
            listBox2.Items.Add($"Start:\n {startCity}");

            listBox3.Items.Clear(); // End city
            listBox3.Items.Add($"End:\n {endCity}");

            listBox4.Items.Clear(); // Distance
            listBox4.Items.Add($"Dist:\n {distance} km");

            listBox6.Items.Clear(); // Time
            listBox6.Items.Add($"Time:\n {time} min");

            listBox5.Items.Clear(); // Cost
            listBox5.Items.Add($"Costo:\n {cost} pesos");

            // Update Totales 
            textBox8.Text = $"{distance} km";    // Total distance
            textBox9.Text = $"{time} min";       // Total time
            textBox10.Text = $"{cost} pesos";    // Total cost
        }

        //ruta alterna button start
        private void button8_Click(object sender, EventArgs e)
        {
            string startCity = comboBox3.Text; // Ciudad de Inicio
            string endCity = comboBox4.Text;  // Ciudad donde termina

            if (string.IsNullOrEmpty(startCity) || string.IsNullOrEmpty(endCity))
            {
                MessageBox.Show("Please select both a start and an end city.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!cityGraph.ContainsKey(startCity) || !cityGraph.ContainsKey(endCity))
            {
                MessageBox.Show("One or both cities are not connected in the graph.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Find the shortest path first
            var (shortestPath, shortestDistance) = FindShortestPath(startCity, endCity);

            if (shortestDistance == int.MaxValue)
            {
                MessageBox.Show("No path exists between the selected cities.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Temporarily remove edges in the shortest path to find an alternate route
            var originalGraph = cityGraph.ToDictionary(
                entry => entry.Key,
                entry => new List<(string city, int distance, int time, int cost)>(entry.Value)
            );

            for (int i = 0; i < shortestPath.Count - 1; i++)
            {
                string cityA = shortestPath[i];
                string cityB = shortestPath[i + 1];

                cityGraph[cityA].RemoveAll(connection => connection.city == cityB);
                cityGraph[cityB].RemoveAll(connection => connection.city == cityA);
            }

            // Find an alternate route
            var (alternatePath, alternateDistance) = FindShortestPath(startCity, endCity);

            // Restore the original graph
            cityGraph = originalGraph;

            // Check if an alternate route exists
            if (alternateDistance == int.MaxValue || alternatePath.SequenceEqual(shortestPath))
            {
                MessageBox.Show("No alternate route exists between the selected cities.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Populate list boxes with alternate route information
            listBox1.Items.Clear(); // Transport mode
            listBox1.Items.Add($"Transport:\n {selectedTransportMode}");

            listBox2.Items.Clear(); // Start city
            listBox2.Items.Add($"Start:\n {startCity}");

            listBox3.Items.Clear(); // End city
            listBox3.Items.Add($"End:\n {endCity}");

            listBox4.Items.Clear(); // Distance
            listBox4.Items.Add($"Dist:\n {alternateDistance} km");

            // Calculate time and cost for alternate route
            int speed = selectedTransportMode == "Auto rentado" ? 80 : 40; // Example speeds
            int costPerKm = selectedTransportMode == "Auto rentado" ? 5 : 2; // Example costs

            int time = (int)Math.Ceiling((double)alternateDistance / speed * 60); // Time in minutes
            int cost = alternateDistance * costPerKm; // Cost in pesos

            listBox6.Items.Clear(); // Time
            listBox6.Items.Add($"Time:\n {time} min");

            listBox5.Items.Clear(); // Cost
            listBox5.Items.Add($"Costo:\n {cost} pesos");

            // Update Totals
            textBox8.Text = $"{alternateDistance} km";    // Total distance
            textBox9.Text = $"{time} min";                // Total time
            textBox10.Text = $"{cost} pesos";             // Total cost
        }

        private void label24_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label25_Click(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label26_Click(object sender, EventArgs e)
        {

        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label27_Click(object sender, EventArgs e)
        {

        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label30_Click(object sender, EventArgs e)
        {

        }

        private void listBox6_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label28_Click(object sender, EventArgs e)
        {

        }

        private void listBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label29_Click(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }
        //end info tab


        //ruta tab


        private void PopulateRouteComboBoxes()
        {
            comboBox1.Items.Clear(); // Clear items for "Ciudad de Inicio"
            comboBox2.Items.Clear(); // Clear items for "Ciudad donde termina"

            foreach (var city in cityCoordinates.Keys)
            {
                comboBox1.Items.Add(city);
                comboBox2.Items.Add(city);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Retrieve input values
            string routeName = textBox1.Text.Trim();
            string startCity = comboBox1.Text; // Ciudad de Inicio
            string endCity = comboBox2.Text;  // Ciudad donde termina
            int distance;
            int autoTime, autoCost, publicTime, publicCost;

            // Validate route name
            if (string.IsNullOrEmpty(routeName))
            {
                MessageBox.Show("Please enter the route name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate city selections
            if (string.IsNullOrEmpty(startCity) || string.IsNullOrEmpty(endCity))
            {
                MessageBox.Show("Please select both a start and an end city.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (startCity == endCity)
            {
                MessageBox.Show("Start and end cities cannot be the same.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate distance
            if (!int.TryParse(textBox2.Text.Trim(), out distance) || distance <= 0)
            {
                MessageBox.Show("Please enter a valid distance (in km).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate time and cost for auto rentado
            if (!int.TryParse(textBox3.Text.Trim(), out autoTime) || autoTime <= 0 ||
                !int.TryParse(textBox4.Text.Trim(), out autoCost) || autoCost <= 0)
            {
                MessageBox.Show("Please enter valid time and cost for auto rentado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate time and cost for transporte público
            if (!int.TryParse(textBox5.Text.Trim(), out publicTime) || publicTime <= 0 ||
                !int.TryParse(textBox6.Text.Trim(), out publicCost) || publicCost <= 0)
            {
                MessageBox.Show("Please enter valid time and cost for transporte público.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Add route to cityGraph
            if (!cityGraph.ContainsKey(startCity))
            {
                cityGraph[startCity] = new List<(string, int, int, int)>();
            }

            if (!cityGraph.ContainsKey(endCity))
            {
                cityGraph[endCity] = new List<(string, int, int, int)>();
            }

            // Add bidirectional connection
            cityGraph[startCity].Add((endCity, distance, autoTime, autoCost));
            cityGraph[endCity].Add((startCity, distance, publicTime, publicCost));

            // Provide feedback to the user
            MessageBox.Show($"Route '{routeName}' successfully added between {startCity} and {endCity}.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Clear input fields
            textBox1.Clear();
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string startCity = comboBox1.Text;
            string endCity = comboBox2.Text;

            if (string.IsNullOrWhiteSpace(startCity) || string.IsNullOrWhiteSpace(endCity))
            {
                MessageBox.Show("Please select both start and end cities to delete a route.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!cityGraph.ContainsKey(startCity) || !cityGraph.ContainsKey(endCity))
            {
                MessageBox.Show("Route does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            cityGraph[startCity].RemoveAll(route => route.city == endCity);
            cityGraph[endCity].RemoveAll(route => route.city == startCity);

            MessageBox.Show($"Route between {startCity} and {endCity} has been deleted.");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string startCity = comboBox1.Text;
            string endCity = comboBox2.Text;
            string distanceText = textBox2.Text;

            if (string.IsNullOrWhiteSpace(startCity) || string.IsNullOrWhiteSpace(endCity) || string.IsNullOrWhiteSpace(distanceText))
            {
                MessageBox.Show("Please fill in all fields to update a route.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(distanceText, out int newDistance) || newDistance <= 0)
            {
                MessageBox.Show("Please enter a valid positive distance.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!cityGraph.ContainsKey(startCity) || !cityGraph[startCity].Any(route => route.city == endCity))
            {
                MessageBox.Show("Route does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Update the route
            cityGraph[startCity].RemoveAll(route => route.city == endCity);
            cityGraph[endCity].RemoveAll(route => route.city == startCity);

            int autoTime = newDistance / 60;
            int autoCost = newDistance * 5;
            int publicTime = newDistance / 30;
            int publicCost = newDistance * 2;

            cityGraph[startCity].Add((endCity, newDistance, autoTime, autoCost));
            cityGraph[endCity].Add((startCity, newDistance, publicTime, publicCost)); // Bidirectional

            MessageBox.Show($"Route between {startCity} and {endCity} updated successfully.");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            AboutBox1 f2 = new AboutBox1();
            f2.Show();
        }
        //end ruta tab
    }

    
}

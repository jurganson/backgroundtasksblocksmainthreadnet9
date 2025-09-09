using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

namespace BackgroundTasksBlocksMainThreadNET9;

public class ListItem
{
	public string Title { get; set; } = string.Empty;
	public string Number { get; set; } = string.Empty;
	public Color Color { get; set; } = Colors.Black;
	public Color BackgroundColor { get; set; } = Colors.White;
}

// Model classes for XML serialization testing
public class SampleDataModel
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public DateTime CreatedDate { get; set; }
	public decimal Price { get; set; }
	public bool IsActive { get; set; }
	public List<string> Tags { get; set; } = new();
	public Dictionary<string, object> Properties { get; set; } = new();
	public NestedObject Details { get; set; } = new();

	public string Serialize()
	{
		var xe = new XElement("SampleData",
			new XElement("Id", Id),
			new XElement("Name", Name),
			new XElement("CreatedDate", CreatedDate.ToString("O")),
			new XElement("Price", Price),
			new XElement("IsActive", IsActive),
			new XElement("Tags", Tags.Select(t => new XElement("Tag", t))),
			new XElement("Properties", Properties.Select(p => new XElement("Property",
				new XAttribute("Key", p.Key),
				new XAttribute("Value", p.Value?.ToString() ?? "")))),
			new XElement("Details", Details.Serialize())
		);

		using (var sw = new StringWriter())
		{
			var settings = new XmlWriterSettings { CheckCharacters = false };
			using (var xw = XmlWriter.Create(sw, settings))
				xe.WriteTo(xw);
			return sw.ToString();
		}
	}
}

public class NestedObject
{
	public string Description { get; set; } = string.Empty;
	public int Priority { get; set; }
	public List<SubItem> Items { get; set; } = new();

	public XElement Serialize()
	{
		return new XElement("NestedObject",
			new XElement("Description", Description),
			new XElement("Priority", Priority),
			new XElement("Items", Items.Select(i => i.Serialize()))
		);
	}
}

public class SubItem
{
	public string Code { get; set; } = string.Empty;
	public double Value { get; set; }
	public string Category { get; set; } = string.Empty;

	public XElement Serialize()
	{
		return new XElement("SubItem",
			new XElement("Code", Code),
			new XElement("Value", Value),
			new XElement("Category", Category)
		);
	}
}

public partial class MainPage : ContentPage
{
	private bool _isAnimating = false;
	private bool _isBackgroundWorkRunning = false;
	private List<SampleDataModel> _xmlTestData = new();
	public ObservableCollection<ListItem> ListItems { get; set; }

	public MainPage()
	{
		InitializeComponent();
		SetupTestData();
		SetupXmlTestData();
		StartOrbitAnimation();
		SetInfo();
	}

	private void SetInfo()
	{
#if DEBUG
		InfoLabel.Text = "NET9 - Debug " + DeviceInfo.Current.Platform;
#else
		InfoLabel.Text = "NET9 - Release " + DeviceInfo.Current.Platform;
#endif
	}

	private void SetupTestData()
	{
		ListItems = new ObservableCollection<ListItem>();
		var colors = new[] { Colors.Red, Colors.Blue, Colors.Green, Colors.Purple, Colors.Orange, Colors.Teal, Colors.Pink, Colors.Brown };
		var adjectives = new[] { "Amazing", "Brilliant", "Creative", "Dynamic", "Elegant", "Fantastic", "Gorgeous", "Incredible", "Magnificent", "Outstanding" };
		var nouns = new[] { "Project", "Task", "Item", "Element", "Component", "Feature", "Module", "Section", "Part", "Piece" };

		var random = new Random();

		// Create 200 items for smooth scrolling test
		for (int i = 1; i <= 200; i++)
		{
			var adjective = adjectives[random.Next(adjectives.Length)];
			var noun = nouns[random.Next(nouns.Length)];
			var color = colors[random.Next(colors.Length)];

			ListItems.Add(new ListItem
			{
				Title = $"{adjective} {noun} #{i}",
				Number = $"Item {i:D3}",
				Color = color,
				BackgroundColor = i % 2 == 0 ? Colors.White : Color.FromRgba(240, 240, 240, 255)
			});
		}

		TestListView.ItemsSource = ListItems;
	}

	private void SetupXmlTestData()
	{
		_xmlTestData = new List<SampleDataModel>();
		var random = new Random();
		var categories = new[] { "Electronics", "Books", "Clothing", "Home", "Sports", "Toys", "Food", "Health" };
		var descriptions = new[] {
			"High-quality product with excellent features",
			"Premium item for discerning customers",
			"Essential everyday item",
			"Professional grade equipment",
			"Budget-friendly option",
			"Luxury premium product"
		};

		// Create 40000 complex objects for XML serialization
		for (int i = 1; i <= 40000; i++)
		{
			var model = new SampleDataModel
			{
				Id = i,
				Name = $"Product {i:D4}",
				CreatedDate = DateTime.Now.AddDays(-random.Next(0, 365)),
				Price = (decimal)(random.NextDouble() * 1000),
				IsActive = random.Next(0, 2) == 1,
				Tags = Enumerable.Range(0, random.Next(3, 8))
					.Select(_ => categories[random.Next(categories.Length)])
					.Distinct()
					.ToList()
			};

			// Add complex properties
			for (int j = 0; j < random.Next(5, 15); j++)
			{
				model.Properties[$"Property_{j}"] = random.Next(0, 3) switch
				{
					0 => random.Next(1, 1000),
					1 => $"StringValue_{random.Next(1, 100)}",
					_ => random.NextDouble() * 100
				};
			}

			// Add nested object
			model.Details = new NestedObject
			{
				Description = descriptions[random.Next(descriptions.Length)],
				Priority = random.Next(1, 11),
				Items = Enumerable.Range(0, random.Next(5, 20))
					.Select(k => new SubItem
					{
						Code = $"CODE_{random.Next(1000, 9999)}",
						Value = random.NextDouble() * 1000,
						Category = categories[random.Next(categories.Length)]
					}).ToList()
			};

			_xmlTestData.Add(model);
		}
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (!_isAnimating)
		{
			StartOrbitAnimation();
		}
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		StopAnimation();
	}

	private async void StartOrbitAnimation()
	{
		_isAnimating = true;

		while (_isAnimating)
		{
			await OrbitContainer.RotateTo(360, 2000, Easing.Linear);
			OrbitContainer.Rotation = 0; // Reset rotation to prevent overflow
		}
	}

	private void StopAnimation()
	{
		_isAnimating = false;
		OrbitContainer?.AbortAnimation("RotateTo");
	}

	private void OnStartBackgroundWorkClicked(object sender, EventArgs e)
	{
		if (_isBackgroundWorkRunning)
			return;

		_isBackgroundWorkRunning = true;

		StartBackgroundWorkBtn.IsEnabled = false;
		StatusLabel.Text = "XML Serialization Running - Watch for Jitter!";
		StatusLabel.TextColor = Colors.Red;

		Task.Run(() =>
		{
			var result = StartXmlSerializationWork();
			MainThread.BeginInvokeOnMainThread(() =>
			{
				_isBackgroundWorkRunning = false;
				StartBackgroundWorkBtn.IsEnabled = true;
				StatusLabel.Text = $"XML Serialization Completed\nSerialized {_xmlTestData.Count} objects with length: {result.Item2}\nTook {result.Item1.Seconds} seconds";
				StatusLabel.TextColor = Colors.Green;
			});
		});
	}

	private (TimeSpan, int) StartXmlSerializationWork()
	{
		var sw = Stopwatch.StartNew();

		var xmlResults = _xmlTestData.Select(x => x.Serialize()).ToList();
		var totalLength = xmlResults.Sum(xml => xml.Length);

		sw.Stop();

		System.Diagnostics.Debug.WriteLine($"Serialized {_xmlTestData.Count} objects to XML. Total length: {totalLength} characters.");
		System.Diagnostics.Debug.WriteLine($"Serialization took {sw.Elapsed.Seconds} seconds");

		return (sw.Elapsed, totalLength);
	}
}
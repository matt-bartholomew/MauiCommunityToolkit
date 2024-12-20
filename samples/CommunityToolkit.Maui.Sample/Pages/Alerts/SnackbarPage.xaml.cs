﻿using System.Reflection;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Sample.Resources.Fonts;
using CommunityToolkit.Maui.Sample.ViewModels.Alerts;
using CommunityToolkit.Mvvm.Input;
using Font = Microsoft.Maui.Font;

namespace CommunityToolkit.Maui.Sample.Pages.Alerts;

public partial class SnackbarPage : BasePage<SnackbarViewModel>
{
	const string displayCustomSnackbarText = "Display a Custom Snackbar, Anchored to this Button";
	const string dismissCustomSnackbarText = "Dismiss Custom Snackbar";
	readonly IReadOnlyList<Color> colors = typeof(Colors)
											.GetFields(BindingFlags.Static | BindingFlags.Public)
											.ToDictionary(c => c.Name, c => (Color)(c.GetValue(null) ?? throw new InvalidOperationException()))
											.Values.ToList();

	ISnackbar? customSnackbar;

	public SnackbarPage(SnackbarViewModel snackbarViewModel) : base(snackbarViewModel)
	{
		InitializeComponent();

		DisplayCustomSnackbarButton.Text = displayCustomSnackbarText;

		Snackbar.Shown += Snackbar_Shown;
		Snackbar.Dismissed += Snackbar_Dismissed;
	}

	async void DisplayDefaultSnackbarButtonClicked(object? sender, EventArgs args) =>
		await this.DisplaySnackbar("This is a Snackbar.\nIt will disappear in 3 seconds.\nOr click OK to dismiss immediately");

	async void DisplayCustomSnackbarButtonClicked(object? sender, EventArgs args)
	{
		if (DisplayCustomSnackbarButton.Text is displayCustomSnackbarText)
		{
			var options = new SnackbarOptions
			{
				BackgroundColor = Colors.Red,
				TextColor = Colors.Green,
				ActionButtonTextColor = Colors.Yellow,
				CornerRadius = new CornerRadius(10),
				Font = Font.SystemFontOfSize(14),
				ActionButtonFont = Font.OfSize(FontFamilies.FontAwesomeBrands, 16, enableScaling: false),
			};

			customSnackbar = Snackbar.Make(
				"This is a customized Snackbar",
				async () =>
				{
					await DisplayCustomSnackbarButton.BackgroundColorTo(colors[Random.Shared.Next(colors.Count)], length: 500);
					DisplayCustomSnackbarButton.Text = displayCustomSnackbarText;
				},
				FontAwesomeIcons.Microsoft,
				TimeSpan.FromSeconds(30),
				options,
				DisplayCustomSnackbarButton);

			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
			await customSnackbar.Show(cts.Token);

			DisplayCustomSnackbarButton.Text = dismissCustomSnackbarText;
		}
		else if (DisplayCustomSnackbarButton.Text is dismissCustomSnackbarText)
		{
			if (customSnackbar is not null)
			{
				var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
				await customSnackbar.Dismiss(cts.Token);

				customSnackbar.Dispose();
			}

			DisplayCustomSnackbarButton.Text = displayCustomSnackbarText;
		}
		else
		{
			throw new NotSupportedException($"{nameof(DisplayCustomSnackbarButton)}.{nameof(ITextButton.Text)} Not Recognized");
		}
	}

	void Snackbar_Dismissed(object? sender, EventArgs e)
	{
		SnackbarShownStatus.Text = $"Snackbar dismissed. Snackbar.IsShown={Snackbar.IsShown}";
	}

	void Snackbar_Shown(object? sender, EventArgs e)
	{
		SnackbarShownStatus.Text = $"Snackbar shown. Snackbar.IsShown={Snackbar.IsShown}";
	}

	async void DisplaySnackbarInModalButtonClicked(object? sender, EventArgs e)
	{
		if (Application.Current?.Windows[0].Page is Page mainPage)
		{
			await mainPage.Navigation.PushModalAsync(new ContentPage
			{
				Content = new VerticalStackLayout
				{
					Spacing = 12,

					Children =
					{
						new Button { Command = new AsyncRelayCommand(static token => Snackbar.Make("Snackbar in a Modal MainPage").Show(token)) }
							.Top().CenterHorizontal()
							.Text("Display Snackbar"),

						new Label()
							.Center().TextCenter()
							.Text("This is a Modal MainPage"),

						new Button { Command = new AsyncRelayCommand(mainPage.Navigation.PopModalAsync) }
							.Bottom().CenterHorizontal()
							.Text("Back to Snackbar MainPage")
					}
				}.Center()
			}.Padding(12));
		}
	}
}
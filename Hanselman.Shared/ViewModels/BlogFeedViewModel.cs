﻿using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace Hanselman.Shared
{
	public class BlogFeedViewModel : BaseViewModel
	{
		public BlogFeedViewModel ()
		{
			Title = "Blog";
			Icon = "blog.png";
		}


        private ObservableCollection<BlogFeedItem> feedItems = new ObservableCollection<BlogFeedItem>();

		/// <summary>
		/// gets or sets the feed items
		/// </summary>
        public ObservableCollection<BlogFeedItem> FeedItems
		{
			get { return feedItems; }
			set { feedItems = value; OnPropertyChanged("FeedItems"); }
		}

        private BlogFeedItem selectedFeedItem;
		/// <summary>
		/// Gets or sets the selected feed item
		/// </summary>
        public BlogFeedItem SelectedFeedItem
		{
			get{ return selectedFeedItem; }
			set
			{
				selectedFeedItem = value;
				OnPropertyChanged("SelectedFeedItem");
			}
		}

		private Command loadItemsCommand;
		/// <summary>
		/// Command to load/refresh items
		/// </summary>
		public Command LoadItemsCommand
		{
			get { return loadItemsCommand ?? (loadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand())); }
		}

		private async Task ExecuteLoadItemsCommand()
		{
			if (IsBusy)
				return;

			IsBusy = true;

			try{
				var httpClient = new HttpClient();
				var feed = "http://feeds.hanselman.com/ScottHanselman";
				var responseString = await httpClient.GetStringAsync(feed);

				FeedItems.Clear();
				var items = await ParseFeed(responseString);
				foreach (var item in items)
				{
					FeedItems.Add(item);
				}
			} catch (Exception ex) {
				var page = new ContentPage();
				var result = page.DisplayAlert ("Error", "Unable to load blog.", "OK", null);
			}

			IsBusy = false;
		}



		/// <summary>
		/// Parse the RSS Feed
		/// </summary>
		/// <param name="rss"></param>
		/// <returns></returns>
        private async Task<List<BlogFeedItem>> ParseFeed(string rss)
		{
			return await Task.Run(() =>
				{
					var xdoc = XDocument.Parse(rss);
					var id = 0;
					return (from item in xdoc.Descendants("item")
                        select new BlogFeedItem
						{
							Title = (string)item.Element("title"),
							Description = (string)item.Element("description"),
							Link = (string)item.Element("link"),
							PublishDate = (string)item.Element("pubDate"),
							Category = (string)item.Element("category"),
							Id = id++
						}).ToList();
				});
		}

		/// <summary>
		/// Gets a specific feed item for an Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
        public BlogFeedItem GetFeedItem(int id)
		{
			return FeedItems.FirstOrDefault(i => i.Id == id);
		}
	}
}


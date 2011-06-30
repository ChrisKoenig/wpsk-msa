﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;

namespace MySchoolApp
{
    public partial class FeedPage : PhoneApplicationPage
    {
        #region Private Variables

        WebClient feedClient;

        #endregion Private Variables

        public FeedPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            DataContext = App.ViewModel;

            if (this.State.ContainsKey("selectedLink"))
            {
                App.ViewModel.SelectedLink = this.State["selectedLink"] as Link;
            }

            setupFeed();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (App.CurrentNavigationMode != NavigationMode.Back)
            {
                this.State["selectedLink"] = App.ViewModel.SelectedLink;
            }
        }

        private void feedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                App.ViewModel.SelectedFeedItem = (Link)e.AddedItems[0];

                WebBrowserTask wbt = new WebBrowserTask();
                wbt.URL = App.ViewModel.SelectedFeedItem.Url;
                wbt.Show();
            }
        }

        private void setupFeed()
        {
            //init feedClient
            if (feedClient == null)
            {
                feedClient = new WebClient();
                feedClient.DownloadStringCompleted += feedClient_DownloadStringCompleted;
            }

            //check if network and client are available
            if (NetworkInterface.GetIsNetworkAvailable() && NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.None && !feedClient.IsBusy)
            {
                feedClient.DownloadStringAsync(new Uri(App.ViewModel.SelectedLink.Url, UriKind.Absolute));
            }
            else
            {
                //notify user
                VisualStateManager.GoToState(this, "ErrorState", true);
            }
        }

        private void feedClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                try
                {
                    //ensure FeedLinks is empty
                    App.ViewModel.FeedLinks.Clear();

                    List<Link> links = App.ViewModel.GetLinksFromFeed(e.Result);

                    links.ForEach(x => App.ViewModel.FeedLinks.Add(x));

                    //show feed items
                    VisualStateManager.GoToState(this, "CompletedState", true);
                }
                catch (Exception ex)
                {
                    VisualStateManager.GoToState(this, "ErrorState", true);
                }
            }
        }
    }
}
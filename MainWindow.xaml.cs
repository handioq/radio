﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;

using radio.Models;
using radio.Collections;
using radio.Loader;
using radio.Saver;
using radio.Sort;
using radio.Search;

using radio.DragDropListView;

namespace radio
{
    public partial class MainWindow : Window
    {
        MusicCollection musicList;
        Playlist playlist;

        ListViewDragDropManager<Song> dragMgr;
        ListViewDragDropManager<Song> dragMgr2;

        public MainWindow()
        {
            InitializeComponent();

            FileLoadParams fileLoadParams = new FileLoadParams("music_collection.xml");
            ILoader<ObservableCollection<Song>> fromFileLoader = new FromFileLoader(fileLoadParams);
            musicList = new MusicCollection(fromFileLoader.Load(), "music collection");

            ListView1.ItemsSource = musicList.Songs;
            
            durationLabel.Content = musicList.Count() + " tracks (" + musicList.getStringDuration() + ")";

            playlist = new Playlist();
            playlistListView.ItemsSource = playlist.Songs;

            //FileSaveParams fileSaveParams = new FileSaveParams("music_collection.xml", musicList.Songs);
            //IMusicCollectionSaver toFileSaver = new ToFileSaver(fileSaveParams);
            //toFileSaver.Save();

            ObservableCollection<string> searchList = new ObservableCollection<string>();
            searchList.Add("Name");
            searchList.Add("Artist");
            searchList.Add("Duration");
            searchComboBox.ItemsSource = searchList;
            searchComboBox.SelectedItem = searchList[0];

            dragMgr = new ListViewDragDropManager<Song>(this.ListView1);
            dragMgr2 = new ListViewDragDropManager<Song>(this.playlistListView);

            ListView1.DragEnter += OnListViewDragEnter;
            playlistListView.DragEnter += OnListViewDragEnter;
            ListView1.Drop += OnListViewDrop;
            playlistListView.Drop += OnListViewDrop;

            dragMgr.ShowDragAdorner = true;

        }

        private void ListView1ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();

            SortOrder order = SortOrder.Ascending;
            SortingStrategy sortingStrategy = new SortingStrategy();

            Dictionary<string, ISortingStrategy> sorts = new Dictionary<string, ISortingStrategy>();
            sorts.Add("Name", new SortByName());
            sorts.Add("Artist", new SortByArtist());
            sorts.Add("Duration", new SortByDuration());

            try
            {
                sortingStrategy.SetStrategy(sorts[sortBy], musicList.Songs, order);
            }
            catch (KeyNotFoundException)
            {
                
            }

            sortingStrategy.Sort();

        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (searchBox.Text != "")
                {
                    SearchingStrategy searchingStrategy = new SearchingStrategy();

                    Dictionary<string, ISearchingStrategy<Song>> searchs = new Dictionary<string, ISearchingStrategy<Song>>();
                    searchs.Add("Name", new SearchByName());

                    SearchParams searchParams = new SearchParams(searchBox.Text);
                    try
                    {
                        searchingStrategy.SetStrategy(searchs[searchComboBox.Text], musicList.Songs, searchParams);
                    }
                    catch (KeyNotFoundException keyNotFoundEx) { }
                    
                    ListView1.ItemsSource = searchingStrategy.Search();
                }
                else
                {
                    ListView1.ItemsSource = musicList.Songs;
                }
            }
            catch (NullReferenceException ex) { }
            
        }


        //
        #region dragMgr_ProcessDrop

        void dragMgr_ProcessDrop( object sender, ProcessDropEventArgs<Song> e )
		{
			// This shows how to customize the behavior of a drop.
			// Here we perform a swap, instead of just moving the dropped item.

			int higherIdx = Math.Max( e.OldIndex, e.NewIndex );
			int lowerIdx = Math.Min( e.OldIndex, e.NewIndex );

			if( lowerIdx < 0 )
			{
				// The item came from the lower ListView
				// so just insert it.
				e.ItemsSource.Insert( higherIdx, e.DataItem );
			}
			else
			{
				// null values will cause an error when calling Move.
				// It looks like a bug in ObservableCollection to me.
				if( e.ItemsSource[lowerIdx] == null ||
					e.ItemsSource[higherIdx] == null )
					return;

				// The item came from the ListView into which
				// it was dropped, so swap it with the item
				// at the target index.
				e.ItemsSource.Move( lowerIdx, higherIdx );
				e.ItemsSource.Move( higherIdx - 1, lowerIdx );
			}

			// Set this to 'Move' so that the OnListViewDrop knows to 
			// remove the item from the other ListView.
			e.Effects = DragDropEffects.Move;
		}

		#endregion // dragMgr_ProcessDrop

		#region OnListViewDragEnter

		// Handles the DragEnter event for both ListViews.
		void OnListViewDragEnter( object sender, DragEventArgs e )
		{
			e.Effects = DragDropEffects.Move;
		}

		#endregion // OnListViewDragEnter

		#region OnListViewDrop

		// Handles the Drop event for both ListViews.
		void OnListViewDrop( object sender, DragEventArgs e )
		{
			if( e.Effects == DragDropEffects.None )
				return;

			Song task = e.Data.GetData( typeof( Song ) ) as Song;
			if( sender == this.ListView1 )
			{
				if( this.dragMgr.IsDragInProgress )
					return;

				// An item was dragged from the bottom ListView into the top ListView
				// so remove that item from the bottom ListView.
				(this.playlistListView.ItemsSource as ObservableCollection<Song>).Remove(task);
			}
			else
			{
				if( this.dragMgr2.IsDragInProgress )
					return;

				// An item was dragged from the top ListView into the bottom ListView
				// so remove that item from the top ListView.
				(this.ListView1.ItemsSource as ObservableCollection<Song>).Remove(task);
			}
		}

		#endregion // OnListViewDrop

    }
}

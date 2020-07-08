using qWPF_lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using static QBasket_demo.MainWindow;


namespace QBasket_demo
{
    /// <summary>
    /// Interaction logic for CheckoutWindow.xaml
    /// </summary>
    /// 
    public partial class ConfirmItemsWin : Window
    {
        // Old - public class ConfirmItem : INotifyPropertyChanged
        public class ConfirmItem
        {
            private string _name;
            private string _title;
            private string _date;
            private bool _delete;

            public string name
            { get { return _name; } set { _name = value; } }
            public bool delete
            { get { return _delete; } set { _delete = value; } }
            public string title
            { get { return _title; } set { _title = value; } }
            public string date
            { get { return _date; } set { _date = value; } }

            public override string ToString()
            {
                return name;
            }
        }   // end confirm items class

        public List<ConfirmItem> confirmList = new List<ConfirmItem>();

        // Initialization routine
        public ConfirmItemsWin()
        {
            String str;
            double totalSize;

            InitializeComponent();

            // Output the number of items selected
            NumItems.Text = mainWin.wmts.downloadInfo.Count.ToString();

            // Output the information for the item selected
            totalSize = 0;
            foreach (WMTS.DownloadLayerInfo info in mainWin.wmts.downloadInfo)
            {
                str = info.title + "\n";
                str += "Zoom Level: " + info.zoomLvl.ToString();
                str += "\tDate: " + info.date;
                str += "\nSize: " + info.pixelWidth.ToString() + " px x "
                                  + info.pixelHeight.ToString() + " px";
                str += "  -  " + info.nMBytes.ToString("F4") + " MB";
                str += "\nExtent: " + info.bbox[0].ToString("F4") + ", " + info.bbox[1].ToString("F4") + "   "
                                    + info.bbox[2].ToString("F4") + ", " + info.bbox[3].ToString("F4");

                confirmList.Add(new ConfirmItem()
                {
                    name = str,
                    delete = false,
                    title = info.title,
                    date = info.date
                });
                totalSize += info.nMBytes;
            }
            str = totalSize.ToString("F4") + " MB";
            TotalSize.Text = str;

            // Output the list of items selected w/checkboxes
            ConfirmList.ItemsSource = confirmList;
        }   // endConfirmItemsWin


        // Return button callback
        private void Return_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Returning from confirmItems Window");

            // Hide confirm item window
            Hide();

            // If there is a sketch and layer - re-enable Select Button
            // if ((mainWin.haveSketch) && (mainWin.haveLayer))
            if ((mainWin.haveSketch) && (mainWin.haveLayer))
            {
                Debug.WriteLine("have sketch = " + mainWin.haveSketch.ToString());
                Debug.WriteLine("have layer = " + mainWin.haveLayer.ToString());
                mainWin.AOISelect.IsEnabled = true;
            }

            if (confirmList.Count > 0)
                mainWin.MainCheckoutBtn.IsEnabled = true;
            else
                mainWin.MainCheckoutBtn.IsEnabled = false;

            // Return to AOI window
            if (mainWin.aoiWin != null)
            {
                try
                {
                    mainWin.aoiWin.ShowDialog();
                    //mainWin.aoiWin.Topmost = true;
                    mainWin.aoiWin.Activate();
                }
                catch
                {
                    Debug.WriteLine(" Confirm items -aoiWin closing down error");
                }
            }
        }   // end Return_Click


        //  Quit button callback
        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Shutting down from ConfirmItems window Quit button");
            Application.Current.Shutdown();
        }   // edn Quit_Click


        // Update checkout lists and variables
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            int num2Delete = 0;

            // Get the number of items to delete
            for (int i = 0; i < confirmList.Count; i++)
                if (confirmList[i].delete == true)
                    num2Delete++;

            // If the whole list is to be deleted, then delete it
            // otherwise check items in the list one by one
            if (num2Delete == confirmList.Count)
            {
                confirmList.Clear();
                mainWin.aoiWin.CheckoutBtn.IsEnabled = false;
                Hide();
            }

            // Remove the items selected for deletion
            else
                for (int i = 0; i < confirmList.Count; i++)
                {
                    if (confirmList[i].delete)
                    {
                        confirmList.RemoveAt(i);
                        mainWin.wmts.downloadInfo.RemoveAt(i);
                    }
                }

            // Update the Confirm list items displayed
            ConfirmList.ItemsSource = null;
            ConfirmList.ItemsSource = confirmList;

            // If there are no items in the checkout list, close the window
            // and go back to the AOI window
            if (confirmList.Count < 1)
            {
                confirmList.Clear();
                ConfirmList.ItemsSource = null;
                ConfirmList.ItemsSource = confirmList;

                NumItems.Text = "0";
                TotalSize.Text = "0";
                //mainWin.Topmost = false;
                if (mainWin.aoiWin.IsVisible == false)
                    mainWin.aoiWin.ShowDialog();
                mainWin.aoiWin.Activate();
                Close();
            }

            // Update summary items
            NumItems.Text = confirmList.Count.ToString();
            double sum = 0;
            foreach (WMTS.DownloadLayerInfo item in mainWin.wmts.downloadInfo)
                sum += item.nMBytes;
            TotalSize.Text = sum.ToString("F4") + " MB";
        }   // end UpdateBtn_Click


        // Download event button callback
        private void Download_Click(object sender, RoutedEventArgs e)
        {
            // Hide window for now - will be cleanup when recalled from AOI win
            Hide();
            // Process the confirmed layers
            // Show format dialog
            OutputFormatWindow formatWin = new OutputFormatWindow();
            formatWin.ShowDialog();
            //  Close(); CONFIRM WIN
        }   // end Download_Click


        private void QBasketConfirmItems_Closing(object sender, CancelEventArgs e)
        {

        }
    }   // end ConfirmItems window class

}

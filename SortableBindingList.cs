using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// <------------------------------------------------ skopiowane z naszej laborki o gridview - niedziała
namespace WindowsFormsApplication1
{
    class SortableBindingList<T> : BindingList<T>
    {
        private List<String> tabels;


        public class PropertyComparer<T> : IComparer<T>
        {
            PropertyDescriptor property;
            private ListSortDirection direction;

            public PropertyComparer(PropertyDescriptor property)
            {
                this.property = property;
            }

            public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
            {
                // TODO: Complete member initialization
                this.property = property;
                this.direction = direction;
            }

            public int Compare(T x, T y)
            {
                return (property.GetValue(x) as IComparable).CompareTo(property.GetValue(y));
            }
        }

        public SortableBindingList()
        {

        }

        //public SortableBindingList(List<Car> myCars)
        //{
        //    // TODO: Complete member initialization
        //    this.myCars = myCars;
        //}

        public SortableBindingList(List<T> list)
            : base(list)
        {

        }

        protected override bool SupportsSortingCore
        {
            get { return true; }
        }

        protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
        {
            if (property.PropertyType.GetInterface("IComparable") != null)
            {
                List<T> items = this.Items as List<T>;

                // Apply and set the sort, if items to sort
                if (items != null)
                {
                    PropertyComparer<T> pc = new PropertyComparer<T>(property, direction); //wywalic direction
                    items.Sort(pc);
                }

                // Let bound controls know they should refresh their views
                this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));

            }
        }

        protected override bool SupportsSearchingCore
        {
            get { return true; }
        }

        protected override int FindCore(PropertyDescriptor property, object key)
        {
            // Specify search columns
            if (property == null) return -1;

            // Get list to search
            List<T> items = this.Items as List<T>;

            // Traverse list for value
            foreach (T item in items)
            {
                if (property.PropertyType == typeof(string))
                {
                    // Test column search value
                    string value = (string)property.GetValue(item);

                    // If value is the search value, return the 
                    // index of the data item
                    if ((string)key == value) return IndexOf(item);
                }
                if (property.PropertyType == typeof(Int32))
                {
                    //Int32 value = (Int32)property.GetValue(item);
                    //if ((Int32)key == value) return IndexOf(item);
                    if ((property.GetValue(item) as Int32?).Equals(Int32.Parse((string)key)))
                    {
                        return IndexOf(item);
                    }
                }


            }
            return -1;

        }


    }
}

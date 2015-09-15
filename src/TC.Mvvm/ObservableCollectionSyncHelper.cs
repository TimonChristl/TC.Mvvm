using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

	public static class ObservableCollectionSyncHelper
	{

		public static void Sync<TSource, TDest>(
			ObservableCollection<TSource> sources,
			ObservableCollection<TDest> dests,
			NotifyCollectionChangedEventArgs e, 
			Func<TSource, TDest> createDestFromSource,
			Func<TDest, TSource> getSourceFromDest,
			Action<TSource, TDest> destroyDest = null)
			where TSource : class
			where TDest : class
		{
			switch(e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach(TSource source in e.NewItems)
						dests.Add(createDestFromSource(source));
					break;
				case NotifyCollectionChangedAction.Move:
                    //TODO: untested when more than one item is moved, but the stock ObservableCollection never fires such events anyway
                    for(int i = 0; i < e.NewItems.Count; i++)
                        dests.Move(e.OldStartingIndex + i, e.NewStartingIndex + i);
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach(TSource source in e.OldItems)
					{
						foreach(TDest dest in dests)
							if(getSourceFromDest(dest) == source)
							{
								if(destroyDest != null)
									destroyDest(source, dest);
								dests.Remove(dest);
								break;
							}
					}
					break;
				case NotifyCollectionChangedAction.Replace:
					foreach(TSource source in e.OldItems)
					{
						foreach(TDest dest in dests)
							if(getSourceFromDest(dest) == source)
							{
								if(destroyDest != null)
									destroyDest(source, dest);
								dests.Remove(dest);
								break;
							}
					}

					foreach(TSource source in e.NewItems)
						dests.Add(createDestFromSource(source));
					break;
				case NotifyCollectionChangedAction.Reset:
					if(destroyDest != null)
						foreach(TDest dest in dests)
						{
							TSource source = getSourceFromDest(dest);
							destroyDest(source, dest);
						}

					dests.Clear();
					foreach(TSource source in sources)
						dests.Add(createDestFromSource(source));
					break;
				default:
					throw new NotImplementedException("Unhandled NotifyCollectionChangedAction enum value");
			}
		}

	}

}

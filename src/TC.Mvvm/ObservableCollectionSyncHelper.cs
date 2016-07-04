using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

    /// <summary>
    /// Utilities for applying changes to one <see cref="ObservableCollection{T}"/> to another <see cref="ObservableCollection{T}"/>.
    /// The intended use case is synchronizing an observable collection of models and an observable collection of view models that encapsulate these models.
    /// </summary>
	public static class ObservableCollectionSyncHelper
    {

        /// <summary>
        /// Synchronizes the collection change described by <paramref name="e"/> to the observable collection <paramref name="sources"/> to the
        /// observable collection <paramref name="dests"/>. The caller has to supply delegate <paramref name="createDestFromSource"/>
        /// to create a destination item based on a source, a delegate <paramref name="getSourceFromDest"/> to map a destination item back to
        /// a source item, and optionally also a delegate <paramref name="destroyDest"/> that allows cleanup when a destination object is destroyed.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDest"></typeparam>
        /// <param name="sources"></param>
        /// <param name="dests"></param>
        /// <param name="e"></param>
        /// <param name="createDestFromSource"></param>
        /// <param name="getSourceFromDest"></param>
        /// <param name="destroyDest"></param>
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
                    for(int i = 0; i < e.NewItems.Count; i++)
                    {
                        TSource source = (TSource)e.NewItems[i];
                        dests.Insert(e.NewStartingIndex + i, createDestFromSource(source));
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    // Untested when more than one item is moved, but the stock ObservableCollection never fires such events anyway
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

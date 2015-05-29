using UnityEngine;
using System.Collections;
using System;

public class Heap<T> where T : IHeapItem<T> {

	T[] items;
	int itemsCount;

	public Heap(int maxCount)
	{
		items = new T[maxCount];
	}

	public void Add(T item)
	{
		item.heapIndex = itemsCount;
		items[itemsCount] = item;
		CompareUp(item);
		itemsCount++;
	}

	public T RemoveFirst()
	{
		T firstItem = items[0];
		itemsCount--;
		items[0] = items[itemsCount];
		items[0].heapIndex = 0;
		CompareDown(items[0]);
		return firstItem;
	}

	public bool Contains(T item)
	{
		return Equals(items[item.heapIndex], item);
	}

	public int Count {
		get{
			return itemsCount;
		}
	}

	public void UpdateItem(T item)
	{
		CompareUp(item);
	}



	void CompareDown(T item)
	{
		while (true)
		{
			int childA = item.heapIndex * 2 + 1;
			int childB = item.heapIndex * 2 + 2;
			int biggest = 0;
			if (childA < itemsCount)
			{
				biggest = childA;
				if (childB < itemsCount)
				{
					if(items[childA].CompareTo(items[childB]) < 0){
						biggest = childB;
					}
				}

				if (item.CompareTo(items[biggest]) < 0)
				{
					Swap(item, items[biggest]);
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}
	}

	void CompareUp(T item)
	{
		int parentIndex = (item.heapIndex-1)/2;

		while (true)
		{
			T parentItem = items[parentIndex];
			if  (item.CompareTo(parentItem) > 0 )
			{
				Swap(item, parentItem);
			}
			else
				break;
			parentIndex = (item.heapIndex-1)/2;
		}
		
	}

	void Swap(T item1, T item2)
	{
		int tempIndex = item1.heapIndex;
		items[item1.heapIndex] = item2;
		items[item2.heapIndex] = item1;
		item1.heapIndex = item2.heapIndex;
		item2.heapIndex = tempIndex;

	}


}

public interface IHeapItem<T> : IComparable<T>
{
	int heapIndex {
		get;
		set;
	}
}

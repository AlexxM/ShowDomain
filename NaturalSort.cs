/*
 * Сделано в SharpDevelop.
 * Пользователь: 055makarov
 * Дата: 25.04.2014
 * Время: 14:42
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
namespace ShowDomain
{
	/// <summary>
	/// Description of NaturalSort.
	/// </summary>
	public class NaturalSort : IComparer<string>
	{
		private char[] _splitBuffer = new char[256];
		
		public int Compare(string x,string y)
		{
			
			List<string> a = SplitByNumbers(x);
			List<string> b= SplitByNumbers(y);
			int aInt,bInt;
			int numToCompare = (a.Count<b.Count) ? a.Count : b.Count;
			
			for(int i=0;i<numToCompare;i++)
			{
				if(a[i].Equals(b[i]))
					continue;
				bool aIsNumber = Int32.TryParse(a[i], out aInt);
				bool bIsNumber = Int32.TryParse(b[i], out bInt);
				if(aIsNumber && bIsNumber)
				{
					return aInt.CompareTo(bInt);
				}
				else if(!aIsNumber && !bIsNumber)
				{
					return a[i].CompareTo(b[i]);
				}
				else if(aIsNumber)
				{
					return -1;
				}
				else
				{
					return 1;
				}
					
			}
			
			return a.Count.CompareTo(b.Count);
			
			
		}
		
		
		
		public List<string> SplitByNumbers(string input)
		{
			Debug.Assert(input.Length<=256);
			List<string> lst =new List<string>();
			
			int i=0,current =0;
			
			while(current<input.Length)
			{
				while(current < input.Length && char.IsDigit(input[current]))
				{
					_splitBuffer[i]=input[current];
					i++;
					current++;
				}
				
				if(i>0)
				{
					lst.Add(new String(_splitBuffer,0,i));
					i=0;
				}
				
				while(current < input.Length && !char.IsDigit(input[current]))
				{
					_splitBuffer[i]=input[current];
					i++;
					current++;	
				}
				
				if(i>0)
				{
					lst.Add(new string(_splitBuffer,0,i));
					i=0;
				}
			}
			return lst;
		}
	}
}

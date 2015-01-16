/*
 * Сделано в SharpDevelop.
 * Пользователь: 055makarov
 * Дата: 29.07.2014
 * Время: 15:32
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.DirectoryServices;
using System.Runtime.InteropServices;
namespace ShowDomain
{
	public class ShowDomain
	{
		Dictionary<string,string> argsValues;
		bool parseError=false;
		SortedDictionary<string,List<string>> usersInOU;
		public ShowDomain(string[] args)
		{
			argsValues=new Dictionary<string,string>();
			usersInOU=new SortedDictionary<string, List<string>>(new NaturalSort());
			
			if(!ParseArgs(args))
			{
				parseError=true;
				Console.WriteLine("Syntax:showdomain -d domainName [-t searchingType] [-o options,... ] [-u domainUser] [-p password]");
				Console.WriteLine("Example: -d Metall.com -t user -o \"telephoneNumber\"");
				Console.WriteLine("Valid common options: name,description,objectClass,objectCategory,distinguishedName,whenCreated,whenChanged");
				Console.WriteLine("Valid options with type user: title,telephoneNumber,mail,logonCount,memberOf");
				Console.WriteLine("Valid options with type computer: operatingSystem,operatingSystemVersion,operatingSystemServicePack,memberOf");
				Console.WriteLine("Valid options with type group: member,info");
			}
		
		}
		//добавление аргументов в словарь,проверка на корректность,т.к. некоторые типы аргументов требуют указания значений
		private bool ParseArgs(string[] args)
		{
			if(args.Length<2 || args[0]!="-d")
				return false;
			int cnt = args.Length;
			for(int i=0;i<cnt;i++)
			{
				bool haveValue=false;
				if(args[i]=="-d" || args[i]=="-t" || args[i]=="-u" || args[i]=="-p" || args[i]=="-o")
				{
					haveValue=true;
				}
				
				if(haveValue==true)
				{
					int j=i+1;
					if(j>=cnt || args[j].StartsWith("-"))
					{
						return false;
					}
					//проверить
					argsValues[args[i]]=args[j].Trim();
					i=j;
				}
				else
				{
					argsValues[args[i]]=null;
				}
			}
			return true;	
		}
		
		//преобразование в строку различных свойст доменных классов
		private string GetFormatedProperties(object o)
		{
			string outputStr = String.Empty;
			if(o==null)
			{
				outputStr="\t";
			}
			else if(o is string || o is int || o is DateTime)
			{
				outputStr=string.Format("\t {0}",o);
				return outputStr;
			}
			else if(o is IEnumerable<string>)
			{
				outputStr=string.Format("\t {0}",string.Join(",",((IEnumerable<string>)o)));
							
			}
			return outputStr;
		}
		//получения значения ключа для подразделений
		private string[] GetOUFromDistinguishedName(string dName)
		{
			if(dName==null)
				return null;
			string[] splitName = dName.Split(new char[]{','});
			string[] ouArr = splitName.Where((e)=>{ return e.StartsWith("OU=") || e.StartsWith("DC=");  }).Select((e)=>{ return e.Split(new char[]{'='})[1];}).Reverse().ToArray<string>();
			return ouArr;
		}
		
		public void ShowDomainData()
		{
			if(parseError)
				return;
			
			try{
			string user;
			string pass;
			argsValues.TryGetValue("-u",out user);
			argsValues.TryGetValue("-p",out pass);
			DirectoryEntry root;
			
			if(user!=null && pass!=null)
				root=new DirectoryEntry("LDAP://"+argsValues["-d"],user,pass);
			else
				root = new DirectoryEntry("LDAP://"+argsValues["-d"]);
			
			DirectorySearcher ds=new DirectorySearcher(root);
			if(argsValues.ContainsKey("-t"))
			{
				switch(argsValues["-t"])
				{
					case "user": ds.Filter="(&(objectCategory=user))";break;
					case "computer": ds.Filter="(&(objectCategory=computer))";break;
					case "group" : ds.Filter="(&(objectCategory=group))";break;
					default : ds.Filter="(&(objectCategory=user))";break;
				}
			}
			else
			{
				ds.Filter="(&(objectCategory=user))";
			}
				
			ds.PropertiesToLoad.Add("cn");
			string[] props=null;
			if(argsValues.ContainsKey("-o"))
			{
				props=argsValues["-o"].Split(',');
				
			}
			if(props != null && props.Length>0)
			{
				 foreach(string prop in props)
				 {
					ds.PropertiesToLoad.Add(prop);
				 }
			}
				
			SearchResultCollection result = ds.FindAll();
			foreach(SearchResult sr in result)
			{
					
				string ouToString=string.Join("/",GetOUFromDistinguishedName(sr.GetDirectoryEntry().Properties["distinguishedName"].Value as string));
					
				DirectoryEntry de = sr.GetDirectoryEntry();
			
				string formatedProp = GetFormatedProperties(de.Properties["cn"].Value);
					
				if(props != null && props.Length>0)
				{
					foreach(string prop in props)
					{
						formatedProp += GetFormatedProperties(de.Properties[prop].Value);
					}
				}
				//заполнение словаря подразделений членами подразделения
				List<string> l;
				usersInOU.TryGetValue(ouToString,out l);
				if(l==null)
				{
					usersInOU[ouToString]=new List<string>(){formatedProp};
				}
				else
				{
					usersInOU[ouToString].Add(formatedProp);
				}
					
			}
				
			string fileName="domain data_"+DateTime.Now.ToFileTime()+".txt";
			using(FileStream fs = new FileStream(fileName,FileMode.Create,FileAccess.Write))
			using(StreamWriter sw = new StreamWriter(fs))
			{
				if(argsValues.ContainsKey("-o"))
				{
					sw.WriteLine(string.Format("showed options: ({0})",argsValues["-o"]));
				}
				
				foreach(KeyValuePair<string,List<string>> kvp in usersInOU)
				{
					sw.WriteLine();
					sw.WriteLine(kvp.Key);
					sw.WriteLine();
					foreach(string userProp in kvp.Value)
					{
						sw.WriteLine(userProp);
					}
						
				}
			}
			Console.WriteLine("Complete");
			}
			catch(IOException ex)
			{
				Console.WriteLine("Ошибка при работе с файлом вывода");
			
			}
			catch(COMException ex)
			{
				Console.WriteLine("Ошибка доступа к ресурсам домена.Возможно указан неправильный логин или пароль, либо домен недоступен.");
			}
			catch(Exception ex)
			{
				Console.WriteLine("Непредвиденная ошибка");
			}
			
		}
	}
}

/*
 * Сделано в SharpDevelop.
 * Пользователь: 055makarov
 * Дата: 23.07.2014
 * Время: 16:18
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.IO;
using System.Linq;
using System.DirectoryServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.DirectoryServices.ActiveDirectory;
namespace ShowDomain
{
	class Program
	{
		public static void Main(string[] args)
		{
			//args = new string[]{"-d","Metall.com","-t","user","-o","lastLogon","-u","metall\\055makarov","-p","351785"};
			
			ShowDomain sd = new ShowDomain(args);
			sd.ShowDomainData();
				
			Console.ReadKey(true);
		}
	}
}